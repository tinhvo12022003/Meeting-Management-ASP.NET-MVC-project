using System.Text.Json;
using System.Linq;
using MeetingManagement.Config;
using MeetingManagement.Helper;
using Microsoft.AspNetCore.Authorization;

namespace MeetingManagement.Handler;
public class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly UserHelper _userHelper;

    public PermissionAuthorizationHandler(UserHelper userHelper)
    {
        _userHelper = userHelper;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var hasPermission = await _userHelper.HasPermission(requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
