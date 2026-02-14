using MeetingManagement.Data.Context;
using MeetingManagement.Interface.IRepository;
using MeetingManagement.Models;

namespace MeetingManagement.Repository;

public class PermissionRepository : GenericRepository<PermissionModel>, IPermissionRepository
{
    private readonly ApplicationDbContext _context;
    public PermissionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
}