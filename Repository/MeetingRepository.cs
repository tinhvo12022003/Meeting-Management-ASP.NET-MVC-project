using MeetingManagement.Data.Context;
using MeetingManagement.Interface.IRepository;
using MeetingManagement.Models;

namespace MeetingManagement.Repository;

public class MeetingRepository : GenericRepository<MeetingModel>, IMeetingRepository
{
    private readonly ApplicationDbContext _context; 
    public MeetingRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
}