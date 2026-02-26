using MeetingManagement.Models;

namespace MeetingManagement.Interface.IRepository;
public interface IMeetingRepository : IGenericRepository<MeetingModel>
{
    public Task<bool> IsMeetingOverlap (DateTime StartAt, DateTime EndAt, string roomId);
    public Task<MeetingModel?> GetMeeting (DateTime StartAt, DateTime EndAt, string roomId);
}