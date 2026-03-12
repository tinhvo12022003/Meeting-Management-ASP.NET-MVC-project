using MeetingManagement.Data.Context;
using MeetingManagement.Enum;
using MeetingManagement.Interface.IRepository;
using MeetingManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingManagement.Repository;

public class UserRepository : GenericRepository<UserModel>, IUserRepository
{
    private readonly ApplicationDbContext _context;
    public UserRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<UserModel?> GetByEmail(string Email)
    {
        var query = await _context.User.FirstOrDefaultAsync(x => x.Email == Email);
        return query;
    }
    public async Task<bool> ExistsEmail(string Email)
    {
        return await _context.User.AnyAsync(x => x.Email == Email);
    }

    public async Task<UserModel?> GetByUsername(string Username)
    {
        return await _context.User.FirstOrDefaultAsync(x => x.Username == Username);
    }

    public async Task<bool> AnyActiveByDepartmentId(string departmentId)
    {
        return await _context.User.AnyAsync(x => x.DepartmentId == departmentId && x.RowStatus == RowStatus.ACTIVE);
    }
}