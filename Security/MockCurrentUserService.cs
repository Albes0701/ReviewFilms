using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ReviewFilms.Api.Interfaces;

namespace ReviewFilms.Api.Security;

public sealed class MockCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MockCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentUserId()
    {
        var principal = _httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("Current user context is not available.");

        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub")
            ?? principal.FindFirstValue("user_id");

        if (string.IsNullOrWhiteSpace(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException("Current user id claim was not found.");
        }

        return userId;
    }
}
