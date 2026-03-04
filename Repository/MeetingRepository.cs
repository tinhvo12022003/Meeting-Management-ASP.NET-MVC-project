using MeetingManagement.Data.Context;
using MeetingManagement.Enum;
using MeetingManagement.Interface.IRepository;
using MeetingManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingManagement.Repository;

public class MeetingRepository : GenericRepository<MeetingModel>, IMeetingRepository
{
    private readonly ApplicationDbContext _context; 
    public MeetingRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> IsMeetingOverlap(DateTime start, DateTime end, string roomId, string? excludeId = null)
    {
        return await _context.Meeting.AnyAsync(x => 
            x.RoomId == roomId && 
            x.StartAt < end && 
            x.EndAt > start &&
            x.RowStatus == RowStatus.ACTIVE &&
            (excludeId == null || x.Id != excludeId));
    }

    public async Task<bool> HasActiveMeetings(string roomId)
    {
        return await _context.Meeting.AnyAsync(x => x.RoomId == roomId && x.RowStatus == RowStatus.ACTIVE);
    }

    public async Task<MeetingModel?> GetMeeting (DateTime StartAt, DateTime EndAt, string RoomId)
    {
        var query = await _context.Meeting.FirstOrDefaultAsync(x => x.StartAt == StartAt && x.EndAt == EndAt && x.RoomId == RoomId);
        return query;
    }
}