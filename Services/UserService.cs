using MeetingManagement.Common;
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

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly UserHelper _helper;
    private readonly HashingLibrary _hash;

    public UserService(IUnitOfWork unitOfWork, IUserRepository userRepository, UserHelper helper, HashingLibrary hash)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _helper = helper;
        _hash = hash;
    }

    public async Task CreateUser (UserCreateModel model)
    {
        if (
            string.IsNullOrWhiteSpace(model.FullName) ||
            string.IsNullOrWhiteSpace(model.Username) ||
            string.IsNullOrWhiteSpace(model.PlainPassword)
        )
        {
            throw new Exception("Họ tên, tên đăng nhập và mật khẩu không được để trống!");
        }

        var user = new UserModel
        {
            FullName = model.FullName, 
            Address = model.Address,
            Email = model.Email,
            Phone = model.Phone,
            Birthday = model.Birthday,
            Gender = model.Gender, 
            DepartmentId = model.DepartmentId,
            CompanyId = model.CompanyId,
            Username = model.Username,
            HashPassword = _hash.HashPassword(model.PlainPassword),
            userType = model.UserType,
            CreateAt = DateTime.UtcNow,
            RowStatus = RowStatus.ACTIVE,
            CreateBy = _helper.GetCurrentUser()
        };

        await _unitOfWork.Users.Add(user);
        await _unitOfWork.CommitAsync();
    }

    public async Task UpdateUser (UserUpdateModel model)
    {
        // if (
        //     string.IsNullOrWhiteSpace(model.FullName) || 
        //     string.IsNullOrWhiteSpace(model.Address) || 
        //     string.IsNullOrWhiteSpace(model.Email) || 
        //     string.IsNullOrWhiteSpace(model.Phone) || 
        //     !model.Birthday.HasValue
        // )
        // {
        //     throw new Exception(MessageConstant.NULL_ERROR);
        // }

        var user = await _userRepository.GetById(model.Id);
        if (user == null)
        {
            throw new Exception(MessageConstant.NOT_EXISTED);
        }

        if  (user.RowStatus == RowStatus.INACTIVE)
        {
            throw new Exception(MessageConstant.INACTIVE);
        }

        user.FullName = model.FullName;
        user.Address = model.Address;
        user.Email = model.Email;
        user.Phone = model.Phone;
        user.Birthday = model.Birthday;
        user.Gender = model.Gender;
        user.CompanyId = model.CompanyId;
        user.DepartmentId = model.DepartmentId;
        user.Username = model.Username;
        user.userType = model.UserType;

        if (!string.IsNullOrWhiteSpace(model.PlainPassword))
        {
            user.HashPassword = _hash.HashPassword(model.PlainPassword);
        }

        user.UpdateAt = DateTime.UtcNow;
        user.UpdateBy = _helper.GetCurrentUser();


        await _unitOfWork.Users.Update(user);
        await _unitOfWork.CommitAsync();
    }

    

    public async Task DeleteUser (string Id)
    {
        var user = await _unitOfWork.Users.GetById(Id);
        if (user == null)
        {
            throw new Exception(MessageConstant.NOT_EXISTED); 
        }
        if (user.RowStatus == RowStatus.INACTIVE)
        {
            throw new Exception(MessageConstant.INACTIVE);
        }
        user.RowStatus = RowStatus.INACTIVE;
        user.UpdateAt = DateTime.UtcNow;
        user.UpdateBy = _helper.GetCurrentUser();
        
        await _unitOfWork.Users.Update(user);
        await _unitOfWork.CommitAsync();
    }


    public async Task<PaginatedResponse<UserViewModel>> Find(PaginatedRequest request, string? companyId = null, string? departmentId = null)
    {
        var paginatedResult = await _unitOfWork.Users.GetPaginated(
            request,
            baseFilter: x => x.RowStatus == RowStatus.ACTIVE
                && (string.IsNullOrEmpty(companyId) || x.CompanyId == companyId)
                && (string.IsNullOrEmpty(departmentId) || x.DepartmentId == departmentId),
            searchFields: "FullName,Email,Phone,Birthday,Username",
            includes: new[] { "Company", "Department" }
        );

        var viewModels = paginatedResult.Items.Select(x => new UserViewModel
        {
            Id = x.Id,
            FullName = x.FullName,
            Address = x.Address,
            Email = x.Email,
            Phone = x.Phone,
            Birthday = x.Birthday,
            userType = x.userType,
            Gender = x.Gender,
            Username = x.Username,
            CompanyName = x.Company?.Name ?? string.Empty,
            DepartmentName = x.Department?.Name ?? string.Empty
        }).ToList();

        return new PaginatedResponse<UserViewModel>
        {
            Items = viewModels,
            TotalRecords = paginatedResult.TotalRecords,
            PageNumber = paginatedResult.PageNumber,
            PageSize = paginatedResult.PageSize
        };
    }
    public async Task<UserUpdateModel?> GetUpdateModelById(string id)
    {
        var user = await _unitOfWork.Users.GetById(id);
        if (user == null) return null;

        return new UserUpdateModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Address = user.Address,
            Email = user.Email,
            Phone = user.Phone,
            Birthday = user.Birthday,
            Gender = user.Gender,
            UserType = user.userType,
            Username = user.Username,
            CompanyId = user.CompanyId,
            DepartmentId = user.DepartmentId,
        };
    }
}
