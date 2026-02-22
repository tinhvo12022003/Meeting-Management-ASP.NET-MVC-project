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

public class AccountService : IAccountService
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

    public AccountService(
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

    public async Task Register(AccountCreateModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.PlainPassword) || string.IsNullOrWhiteSpace(model.ConfirmPassword))
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }

        if (!string.Equals(model.PlainPassword, model.ConfirmPassword))
        {
            throw new Exception(MessageConstant.INVALID_PASSWORD);
        }

        var isExisted = await _accountRepository.GetByUsername(model.Username);
        if (isExisted != null)
        {
            throw new Exception(MessageConstant.ACCOUNT_EXISTED);
        }

        var account = new AccountModel
        {
            Username = model.Username,
            HashPassword = _hashing.HashPassword(model.PlainPassword),
            CreateBy = _helper.GetCurrentUser()
        };

        await _unitOfWork.Accounts.Add(account);
        await _unitOfWork.CommitAsync();

    }

    public async Task ChangePassword(AccountUpdateModel model, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.OldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }
        
        var account = await _accountRepository.GetByUsername(model.Username);
        if (account == null)
        {
            throw new Exception(MessageConstant.ACCOUNT_NOT_EXISTED);
        }

        if (!string.Equals(account.HashPassword, _hashing.HashPassword(model.OldPassword)))
        {
            throw new Exception(MessageConstant.INVALID_PASSWORD);
        }

        if (!string.Equals(newPassword, confirmPassword))
        {
            throw new Exception(MessageConstant.INVALID_PASSWORD);
        }

        account.HashPassword = _hashing.HashPassword(newPassword);
        account.UpdateAt = DateTime.UtcNow;
        account.UpdateBy = _helper.GetCurrentUser();

        await _unitOfWork.Accounts.Update(account);
        await _unitOfWork.CommitAsync();
    }



    public async Task Delete(string Id)
    {
        var account = await _accountRepository.GetById(Id);
        if (account != null)
        {
            throw new Exception(MessageConstant.ACCOUNT_NOT_EXISTED);
        }

        if (account?.RowStatus == RowStatus.ACTIVE)
        {
            account.RowStatus = RowStatus.INACTIVE;
        }
        await _unitOfWork.Accounts.Update(account);
        await _unitOfWork.CommitAsync();
    }
}