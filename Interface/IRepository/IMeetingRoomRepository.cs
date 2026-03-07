using MeetingManagement.Models;

namespace MeetingManagement.Interface.IRepository;
public interface IMeetingRoomRepository : IGenericRepository<MeetingRoomModel>
{
    public Task<MeetingRoomModel?> GetByName (string Name);
}