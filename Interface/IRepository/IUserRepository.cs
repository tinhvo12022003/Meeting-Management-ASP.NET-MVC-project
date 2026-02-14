using MeetingManagement.Models;

namespace MeetingManagement.Interface.IRepository;

public interface IUserRepository : IGenericRepository<UserModel>
{
    public Task<UserModel?> GetByEmail(string Email);
    public Task<bool> ExistsEmail(string email);
}