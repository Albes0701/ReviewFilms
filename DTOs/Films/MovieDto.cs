using ReviewFilms.Api.Entities;
using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.DTOs.Films;

public sealed class MovieDto
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? OriginalTitle { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string? Overview { get; init; }

    public DateOnly? ReleaseDate { get; init; }

    public int? RuntimeMinutes { get; init; }

    public string? AgeRating { get; init; }

    public string? OriginalLanguage { get; init; }

    public string? PosterUrl { get; init; }

    public string? BackdropUrl { get; init; }

    public string? TrailerUrl { get; init; }

    public decimal? AvgRating { get; init; }

    public int RatingCount { get; init; }

    public int CommentCount { get; init; }

    public MovieStatus Status { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }

    public Guid? CreatedByUserId { get; init; }

    public IReadOnlyCollection<MovieGenreDto> Genres { get; init; } = [];

    public IReadOnlyCollection<MovieCreditDto> Credits { get; init; } = [];

    public static MovieDto FromEntity(Movie movie)
    {
        return FromEntity(movie, includeRelations: false);
    }

    public static MovieDto FromEntity(Movie movie, bool includeRelations)
    {
        return new MovieDto
        {
            Id = movie.Id,
            Title = movie.Title,
            OriginalTitle = movie.OriginalTitle,
            Slug = movie.Slug,
            Overview = movie.Overview,
            ReleaseDate = movie.ReleaseDate,
            RuntimeMinutes = movie.RuntimeMinutes,
            AgeRating = movie.AgeRating,
            OriginalLanguage = movie.OriginalLanguage,
            PosterUrl = movie.PosterUrl,
            BackdropUrl = movie.BackdropUrl,
            TrailerUrl = movie.TrailerUrl,
            AvgRating = movie.AvgRating,
            RatingCount = movie.RatingCount,
            CommentCount = movie.CommentCount,
            Status = movie.Status,
            CreatedAt = movie.CreatedAt,
            UpdatedAt = movie.UpdatedAt,
            CreatedByUserId = movie.CreatedByUserId,
            Genres = includeRelations
                ? movie.MovieGenres
                    .Select(movieGenre => new MovieGenreDto
                    {
                        Id = movieGenre.Genre.Id,
                        Name = movieGenre.Genre.Name,
                        Slug = movieGenre.Genre.Slug
                    })
                    .ToArray()
                : [],
            Credits = includeRelations
                ? movie.MovieCredits
                    .Select(movieCredit => new MovieCreditDto
                    {
                        Id = movieCredit.Id,
                        PersonId = movieCredit.PersonId,
                        PersonName = movieCredit.Person.Name,
                        PersonOriginalName = movieCredit.Person.OriginalName,
                        CreditType = movieCredit.CreditType,
                        Department = movieCredit.Department,
                        Job = movieCredit.Job,
                        CharacterName = movieCredit.CharacterName,
                        BillingOrder = movieCredit.BillingOrder
                    })
                    .ToArray()
                : []
        };
    }
}
