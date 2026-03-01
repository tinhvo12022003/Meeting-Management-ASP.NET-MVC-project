using MeetingManagement.Common;
using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Interface.IService;

public interface IMeetingUserService
{
    Task AddMember(MeetingUserCreateModel model);
    Task AddMembers(List<MeetingUserCreateModel> models);
    Task UpdateRole(MeetingUserUpdateModel model);
    Task RemoveMember(string meetingId, string userId);
    Task RemoveMembers(string meetingId, List<string> userIds);
    Task<List<MeetingUserViewModel>> GetMeetingParticipants(string meetingId);
    Task<PaginatedResponse<MeetingUserViewModel>> GetUserMeetings(string userId, PaginatedRequest request);
}