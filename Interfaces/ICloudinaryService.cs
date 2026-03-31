using Microsoft.AspNetCore.Http;

namespace ReviewFilms.Api.Interfaces;

public interface ICloudinaryService
{
    Task<string?> UploadImageAsync(
        IFormFile? file,
        string? folder = null,
        CancellationToken cancellationToken = default);
}
