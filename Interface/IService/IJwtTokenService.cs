namespace MeetingManagement.Interface.IService;
public interface IJwtTokenService
{
    public Task<string> GenerateAccessToken(string UserId, string Username);
    public string GenerateRefreshToken();
}