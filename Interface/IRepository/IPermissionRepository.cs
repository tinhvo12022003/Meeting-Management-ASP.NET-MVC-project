using MeetingManagement.Models;

namespace MeetingManagement.Interface.IRepository;
public interface IPermissionRepository : IGenericRepository<PermissionModel>
{
    public Task<bool> IsExisted (string UserId, string Controller, string Action);
    public Task<PermissionModel?> GetPermission (string UserId, string Controller, string Action);
    public Task<List<PermissionModel>> GetByUserId(string userId);
}