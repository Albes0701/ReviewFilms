using Microsoft.AspNetCore.Authorization;

namespace ReviewFilms.Api.Security;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var hasPermission = context.User
            .FindAll(PermissionClaimTypes.Permission)
            .Select(claim => claim.Value)
            .Any(permission => string.Equals(
                permission,
                requirement.Permission,
                StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
