using MeetingManagement.Data.Context;
using MeetingManagement.Interface.IRepository;
using MeetingManagement.Models;

namespace MeetingManagement.Repository;

public class MeetingRoomRepository : GenericRepository<MeetingRoomModel>, IMeetingRoomRepository
{
    private readonly ApplicationDbContext _context; 
    public MeetingRoomRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
}