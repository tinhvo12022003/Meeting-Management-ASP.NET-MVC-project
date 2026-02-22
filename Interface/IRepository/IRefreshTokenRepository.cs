using MeetingManagement.Models;

namespace MeetingManagement.Interface.IRepository;

public interface IRefreshTokenRepository : IGenericRepository<RefreshTokenModel>
{
    public Task<IEnumerable<RefreshTokenModel>> FindAll(string accountId);
    public Task<IEnumerable<RefreshTokenModel>> GetActiveByUserId(string accountId);
    public Task<RefreshTokenModel?> GetByTokenHash(string tokenHash);
    public Task RevokeAllByUserId(string accountId);
}