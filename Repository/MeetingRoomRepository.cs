using MeetingManagement.Data.Context;
using MeetingManagement.Enum;
using MeetingManagement.Interface.IRepository;
using MeetingManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingManagement.Repository;

public class MeetingRoomRepository : GenericRepository<MeetingRoomModel>, IMeetingRoomRepository
{
    private readonly ApplicationDbContext _context; 
    public MeetingRoomRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<MeetingRoomModel?> GetByName (string Name)
    {
        return await _context.MeetingRoom.FirstOrDefaultAsync(x => x.Name == Name && x.RowStatus == RowStatus.ACTIVE);
    }

    public async Task<List<MeetingRoomModel>> GetAllActive()
    {
        return await _context.MeetingRoom
            .Where(x => x.RowStatus == RowStatus.ACTIVE)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }
}