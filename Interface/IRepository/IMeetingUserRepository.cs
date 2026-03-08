using MeetingManagement.Common;

namespace MeetingManagement.Interface.IRepository;

public interface IMeetingUserRepository : IGenericRepository<MeetingUserModel>
{
    Task<MeetingUserModel?> GetMeetingUser(string meetingId, string userId);
    Task<List<MeetingUserModel>> GetByMeetingId(string meetingId);
    Task<List<MeetingUserModel>> GetByUserId(string userId);
    Task<(List<MeetingUserModel> Items, int TotalCount)> GetPaginatedByUserId(string userId, int pageNumber, int pageSize);
    Task Delete(string meetingId, string userId);
}