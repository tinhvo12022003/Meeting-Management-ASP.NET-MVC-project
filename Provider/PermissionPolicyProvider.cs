using MeetingManagement.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
namespace MeetingManagement.Provider;
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    const string POLICY_PREFIX = "Permission:";

    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(POLICY_PREFIX))
        {
            var permission = policyName.Substring(POLICY_PREFIX.Length);

            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(permission));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => FallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => Task.FromResult<AuthorizationPolicy?>(null);
}
