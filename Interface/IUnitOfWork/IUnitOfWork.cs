using MeetingManagement.Interface.IRepository;

namespace MeetingManagement.Interface.IUnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IAccountRepository Accounts { get; }
    ICompanyRepository Companies {get; }
    IDepartmentRepository Departments {get; }
    IMeetingRepository Meetings { get; }
    IMeetingRoomRepository MeetingRooms {get; }
    IMeetingUserRepository MeetingUsers {get; }
    IPermissionRepository Permissions {get; }
    IRefreshTokenRepository RefreshTokens {get; }
    IUserRepository Users {get; }
    

    public Task<int> CommitAsync();
}