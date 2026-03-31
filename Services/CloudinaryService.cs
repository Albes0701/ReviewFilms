using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ReviewFilms.Api.Configurations;
using ReviewFilms.Api.Interfaces;

namespace ReviewFilms.Api.Services;

public sealed class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(
        IOptions<CloudinarySettings> settings,
        ILogger<CloudinaryService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        ValidateSettings();

        var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
        _cloudinary = new Cloudinary(account)
        {
            Api = { Secure = true }
        };
    }

    public async Task<string?> UploadImageAsync(
        IFormFile? file,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        var resolvedFolder = string.IsNullOrWhiteSpace(folder)
            ? _settings.DefaultFolder
            : folder;

        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = resolvedFolder,
            UseFilename = false,
            UniqueFilename = true
        };

        cancellationToken.ThrowIfCancellationRequested();
        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error is not null)
        {
            _logger.LogError(
                "Cloudinary upload failed. Error: {ErrorMessage}",
                uploadResult.Error.Message);

            throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error.Message}");
        }

        if (uploadResult.SecureUrl is not null)
        {
            return uploadResult.SecureUrl.AbsoluteUri;
        }

        if (uploadResult.Url is not null)
        {
            return uploadResult.Url.AbsoluteUri;
        }

        throw new InvalidOperationException("Cloudinary upload did not return a URL.");
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.CloudName))
        {
            throw new InvalidOperationException("Cloudinary:CloudName is required.");
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("Cloudinary:ApiKey is required.");
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiSecret))
        {
            throw new InvalidOperationException("Cloudinary:ApiSecret is required.");
        }
    }
}
