using System.Security;
using AutoMapper;
using MeetingManagement.Constant;
using MeetingManagement.Enum;
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
    public AuthService(
        IUnitOfWork unitOfWork,
        IAccountRepository accountRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IJwtTokenService jwtService,
        HashingLibrary hashing,
        IUserRepository userRepository,
        IMapper mapper
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
    }

    public async Task<AccountLoginResponse> Login(LoginDTO login)
    {
        if (string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
        {
            throw new ArgumentException(MessageConstant.EMPTY_STRING);
        }

        Console.WriteLine(_hashing.HashPassword("12345678"));

        var account = await _accountRepository.GetByUsername(login.Username);
        if (account == null)
        {
            throw new UnauthorizedAccessException(MessageConstant.ACCOUNT_NOT_EXISTED);
        }
        if (account.rowStatus == RowStatus.INACTIVE)
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
        var refreshTokenValue = await _jwtService.GenerateRefreshToken();
        var refreshToken = new RefreshTokenModel
        {
            TokenHash = _hashing.HashRefreshToken(refreshToken: refreshTokenValue),
            AccountId = account.Id,
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

        await _unitOfWork.RefreshTokens.Add(refreshToken);
        await _unitOfWork.CommitAsync();

        return new AccountLoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            User = _mapper.Map<UserViewModel>(user)
        };
    }

    public async Task<AccountLoginResponse> LoginWithToken(string refreshToken)
    {
        var hash = _hashing.HashRefreshToken(refreshToken);

        var token = await _unitOfWork.RefreshTokens.GetByHashToken(hash)
            ?? throw new SecurityException("Invalid token");

        if (token == null || token.RevokedAt != null || token.ExpiresAt < DateTime.UtcNow)
            throw new SecurityException("Invalid refresh token");

        if (token.ReplacedByToken != null)
            throw new SecurityException("Token has been reused");


        var now = DateTime.UtcNow;

        token.RevokedAt = now;

        var newRefreshPlain = await _jwtService.GenerateRefreshToken();

        var newRefreshEntity = new RefreshTokenModel
        {
            TokenHash = _hashing.HashRefreshToken(newRefreshPlain),
            AccountId = token.AccountId,
            ExpiresAt = now.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays")),
            LoginAt = now
        };

        token.ReplacedByToken = newRefreshEntity.TokenHash;

        await _unitOfWork.RefreshTokens.Update(token);
        await _unitOfWork.RefreshTokens.Add(newRefreshEntity);

        var user = await _userRepository.GetById(token.Account.UserId)
            ?? throw new Exception("User not found");
        
        var accessToken = await _jwtService.GenerateAccessToken(
            token.AccountId,
            token.Account.Username,
            token.Account.UserId
        );

        await _unitOfWork.CommitAsync();

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
        var token = await _refreshTokenRepository.GetByHashToken(hash)
            ?? throw new SecurityException("Invalid token");

        token.RevokedAt = DateTime.UtcNow;
        await _unitOfWork.RefreshTokens.Update(token);
        await _unitOfWork.CommitAsync();

    }

    public async Task RevokeAllByAccountId(string accountId)
    {
        await _unitOfWork.RefreshTokens.RevokeAllByAccountId(accountId);
    }
}