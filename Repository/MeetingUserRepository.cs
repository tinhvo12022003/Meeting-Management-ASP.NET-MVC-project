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

    /// <summary>
    /// Phân trang trực tiếp ở DB — SQL Server thực hiện Skip/Take, không load toàn bộ dữ liệu vào RAM.
    /// </summary>
    public async Task<(List<MeetingUserModel> Items, int TotalCount)> GetPaginatedByUserId(
        string userId, int pageNumber, int pageSize)
    {
        var query = _context.MeetingUser
            .Include(x => x.Meeting)
            .ThenInclude(m => m!.MeetingRoom)
            .Include(x => x.User)
            .Where(x => x.UserId == userId);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
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