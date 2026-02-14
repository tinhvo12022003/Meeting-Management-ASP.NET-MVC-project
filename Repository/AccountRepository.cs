using MeetingManagement.Data.Context;
using MeetingManagement.Interface.IRepository;
using MeetingManagement.Models;
using MeetingManagement.Repository;
using Microsoft.EntityFrameworkCore;

namespace MeetingManagement.Service;

public class AccountRepository : GenericRepository<AccountModel>, IAccountRepository
{
    private readonly ApplicationDbContext _context;
    public AccountRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<AccountModel?> GetByUsername(string Username)
    {
        return await _context.Account.FirstOrDefaultAsync(x => x.Username == Username);
    }

    public async Task<bool> Existed(string Username)
    {
        return await _context.Account.AnyAsync(x => x.Username == Username);
    }
}