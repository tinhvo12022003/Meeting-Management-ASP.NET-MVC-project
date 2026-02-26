using MeetingManagement.Data.Context;
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

    public async Task<bool> IsMeetingOverlap (DateTime StartAt, DateTime EndAt, string RoomId)
    {
        var query = await _context.Meeting.AnyAsync(x => x.StartAt == StartAt && x.EndAt == EndAt && x.RoomId == RoomId);
        return query;
    }

    public async Task<MeetingModel?> GetMeeting (DateTime StartAt, DateTime EndAt, string RoomId)
    {
        var query = await _context.Meeting.FirstOrDefaultAsync(x => x.StartAt == StartAt && x.EndAt == EndAt && x.RoomId == RoomId);
        return query;
    }
}