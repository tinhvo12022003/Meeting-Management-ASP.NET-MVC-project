using System.Security;
using MeetingManagement.Data.Context;
using MeetingManagement.Interface.IRepository;
using MeetingManagement.Interface.IUnitOfWork;
using MeetingManagement.Library;
using MeetingManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingManagement.Repository;

public class RefreshTokenRepository : GenericRepository<RefreshTokenModel>, IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;
    private readonly HashingLibrary _hash;
    public RefreshTokenRepository(ApplicationDbContext context, HashingLibrary hash) : base(context)
    {
        _context = context;
        _hash = hash;
    }

    public async Task<IEnumerable<RefreshTokenModel>> FindAll(string accountId)
    {
        return await _context.RefreshToken
            .Where(x => x.AccountId == accountId)
            .ToListAsync();
    }

    public async Task<RefreshTokenModel?> GetByAccountId (string accountId)
    {
        return await _context.RefreshToken.FirstOrDefaultAsync(x => x.AccountId == accountId);
    }

    public async Task<IEnumerable<RefreshTokenModel>> GetActiveByAccountId(string accountId)
    {
        return await _context.RefreshToken
            .Where(x => x.AccountId == accountId && x.RevokedAt == null)
            .ToListAsync();
    }

    public async Task<RefreshTokenModel?> GetByTokenHash(string tokenHash)
    {
        return await _context.RefreshToken
            .Include(x => x.Account)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
    }

    public async Task RevokeAllByAccountId(string accountId)
    {
        var now = DateTime.UtcNow;
        await _context.RefreshToken
            .Where(t => t.AccountId == accountId && t.RevokedAt == null)
            .ExecuteUpdateAsync(x => x.SetProperty(t => t.RevokedAt, now));
    }
}