using MeetingManagement.Data.Context;
using MeetingManagement.Interface.IRepository;
using MeetingManagement.Interface.IUnitOfWork;

namespace MeetingManagement.UnitOfWork; 
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    public ICompanyRepository Companies {get; }
    public IDepartmentRepository Departments {get; }
    public IMeetingRepository Meetings { get; }
    public IMeetingRoomRepository MeetingRooms {get; }
    // public IMeetingUserRepository MeetingUsers {get; }
    public IPermissionRepository Permissions {get; }
    public IRefreshTokenRepository RefreshTokens {get; }
    public IUserRepository Users {get; }

    public UnitOfWork(ApplicationDbContext context,
                      ICompanyRepository companyRepo,
                      IDepartmentRepository departmentRepo,
                      IMeetingRepository meetingRepo,
                      IMeetingRoomRepository meetingRoomRepo,
                    //   IMeetingUserRepository meetingUserRepo,
                      IPermissionRepository permissionRepo,
                      IRefreshTokenRepository refreshTokenRepo, 
                      IUserRepository userRepo
                    )
    {
        _context = context;
        Companies = companyRepo;
        Departments = departmentRepo;
        Meetings = meetingRepo;
        MeetingRooms = meetingRoomRepo;
        // MeetingUsers = meetingUserRepo;
        Permissions = permissionRepo;
        RefreshTokens = refreshTokenRepo;
        Users = userRepo;
    }
    
    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync();
    }

    private bool _disposed = false;
    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}