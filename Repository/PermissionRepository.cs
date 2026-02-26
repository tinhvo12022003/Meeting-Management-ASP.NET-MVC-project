using MeetingManagement.Data.Context;
using MeetingManagement.Interface.IRepository;
using MeetingManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingManagement.Repository;

public class PermissionRepository : GenericRepository<PermissionModel>, IPermissionRepository
{
    private readonly ApplicationDbContext _context;
    public PermissionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> IsExisted (string UserId, string Controller, string Action)
    {
        var query = await _context.Permission.AnyAsync(x => x.UserId == UserId && x.Controller == Controller && x.Action == Action);
        return query;
    }

    public async Task<PermissionModel?> GetPermission (string UserId, string Controller, string Action)
    {
        var query = await _context.Permission.FirstOrDefaultAsync(x => x.UserId == UserId && x.Controller == Controller && x.Action == Action);
        return query;
    }
}