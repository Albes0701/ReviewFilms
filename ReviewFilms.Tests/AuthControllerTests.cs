using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewFilms.Api.Controllers;
using ReviewFilms.Api.Interfaces;
using Xunit;

namespace ReviewFilms.Tests;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task Logout_uses_refresh_token_from_cookie_when_request_body_is_empty()
    {
        var authService = DispatchProxy.Create<IAuthService, RecordingAuthServiceProxy>();
        var proxy = (RecordingAuthServiceProxy)(object)authService;
        var controller = CreateController(authService);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        controller.HttpContext.Request.Headers.Cookie = "refreshToken=cookie-refresh-token";

        var requestType = typeof(AuthController).Assembly.GetType("ReviewFilms.Api.DTOs.Auth.LogoutRequest");
        var logoutMethod = typeof(AuthController).GetMethod("Logout");

        Assert.NotNull(requestType);
        Assert.NotNull(logoutMethod);

        var request = Activator.CreateInstance(requestType!);
        requestType!.GetProperty("RefreshToken")!.SetValue(request, string.Empty);

        var actionTask = Assert.IsAssignableFrom<Task>(logoutMethod!.Invoke(
            controller,
            [request!, CancellationToken.None]));

        await actionTask;

        Assert.Equal("cookie-refresh-token", proxy.CapturedRefreshToken);
    }

    [Fact]
    public void Me_endpoints_require_authorization()
    {
        var getMeMethod = typeof(AuthController).GetMethod("Me");
        var updateMeMethod = typeof(AuthController).GetMethod("UpdateMe");

        Assert.NotNull(getMeMethod);
        Assert.NotNull(updateMeMethod);
        Assert.NotNull(getMeMethod!.GetCustomAttribute<AuthorizeAttribute>());
        Assert.NotNull(updateMeMethod!.GetCustomAttribute<AuthorizeAttribute>());
    }

    private class RecordingAuthServiceProxy : DispatchProxy
    {
        public string? CapturedRefreshToken { get; private set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == "LogoutAsync")
            {
                CapturedRefreshToken = args?[0] as string;
                return Task.CompletedTask;
            }

            throw new NotSupportedException($"Method '{targetMethod?.Name}' is not supported by this proxy.");
        }
    }

    private static AuthController CreateController(IAuthService authService)
    {
        var constructor = typeof(AuthController).GetConstructors().Single();
        var arguments = constructor.GetParameters()
            .Select(parameter => parameter.ParameterType == typeof(IAuthService)
                ? (object)authService
                : new StubCurrentUserService(Guid.NewGuid()))
            .ToArray();

        return (AuthController)Activator.CreateInstance(typeof(AuthController), arguments)!;
    }

    private sealed class StubCurrentUserService : ICurrentUserService
    {
        private readonly Guid _userId;

        public StubCurrentUserService(Guid userId)
        {
            _userId = userId;
        }

        public Guid GetCurrentUserId() => _userId;
    }
}
