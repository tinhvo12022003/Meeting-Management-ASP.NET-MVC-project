using MeetingManagement.Common;
using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Interface.IService;

public interface ICompanyService
{
    public Task Create (CompanyCreateModel model);
    public Task Update (CompanyUpdateModel model);
    public Task Delete (string CompanyId);
    public Task<PaginatedResponse<CompanyViewModel>> Find(PaginatedRequest request);
    public Task<CompanyUpdateModel?> GetById(string id);
}