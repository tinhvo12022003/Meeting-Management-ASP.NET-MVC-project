using MeetingManagement.Common;
using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Interface.IService;

public interface IMeetingRoomService
{
    public Task Create (MeetingRoomCreateModel model);
    public Task Update (MeetingRoomUpdateModel model);
    public Task Delete (string Id);
    public Task<PaginatedResponse<MeetingRoomViewModel>> Find (PaginatedRequest request);
}