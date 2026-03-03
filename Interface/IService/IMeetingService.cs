using MeetingManagement.Common;
using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Interface.IService;
public interface IMeetingService
{
    public Task Create (MeetingCreateModel model);
    public Task Update (MeetingUpdateModel model);
    public Task Reschedule(string id, DateTime startAt, DateTime endAt);
    public Task Delete (string Id);
    public Task<PaginatedResponse<MeetingViewModel>> Find (PaginatedRequest request); 
}