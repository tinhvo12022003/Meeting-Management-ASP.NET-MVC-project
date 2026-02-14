using System.Text.Json;
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

        var permissions =
            JsonSerializer.Deserialize<List<string>>(permissionClaim.Value);

        if (permissions == null)
            return Task.CompletedTask;

        // Exact match
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var controller = requirement.Permission.Split('.')[0];

        if (permissions.Contains($"{controller}.*"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
