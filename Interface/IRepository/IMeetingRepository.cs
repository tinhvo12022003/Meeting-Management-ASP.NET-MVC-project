using MeetingManagement.Enum;
using MeetingManagement.Models;

namespace MeetingManagement.Interface.IRepository;
public interface IMeetingRepository : IGenericRepository<MeetingModel>
{
    public Task<bool> IsMeetingOverlap (DateTime startAt, DateTime endAt, string roomId, string? excludeId = null);
    public Task<MeetingModel?> GetMeeting (DateTime StartAt, DateTime EndAt, string roomId);
}