using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using ReviewFilms.Api.Configurations;
using ReviewFilms.Api.DTOs.Films;
using ReviewFilms.Api.Interfaces;

namespace ReviewFilms.Api.Services;

public sealed class TmdbSyncService : ITmdbSyncService
{
    private readonly HttpClient _httpClient;
    private readonly TmdbSettings _tmdbSettings;
    private readonly ILogger<TmdbSyncService> _logger;

    public TmdbSyncService(
        HttpClient httpClient,
        IOptions<TmdbSettings> tmdbSettings,
        ILogger<TmdbSyncService> logger)
    {
        _httpClient = httpClient;
        _tmdbSettings = tmdbSettings.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<TmdbGenreDto>> FetchGenresAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"/3/genre/movie/list?api_key={Uri.EscapeDataString(_tmdbSettings.ApiKey)}",
            cancellationToken);

        await EnsureSuccessAsync(response, "genre list", cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<GenreListResponse>(cancellationToken: cancellationToken);
        if (payload?.Genres is null)
        {
            throw new InvalidOperationException("TMDB genre list response is empty.");
        }

        return payload.Genres
            .Where(genre => !string.IsNullOrWhiteSpace(genre.Name))
            .Select(genre => new TmdbGenreDto
            {
                Name = genre.Name!.Trim()
            })
            .ToArray();
    }

    public async Task<TmdbMovieDetailsDto> FetchMovieDetailsAsync(int tmdbId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"/3/movie/{tmdbId}?append_to_response=credits&api_key={Uri.EscapeDataString(_tmdbSettings.ApiKey)}",
            cancellationToken);

        await EnsureSuccessAsync(response, $"movie details for {tmdbId}", cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<MovieDetailsResponse>(cancellationToken: cancellationToken);
        if (payload is null)
        {
            throw new InvalidOperationException($"TMDB movie '{tmdbId}' returned an empty response.");
        }

        return new TmdbMovieDetailsDto
        {
            Title = payload.Title?.Trim() ?? string.Empty,
            OriginalTitle = payload.OriginalTitle?.Trim(),
            Overview = payload.Overview?.Trim(),
            ReleaseDate = ParseDate(payload.ReleaseDate),
            RuntimeMinutes = payload.Runtime,
            OriginalLanguage = payload.OriginalLanguage?.Trim(),
            PosterUrl = BuildImageUrl(payload.PosterPath),
            BackdropUrl = BuildImageUrl(payload.BackdropPath),
            Genres = payload.Genres?
                .Where(genre => !string.IsNullOrWhiteSpace(genre.Name))
                .Select(genre => new TmdbGenreDto
                {
                    Name = genre.Name!.Trim()
                })
                .ToArray() ?? [],
            Cast = payload.Credits?.Cast?
                .Where(person => !string.IsNullOrWhiteSpace(person.Name))
                .Select(person => new TmdbMovieCreditPersonDto
                {
                    Name = person.Name!.Trim(),
                    OriginalName = person.OriginalName?.Trim(),
                    KnownForDepartment = person.KnownForDepartment?.Trim(),
                    ProfileUrl = BuildImageUrl(person.ProfilePath),
                    Department = person.Department?.Trim(),
                    Job = person.Job?.Trim(),
                    CharacterName = person.Character?.Trim(),
                    BillingOrder = person.Order
                })
                .ToArray() ?? [],
            Crew = payload.Credits?.Crew?
                .Where(person => !string.IsNullOrWhiteSpace(person.Name))
                .Select(person => new TmdbMovieCreditPersonDto
                {
                    Name = person.Name!.Trim(),
                    OriginalName = person.OriginalName?.Trim(),
                    KnownForDepartment = person.KnownForDepartment?.Trim(),
                    ProfileUrl = BuildImageUrl(person.ProfilePath),
                    Department = person.Department?.Trim(),
                    Job = person.Job?.Trim(),
                    CharacterName = person.Character?.Trim(),
                    BillingOrder = person.Order
                })
                .ToArray() ?? []
        };
    }

    public async Task<IReadOnlyCollection<int>> FetchPopularMovieIdsAsync(int page, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"/3/movie/popular?page={page}&api_key={Uri.EscapeDataString(_tmdbSettings.ApiKey)}",
            cancellationToken);

        await EnsureSuccessAsync(response, $"popular movies page {page}", cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<PopularMoviesResponse>(cancellationToken: cancellationToken);
        if (payload?.Results is null)
        {
            return [];
        }

        return payload.Results
            .Where(movie => movie.Id > 0)
            .Select(movie => movie.Id)
            .ToArray();
    }

    private async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string operation,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogWarning(
            "TMDB request failed for {Operation}. StatusCode: {StatusCode}. Body: {Body}",
            operation,
            (int)response.StatusCode,
            responseBody);

        throw new InvalidOperationException(
            $"TMDB request failed for {operation} with status code {(int)response.StatusCode}.");
    }

    private string? BuildImageUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return $"{_tmdbSettings.ImageBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }

    private static DateOnly? ParseDate(string? value)
    {
        return DateOnly.TryParse(value, out var parsedDate)
            ? parsedDate
            : null;
    }

    private sealed class GenreListResponse
    {
        public List<GenreItem>? Genres { get; init; }
    }

    private sealed class GenreItem
    {
        public int Id { get; init; }

        public string? Name { get; init; }
    }

    private sealed class MovieDetailsResponse
    {
        public string? Title { get; init; }

        [JsonPropertyName("original_title")]
        public string? OriginalTitle { get; init; }

        public string? Overview { get; init; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; init; }

        public int? Runtime { get; init; }

        [JsonPropertyName("original_language")]
        public string? OriginalLanguage { get; init; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; init; }

        [JsonPropertyName("backdrop_path")]
        public string? BackdropPath { get; init; }

        public List<GenreItem>? Genres { get; init; }

        public CreditsResponse? Credits { get; init; }
    }

    private sealed class CreditsResponse
    {
        public List<CreditPersonResponse>? Cast { get; init; }

        public List<CreditPersonResponse>? Crew { get; init; }
    }

    private sealed class CreditPersonResponse
    {
        public string? Name { get; init; }

        [JsonPropertyName("original_name")]
        public string? OriginalName { get; init; }

        [JsonPropertyName("known_for_department")]
        public string? KnownForDepartment { get; init; }

        [JsonPropertyName("profile_path")]
        public string? ProfilePath { get; init; }

        public string? Department { get; init; }

        public string? Job { get; init; }

        public string? Character { get; init; }

        public int? Order { get; init; }
    }

    private sealed class PopularMoviesResponse
    {
        public List<PopularMovieItem>? Results { get; init; }
    }

    private sealed class PopularMovieItem
    {
        public int Id { get; init; }
    }
}
