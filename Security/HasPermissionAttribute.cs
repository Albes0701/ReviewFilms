using Microsoft.AspNetCore.Authorization;

namespace ReviewFilms.Api.Security;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
    {
        Permission = permission;
        Policy = $"{PermissionAuthorizationPolicyProvider.PolicyPrefix}{permission}";
    }

    public string Permission { get; }
}
