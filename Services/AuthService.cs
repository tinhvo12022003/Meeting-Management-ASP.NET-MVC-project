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
        if (string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.PlainPassword))
        {
            throw new ArgumentException(MessageConstant.EMPTY_STRING);
        }

        var account = await _userRepository.GetByUsername(login.Username);

        if (account == null)
        {
            throw new UnauthorizedAccessException(MessageConstant.ACCOUNT_NOT_EXISTED);
        }
        if (account.RowStatus == RowStatus.INACTIVE)
        {
            throw new Exception(MessageConstant.ACCOUNT_DISABLE);
        }
        if (!_hashing.VerifyPassword(login.PlainPassword, account.HashPassword))
        {
            throw new UnauthorizedAccessException(MessageConstant.INVALID_PASSWORD);
        }
        
        var accessToken = await _jwtService.GenerateAccessToken(account.Id, account.Username);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();
        var newRefreshTokenHash = _hashing.HashRefreshToken(refreshTokenValue);
        var expirationDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays");

        var currentRefreshTokenCookie = _httpContextAccessor.HttpContext?.Request.Cookies["refresh_token"];
        RefreshTokenModel? existingTokenEntity = null;

        if (!string.IsNullOrEmpty(currentRefreshTokenCookie))
        {
            var oldHash = _hashing.HashRefreshToken(currentRefreshTokenCookie);
            // Tìm token cũ trong DB (kể cả đã hết hạn)
            existingTokenEntity = await _unitOfWork.RefreshTokens.GetByTokenHash(oldHash);
        }

        // KỊCH BẢN 1: Thu hồi token tĩnh cũ nếu có từ Cookie
        if (existingTokenEntity != null && existingTokenEntity.UserId == account.Id)
        {
            existingTokenEntity.RevokedAt = DateTime.UtcNow;
            existingTokenEntity.ReplacedByToken = newRefreshTokenHash;
            await _unitOfWork.RefreshTokens.Update(existingTokenEntity);
        }

        // Tạo Refresh Token mới
        var refreshTokenEntity = new RefreshTokenModel
        {
            TokenHash = newRefreshTokenHash,
            UserId = account.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            LoginAt = DateTime.UtcNow,
        };

        // Giới hạn 5 thiết bị đăng nhập cùng lúc
        var activeTokens = await _unitOfWork.RefreshTokens.GetActiveByUserId(account.Id);
        if (activeTokens.Count() >= 5)
        {
            var oldest = activeTokens.OrderBy(t => t.LoginAt).First();
            oldest.RevokedAt = DateTime.UtcNow;
            await _unitOfWork.RefreshTokens.Update(oldest);
        }

        await _unitOfWork.RefreshTokens.Add(refreshTokenEntity);

        // Housekeeping: xóa token đã hết hạn/bị thu hồi quá 30 ngày của user này
        // Dùng ExecuteDeleteAsync — xóa trực tiếp ở DB, không load vào RAM
        await _unitOfWork.RefreshTokens.PurgeExpiredByUserId(account.Id, olderThanDays: 30);

        await _unitOfWork.CommitAsync();

        return new AccountLoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            User = _mapper.Map<UserViewModel>(account)
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
            await _unitOfWork.RefreshTokens.RevokeAllByUserId(tokenData.UserId);
            throw new SecurityException("Reuse detected");
        }

        var now = DateTime.UtcNow;
        var newRefreshPlain = _jwtService.GenerateRefreshToken();
        var newRefreshHash = _hashing.HashRefreshToken(newRefreshPlain);
        var expirationDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays");

        // Thu hồi token cũ và đánh dấu đã bị thay thế
        tokenData.RevokedAt = now;
        tokenData.ReplacedByToken = newRefreshHash;
        tokenData.UpdateBy = _helper.GetCurrentUser();
        tokenData.UpdateAt = now;
        await _unitOfWork.RefreshTokens.Update(tokenData);

        // Tạo Refresh Token mới cho token rotation
        var newRefreshTokenEntity = new RefreshTokenModel
        {
            TokenHash = newRefreshHash,
            UserId = tokenData.UserId,
            ExpiresAt = now.AddDays(expirationDays),
            LoginAt = now,
            CreateBy = _helper.GetCurrentUser(),
            CreateAt = now
        };
        await _unitOfWork.RefreshTokens.Add(newRefreshTokenEntity);

        // Dọn dẹp token rác định kỳ (tránh làm đầy database)
        await _unitOfWork.RefreshTokens.PurgeExpiredByUserId(tokenData.UserId, olderThanDays: 30);

        await _unitOfWork.CommitAsync();

        // Fetch user using UserId from the token record directly (safe, no navigation property dependency)
        var user = await _userRepository.GetById(tokenData.UserId)
            ?? throw new Exception("User not found");

        var accessToken = await _jwtService.GenerateAccessToken(
            tokenData.UserId,
            user.Username
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
            await _unitOfWork.RefreshTokens.RevokeAllByUserId(token.UserId);
    }

    public async Task RevokeAllByAccountId(string UserId)
    {
        await _unitOfWork.RefreshTokens.RevokeAllByUserId(UserId);
    }
}