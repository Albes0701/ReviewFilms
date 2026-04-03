using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using ReviewFilms.Api.Controllers;
using Xunit;

namespace ReviewFilms.Tests;

public sealed class PermissionAuthorizationTests
{
    [Fact]
    public async Task Permission_policy_provider_builds_policy_for_has_permission_attribute()
    {
        var apiAssembly = typeof(AuthController).Assembly;
        var attributeType = apiAssembly.GetType("ReviewFilms.Api.Security.HasPermissionAttribute");
        var providerType = apiAssembly.GetType("ReviewFilms.Api.Security.PermissionAuthorizationPolicyProvider");
        var requirementType = apiAssembly.GetType("ReviewFilms.Api.Security.PermissionRequirement");

        Assert.NotNull(attributeType);
        Assert.NotNull(providerType);
        Assert.NotNull(requirementType);

        var attribute = Assert.IsAssignableFrom<AuthorizeAttribute>(
            Activator.CreateInstance(attributeType!, "movies:delete"));
        var provider = Assert.IsAssignableFrom<IAuthorizationPolicyProvider>(
            Activator.CreateInstance(providerType!, Options.Create(new AuthorizationOptions())));

        var policy = await provider.GetPolicyAsync(attribute.Policy!);

        Assert.NotNull(policy);
        Assert.Contains(JwtBearerDefaults.AuthenticationScheme, policy!.AuthenticationSchemes);

        var requirement = Assert.Single(policy.Requirements, requirement => requirementType!.IsInstanceOfType(requirement));
        Assert.Equal("movies:delete", requirementType!.GetProperty("Permission")!.GetValue(requirement));
    }

    [Fact]
    public async Task Permission_authorization_handler_denies_access_when_permission_claim_is_missing()
    {
        var apiAssembly = typeof(AuthController).Assembly;
        var requirementType = apiAssembly.GetType("ReviewFilms.Api.Security.PermissionRequirement");
        var handlerType = apiAssembly.GetType("ReviewFilms.Api.Security.PermissionAuthorizationHandler");

        Assert.NotNull(requirementType);
        Assert.NotNull(handlerType);

        var requirement = Assert.IsAssignableFrom<IAuthorizationRequirement>(
            Activator.CreateInstance(requirementType!, "movies:delete"));
        var handler = Assert.IsAssignableFrom<IAuthorizationHandler>(
            Activator.CreateInstance(handlerType!));
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim("permissions", "movies:create")],
                JwtBearerDefaults.AuthenticationScheme));
        var context = new AuthorizationHandlerContext([requirement], user, resource: null);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
        Assert.Contains(requirement, context.PendingRequirements);
    }

    [Fact]
    public async Task Permission_authorization_handler_succeeds_when_permission_claim_is_present()
    {
        var apiAssembly = typeof(AuthController).Assembly;
        var requirementType = apiAssembly.GetType("ReviewFilms.Api.Security.PermissionRequirement");
        var handlerType = apiAssembly.GetType("ReviewFilms.Api.Security.PermissionAuthorizationHandler");

        Assert.NotNull(requirementType);
        Assert.NotNull(handlerType);

        var requirement = Assert.IsAssignableFrom<IAuthorizationRequirement>(
            Activator.CreateInstance(requirementType!, "movies:delete"));
        var handler = Assert.IsAssignableFrom<IAuthorizationHandler>(
            Activator.CreateInstance(handlerType!));
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim("permissions", "movies:delete")],
                JwtBearerDefaults.AuthenticationScheme));
        var context = new AuthorizationHandlerContext([requirement], user, resource: null);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.Empty(context.PendingRequirements);
    }
}
