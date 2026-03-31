using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.DTOs.Films;

public sealed class MovieCreateRequest
{
    [Required]
    [StringLength(255)]
    public string Title { get; init; } = string.Empty;

    [StringLength(255)]
    public string? OriginalTitle { get; init; }

    [StringLength(255)]
    public string? Slug { get; init; }

    public string? Overview { get; init; }

    public DateOnly? ReleaseDate { get; init; }

    public int? RuntimeMinutes { get; init; }

    [StringLength(20)]
    public string? AgeRating { get; init; }

    [StringLength(10)]
    public string? OriginalLanguage { get; init; }

    [StringLength(500)]
    public string? TrailerUrl { get; init; }

    public MovieStatus Status { get; init; } = MovieStatus.Draft;

    public IFormFile? PosterFile { get; init; }

    public IFormFile? BackdropFile { get; init; }

    public List<Guid> GenreIds { get; init; } = [];
}
