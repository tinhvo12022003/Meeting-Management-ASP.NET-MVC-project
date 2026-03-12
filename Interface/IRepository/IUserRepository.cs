using MeetingManagement.Models;

namespace MeetingManagement.Interface.IRepository;

public interface IUserRepository : IGenericRepository<UserModel>
{
    public Task<UserModel?> GetByEmail(string Email);
    public Task<bool> ExistsEmail(string email);
    public Task<UserModel?> GetByUsername(string Username);
    public Task<bool> AnyActiveByDepartmentId(string departmentId);
}