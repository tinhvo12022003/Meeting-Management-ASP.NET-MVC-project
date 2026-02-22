using System.Security;
using AutoMapper;
using MeetingManagement.Constant;
using MeetingManagement.Enum;
using MeetingManagement.Helper;
using MeetingManagement.Interface.IRepository;
using MeetingManagement.Interface.IService;
using MeetingManagement.Interface.IUnitOfWork;
using MeetingManagement.Library;
using MeetingManagement.Models;
using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Service;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountRepository _accountRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtTokenService _jwtService;
    private readonly HashingLibrary _hashing;
    private readonly IMapper _mapper;
    private readonly UserHelper _helper;
    public AuthService(
        IUnitOfWork unitOfWork,
        IAccountRepository accountRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IJwtTokenService jwtService,
        HashingLibrary hashing,
        IUserRepository userRepository,
        IMapper mapper,
        UserHelper helper
        )
    {
        _unitOfWork = unitOfWork;
        _accountRepository = accountRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
        _userRepository = userRepository;
        _hashing = hashing;
        _mapper = mapper;
        _helper = helper;
    }

    public async Task<AccountLoginResponse> Login(LoginDTO login)
    {
        if (string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
        {
            throw new ArgumentException(MessageConstant.EMPTY_STRING);
        }

        var account = await _accountRepository.GetByUsername(login.Username);
        if (account == null)
        {
            throw new UnauthorizedAccessException(MessageConstant.ACCOUNT_NOT_EXISTED);
        }
        if (account.RowStatus == RowStatus.INACTIVE)
        {
            throw new Exception(MessageConstant.ACCOUNT_DISABLE);
        }
        if (!_hashing.VerifyPassword(login.Password, account.HashPassword))
        {
            throw new UnauthorizedAccessException(MessageConstant.INVALID_PASSWORD);
        }
        var user = await _userRepository.GetById(account.UserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found for this account.");
        }

        var accessToken = await _jwtService.GenerateAccessToken(account.Id, account.Username, user.Id);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();
        var newRefreshTokenHash = _hashing.HashRefreshToken(refreshTokenValue);
        var expirationDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays");

        // Kiểm tra xem browser có gửi kèm refresh token cũ không (Cookie)
        var currentRefreshTokenCookie = _httpContextAccessor.HttpContext?.Request.Cookies["refresh_token"];
        RefreshTokenModel? existingTokenEntity = null;

        if (!string.IsNullOrEmpty(currentRefreshTokenCookie))
        {
            var oldHash = _hashing.HashRefreshToken(currentRefreshTokenCookie);
            // Tìm token cũ trong DB (kể cả đã hết hạn)
            existingTokenEntity = await _unitOfWork.RefreshTokens.GetByTokenHash(oldHash);
        }

        // KỊCH BẢN 1: Tái sử dụng dòng cũ (Token Rotation) nếu token cũ thuộc về chính user này
        if (existingTokenEntity != null && existingTokenEntity.AccountId == account.Id)
        {
            existingTokenEntity.TokenHash = newRefreshTokenHash;
            existingTokenEntity.ExpiresAt = DateTime.UtcNow.AddDays(expirationDays);
            existingTokenEntity.LoginAt = DateTime.UtcNow;
            existingTokenEntity.RevokedAt = null;      // Reset trạng thái revoked
            existingTokenEntity.ReplacedByToken = null; // Reset trạng thái replaced
            
            await _unitOfWork.RefreshTokens.Update(existingTokenEntity);
        }
        // KỊCH BẢN 2: Tạo mới hoàn toàn (Máy mới hoặc Cookie đã bị xóa)
        else
        {
            var refreshTokenEntity = new RefreshTokenModel
            {
                TokenHash = newRefreshTokenHash,
                AccountId = account.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
                LoginAt = DateTime.UtcNow,
                RevokedAt = null,
                ReplacedByToken = null,
            };

            // Giới hạn 5 thiết bị đăng nhập cùng lúc
            var activeTokens = await _unitOfWork.RefreshTokens.GetActiveByAccountId(account.Id);
            if (activeTokens.Count() >= 5)
            {
                var oldest = activeTokens.OrderBy(t => t.LoginAt).First();
                oldest.RevokedAt = DateTime.UtcNow;
                await _unitOfWork.RefreshTokens.Update(oldest);
            }

            await _unitOfWork.RefreshTokens.Add(refreshTokenEntity);
        }

        await _unitOfWork.CommitAsync();

        return new AccountLoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            User = _mapper.Map<UserViewModel>(user)
        };
    }



    public async Task<AccountLoginResponse> LoginWithToken(string refreshToken)
    {
        var hash = _hashing.HashRefreshToken(refreshToken);

        var tokenData = await _unitOfWork.RefreshTokens.GetByTokenHash(hash)
            ?? throw new SecurityException("Invalid token");

        if (tokenData == null || tokenData.RevokedAt != null || tokenData.ExpiresAt < DateTime.UtcNow)
            throw new SecurityException("Invalid refresh token");

        if (tokenData.ReplacedByToken != null)
        {
            await _unitOfWork.RefreshTokens.RevokeAllByAccountId(tokenData.AccountId);
            throw new SecurityException("Reuse detected");
        }

        var now = DateTime.UtcNow;
        var newRefreshPlain = _jwtService.GenerateRefreshToken();
        var newRefreshHash = _hashing.HashRefreshToken(newRefreshPlain);
        var expirationDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays");

        // Cập nhật token hiện tại thay vì tạo mới để tránh làm đầy database
        tokenData.TokenHash = newRefreshHash;
        tokenData.ExpiresAt = now.AddDays(expirationDays);
        tokenData.LoginAt = now;
        tokenData.UpdateBy = _helper.GetCurrentUser();
        tokenData.UpdateAt = now;
        
        await _unitOfWork.RefreshTokens.Update(tokenData);
        await _unitOfWork.CommitAsync();

        // Fetch user and account information
        var account = await _unitOfWork.Accounts.GetById(tokenData.AccountId);
        var user = await _userRepository.GetById(tokenData.Account.UserId)
            ?? throw new Exception("User not found");

        var accessToken = await _jwtService.GenerateAccessToken(
            tokenData.AccountId,
            tokenData.Account.Username,
            tokenData.Account.UserId
        );

        return new AccountLoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshPlain,
            RefreshTokenExpiresAt = tokenData.ExpiresAt,
            User = _mapper.Map<UserViewModel>(user)
        };
    }


    public async Task Logout(string refreshToken)
    {
        var hash = _hashing.HashRefreshToken(refreshToken);
        var token = await _refreshTokenRepository.GetByTokenHash(hash);

        if (token != null)
            await _unitOfWork.RefreshTokens.RevokeAllByAccountId(token.AccountId);
    }

    public async Task RevokeAllByAccountId(string accountId)
    {
        await _unitOfWork.RefreshTokens.RevokeAllByAccountId(accountId);
    }
}