using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewFilms.Api.DTOs.Auth;
using ReviewFilms.Api.DTOs.Common;
using ReviewFilms.Api.Interfaces;

namespace ReviewFilms.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "refreshToken";

    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IAuthService authService, ICurrentUserService currentUserService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(response, "Registration successful."));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(response, "Login successful."));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(response, "Token refreshed successfully."));
    }

    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Logout(
        [FromBody] LogoutRequest? request,
        CancellationToken cancellationToken)
    {
        var refreshToken = !string.IsNullOrWhiteSpace(request?.RefreshToken)
            ? request.RefreshToken
            : Request.Cookies[RefreshTokenCookieName];

        await _authService.LogoutAsync(refreshToken, cancellationToken);
        return Ok(ApiResponse<object>.Ok("Logout successful."));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> Me(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        var response = await _authService.GetCurrentUserProfileAsync(userId, cancellationToken);

        return Ok(ApiResponse<UserProfileDto>.Ok(response, "User profile loaded."));
    }

    [HttpPut("me")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateMe(
        [FromForm] UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        var response = await _authService.UpdateCurrentUserProfileAsync(userId, request, cancellationToken);

        return Ok(ApiResponse<UserProfileDto>.Ok(response, "User profile updated."));
    }
}
