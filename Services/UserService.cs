using MeetingManagement.Constant;
using MeetingManagement.Enum;
using MeetingManagement.Helper;
using MeetingManagement.Interface.IRepository;
using MeetingManagement.Interface.IService;
using MeetingManagement.Interface.IUnitOfWork;
using MeetingManagement.Models;
using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Service;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly UserHelper _helper;

    public UserService(IUnitOfWork unitOfWork, IUserRepository userRepository, UserHelper helper)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _helper = helper;
    }

    public async Task CreateUser (UserCreateModel model)
    {
        if (
            string.IsNullOrWhiteSpace(model.FullName) ||
            string.IsNullOrWhiteSpace(model.Address) 
        )
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
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
            CreateAt = DateTime.UtcNow,
            CreateBy = _helper.GetCurrentUser() 
        };

        await _unitOfWork.Users.Add(user);
        await _unitOfWork.CommitAsync();
    }

    public async Task UpdateUser (UserUpdateModel model)
    {
        if (
            string.IsNullOrWhiteSpace(model.FullName) || 
            string.IsNullOrWhiteSpace(model.Address) || 
            string.IsNullOrWhiteSpace(model.Email) || 
            string.IsNullOrWhiteSpace(model.Phone) || 
            !model.Birthday.HasValue
        )
        {
            throw new Exception(MessageConstant.NULL_ERROR);
        }

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
}