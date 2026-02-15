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

        // store database
        var refreshTokenEntity = new RefreshTokenModel
        {
            TokenHash = _hashing.HashRefreshToken(refreshToken: refreshTokenValue),
            AccountId = account.Id,

            // hết hạn
            ExpiresAt = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays")),

            LoginAt = DateTime.UtcNow,
            RevokedAt = null,
            ReplacedByToken = null,
        };

        var activeTokens = await _unitOfWork.RefreshTokens.GetActiveByAccountId(account.Id);

        if (activeTokens.Count() >= 5)
        {
            var oldest = activeTokens.OrderBy(t => t.LoginAt).First();
            oldest.RevokedAt = DateTime.UtcNow;
            await _unitOfWork.RefreshTokens.Update(oldest);
        }

        await _unitOfWork.RefreshTokens.Add(refreshTokenEntity); // store
        await _unitOfWork.CommitAsync();

        return new AccountLoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt,
            User = _mapper.Map<UserViewModel>(user)
        };
    }



    public async Task<AccountLoginResponse> LoginWithToken(string refreshToken)
    {
        var hash = _hashing.HashRefreshToken(refreshToken);

        var tokenData = await _unitOfWork.RefreshTokens.GetByHashToken(hash)
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

        // Create new refresh token
        var newRefreshEntity = new RefreshTokenModel
        {
            TokenHash = newRefreshHash,
            AccountId = tokenData.AccountId,
            ExpiresAt = now.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays")),
            LoginAt = now,
            CreateBy = _helper.GetCurrentUser(),
            UpdateBy = _helper.GetCurrentUser(),
            CreateAt = now,
            UpdateAt = now,
            RowStatus = RowStatus.ACTIVE
        };

        // Add new token first
        await _unitOfWork.RefreshTokens.Add(newRefreshEntity);
        await _unitOfWork.CommitAsync();

        // Now update the old token as a separate operation
        var oldToken = await _unitOfWork.RefreshTokens.GetById(tokenData.Id);
        if (oldToken != null)
        {
            oldToken.RevokedAt = now;
            oldToken.ReplacedByToken = newRefreshHash;
            oldToken.UpdateBy = _helper.GetCurrentUser();
            oldToken.UpdateAt = now;
            await _unitOfWork.RefreshTokens.Update(oldToken);
            await _unitOfWork.CommitAsync();
        }

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
            RefreshTokenExpiresAt = newRefreshEntity.ExpiresAt,
            User = _mapper.Map<UserViewModel>(user)
        };
    }


    public async Task Logout(string refreshToken)
    {
        var hash = _hashing.HashRefreshToken(refreshToken);
        var token = await _refreshTokenRepository.GetByHashToken(hash);

        if (token != null)
            await _unitOfWork.RefreshTokens.RevokeAllByAccountId(token.AccountId);
    }

    public async Task RevokeAllByAccountId(string accountId)
    {
        await _unitOfWork.RefreshTokens.RevokeAllByAccountId(accountId);
    }
}