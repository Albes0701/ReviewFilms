using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ReviewFilms.Api.DTOs.Auth;

public sealed class UpdateUserProfileRequest
{
    [StringLength(100)]
    public string? DisplayName { get; init; }

    public string? Bio { get; init; }

    public IFormFile? AvatarFile { get; init; }
}
