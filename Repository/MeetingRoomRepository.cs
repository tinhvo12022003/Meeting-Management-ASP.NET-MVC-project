using MeetingManagement.Data.Context;
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

    public async Task<MeetingRoomModel> GetByName (string Name)
    {
        var query = await _context.MeetingRoom.FirstOrDefaultAsync(x => x.Name == Name);
        return query;
    }
}