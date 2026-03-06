using MeetingManagement.Common;
using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Interface.IService;

public interface IUserService
{
    public Task CreateUser(UserCreateModel model);
    public Task UpdateUser(UserUpdateModel model);
    public Task DeleteUser(string Id);
    public Task<PaginatedResponse<UserViewModel>> Find(PaginatedRequest request, string? companyId = null, string? departmentId = null);
    public Task<UserUpdateModel?> GetUpdateModelById(string id);
}
