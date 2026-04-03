using Microsoft.AspNetCore.Authorization;

namespace ReviewFilms.Api.Security;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            throw new ArgumentException("Permission must not be empty.", nameof(permission));
        }

        Permission = permission;
    }

    public string Permission { get; }
}
