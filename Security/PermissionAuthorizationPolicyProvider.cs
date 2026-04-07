using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ReviewFilms.Api.Security;

public sealed class PermissionAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    public const string PolicyPrefix = "Permission:";

    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName[PolicyPrefix.Length..];

            if (string.IsNullOrWhiteSpace(permission))
            {
                return Task.FromResult<AuthorizationPolicy?>(null);
            }

            var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .AddRequirements(new PermissionRequirement(permission))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }
}
