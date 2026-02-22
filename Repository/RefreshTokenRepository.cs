using MeetingManagement.Data.Context;
using MeetingManagement.Interface.IRepository;
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

    public async Task<IEnumerable<RefreshTokenModel>> FindAll(string UserId)
    {
        return await _context.RefreshToken
            .Where(x => x.UserId == UserId)
            .ToListAsync();
    }


    public async Task<IEnumerable<RefreshTokenModel>> GetActiveByUserId(string UserId)
    {
        return await _context.RefreshToken
            .Where(x => x.UserId == UserId && x.RevokedAt == null)
            .ToListAsync();
    }

    public async Task<RefreshTokenModel?> GetByTokenHash(string tokenHash)
    {
        return await _context.RefreshToken
            .Include(x => x.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
    }

    public async Task RevokeAllByUserId(string UserId)
    {
        var now = DateTime.UtcNow;
        await _context.RefreshToken
            .Where(t => t.UserId == UserId && t.RevokedAt == null)
            .ExecuteUpdateAsync(x => x.SetProperty(t => t.RevokedAt, now));
    }
}