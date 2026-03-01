using MeetingManagement.Common;
using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Interface.IService;
public interface IDepartmentService
{
    public Task Create(DepartmentCreateModel model);
    public Task Update(DepartmentUpdateModel model);
    public Task Delete(string DepartmentId);
    public Task<PaginatedResponse<DepartmentViewModel>> Find(PaginatedRequest request, string? companyId = null);
    public Task<DepartmentUpdateModel?> GetById(string id);
}