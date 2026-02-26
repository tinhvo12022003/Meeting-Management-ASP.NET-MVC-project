using MeetingManagement.Common;
using MeetingManagement.Models;
using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Interface.IService;
public interface IPermissionService
{
    public Task Create (PermissionCreateModel model);
    public Task Update (PermissionUpdateModel model);
    // public Task Delete (string Id);
    public Task<PaginatedResponse<PermissionViewModel>> Find (PaginatedRequest request); 
    public Task AddBulkPermissions(PermissionCreateBulkModel model);
}