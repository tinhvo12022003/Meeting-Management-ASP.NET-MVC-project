using MeetingManagement.Models;

namespace MeetingManagement.Interface.IRepository;
public interface  IAccountRepository : IGenericRepository<AccountModel>
{
    public Task<AccountModel?> GetByUsername(string Username);
    public Task<bool> Existed (string Username);
}
