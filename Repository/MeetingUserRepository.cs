using MeetingManagement.Data.Context;
using MeetingManagement.Interface.IRepository;

namespace MeetingManagement.Repository;

public class MeetingUserRepository : GenericRepository<MeetingUserModel>, IMeetingUserRepository
{
    private readonly ApplicationDbContext _context;
    public MeetingUserRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
}