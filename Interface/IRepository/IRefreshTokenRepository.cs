using MeetingManagement.Models;

namespace MeetingManagement.Interface.IRepository;

public interface IRefreshTokenRepository : IGenericRepository<RefreshTokenModel>
{
    public Task<IEnumerable<RefreshTokenModel>> FindAll(string accountId);
    public Task<IEnumerable<RefreshTokenModel>> GetActiveByAccountId(string accountId);
    public Task<RefreshTokenModel?> GetByHashToken(string refreshToken);
    public Task RevokeAllByAccountId(string accountId);
}