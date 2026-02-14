using System.Security.Claims;

namespace MeetingManagement.Helper;

public class UserHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public string GetCurrentUser()
    {
        var userId = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId ?? "System";
    }
}