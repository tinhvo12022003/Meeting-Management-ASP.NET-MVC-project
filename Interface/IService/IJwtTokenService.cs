namespace MeetingManagement.Interface.IService;
public interface IJwtTokenService
{
    public Task<string> GenerateAccessToken(string Id, string Username, string UserId);
    public string GenerateRefreshToken();
}