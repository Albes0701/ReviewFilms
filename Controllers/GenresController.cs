using Microsoft.AspNetCore.Mvc;
using ReviewFilms.Api.DTOs.Common;
using ReviewFilms.Api.DTOs.Films;
using ReviewFilms.Api.Interfaces;

namespace ReviewFilms.Api.Controllers;

[ApiController]
[Route("api/genres")]
public sealed class GenresController : ControllerBase
{
    private readonly IMovieService _movieService;

    public GenresController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<GenreListItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<GenreListItemDto>>>> GetGenres(
        CancellationToken cancellationToken = default)
    {
        var genres = await _movieService.GetGenresAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<GenreListItemDto>>.Ok(genres));
    }
}
