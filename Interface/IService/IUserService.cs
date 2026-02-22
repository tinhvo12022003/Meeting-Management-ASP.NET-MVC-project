using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Interface.IService;

public interface IUserService
{
    public Task CreateUser(UserCreateModel model);
    public Task UpdateUser(UserUpdateModel model);
    public Task DeleteUser(string Id);
}