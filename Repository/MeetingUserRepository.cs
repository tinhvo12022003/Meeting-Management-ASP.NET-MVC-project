using MeetingManagement.Data.Context;
using MeetingManagement.Interface.IRepository;
using Microsoft.EntityFrameworkCore;

namespace MeetingManagement.Repository;

public class MeetingUserRepository : GenericRepository<MeetingUserModel>, IMeetingUserRepository
{
    private readonly ApplicationDbContext _context;
    public MeetingUserRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<MeetingUserModel?> GetMeetingUser(string meetingId, string userId)
    {
        return await _context.MeetingUser
            .Include(x => x.User)
            .Include(x => x.Meeting)
            .FirstOrDefaultAsync(x => x.MeetingId == meetingId && x.UserId == userId);
    }

    public async Task<List<MeetingUserModel>> GetByMeetingId(string meetingId)
    {
        return await _context.MeetingUser
            .Include(x => x.User)
            .Where(x => x.MeetingId == meetingId)
            .ToListAsync();
    }

    public async Task<List<MeetingUserModel>> GetByUserId(string userId)
    {
        return await _context.MeetingUser
            .Include(x => x.Meeting)
            .ThenInclude(m => m!.MeetingRoom)
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task Delete(string meetingId, string userId)
    {
        var entity = await _context.MeetingUser.FirstOrDefaultAsync(x => x.MeetingId == meetingId && x.UserId == userId);
        if (entity != null)
        {
            _context.MeetingUser.Remove(entity);
        }
    }
}