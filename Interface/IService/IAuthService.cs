using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Interface.IService;

public interface IAuthService
{
    public Task<AccountLoginResponse> Login(LoginDTO login);
    public Task<AccountLoginResponse> LoginWithToken(string RefreshToken);
    public Task Logout(string refreshToken);
    public  Task RevokeAllByAccountId(string accountId);
    
}