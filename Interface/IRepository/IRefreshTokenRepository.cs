using MeetingManagement.Models;

namespace MeetingManagement.Interface.IRepository;

public interface IRefreshTokenRepository : IGenericRepository<RefreshTokenModel>
{
    public Task<IEnumerable<RefreshTokenModel>> FindAll(string accountId);
    public Task<RefreshTokenModel?> GetByAccountId(string accountId);
    public Task<IEnumerable<RefreshTokenModel>> GetActiveByAccountId(string accountId);
    public Task<RefreshTokenModel?> GetByTokenHash(string tokenHash);
    public Task RevokeAllByAccountId(string accountId);
}