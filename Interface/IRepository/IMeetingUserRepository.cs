namespace MeetingManagement.Interface.IRepository;

public interface IMeetingUserRepository : IGenericRepository<MeetingUserModel>
{
    Task<MeetingUserModel?> GetMeetingUser(string meetingId, string userId);
    Task<List<MeetingUserModel>> GetByMeetingId(string meetingId);
    Task<List<MeetingUserModel>> GetByUserId(string userId);
    Task Delete(string meetingId, string userId);
}