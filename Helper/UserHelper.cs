using MeetingManagement.Interface.IService;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace MeetingManagement.Helper;

public class UserHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _cache;

    public UserHelper(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider, IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
        _cache = cache;
    }
    public string GetCurrentUser()
    {
        var userId = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId ?? "System";
    }

    public async Task<bool> HasPermission(string req)
    {
        var userId = GetCurrentUser();
        if (userId == "System") return false;

        var cacheKey = $"permissions_{userId}";
        if (!_cache.TryGetValue(cacheKey, out List<string>? permissions))
        {
            using var scope = _serviceProvider.CreateScope();
            var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
            permissions = await permissionService.GetPermissionsForUser(userId);
            _cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(30)); 
        }

        if (permissions == null) return false;

        if (permissions.Contains("*.*")) return true;

        var parts = req.Split('.');
        if (parts.Length < 2) return permissions.Contains(req);

        var controller = parts[0];
        var action = parts[1];

        if (permissions.Contains($"{controller}.*")) return true;
        if (permissions.Contains($"{controller}.{action}.FullPermission")) return true;
        if (permissions.Contains(req)) return true;
            
        // Check for All permissions if looking for a specific one
        if (req.EndsWith(".Edit") && permissions.Contains($"{controller}.{action}.EditAll")) return true;
        if (req.EndsWith(".Delete") && permissions.Contains($"{controller}.{action}.DeleteAll")) return true;
        if (req.EndsWith(".Insert") && permissions.Contains($"{controller}.{action}.InsertAll")) return true;

        return false;
    }
}
