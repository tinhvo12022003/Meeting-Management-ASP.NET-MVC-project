using System.Text.Json;
using System.Linq;
using MeetingManagement.Config;
using Microsoft.AspNetCore.Authorization;

namespace MeetingManagement.Handler;
public class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissionClaim =
            context.User.FindFirst("Permissions");

        if (permissionClaim == null)
            return Task.CompletedTask;

        var permissions = JsonSerializer.Deserialize<List<string>>(permissionClaim.Value);

        var req = requirement.Permission; // Ví dụ: "Meeting.Index.View"
        var parts = req.Split('.');
        if (parts.Length < 2) return Task.CompletedTask;

        var controller = parts[0];
        var action = parts[1];

        // 1. Kiểm tra Siêu Admin (*.*)
        if (permissions.Contains("*.*"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // 2. Kiểm tra Quản trị Controller (Controller.*)
        if (permissions.Contains($"{controller}.*"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // 3. Kiểm tra Quản trị hành động cụ thể (Controller.Action.FullPermission)
        if (permissions.Contains($"{controller}.{action}.FullPermission"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // 4. Kiểm tra quyền chính xác (Exact Match)
        if (permissions.Contains(req))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
