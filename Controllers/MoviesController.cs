using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewFilms.Api.DTOs.Common;
using ReviewFilms.Api.DTOs.Films;
using ReviewFilms.Api.Enums;
using ReviewFilms.Api.Interfaces;
using ReviewFilms.Api.Security;

namespace ReviewFilms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<MovieDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<MovieDto>>>> GetMovies(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? genreId = null,
        [FromQuery] MovieStatus? status = null,
        [FromQuery] Guid? personId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _movieService.GetMoviesAsync(
            pageNumber,
            pageSize,
            search,
            genreId,
            status,
            personId,
            cancellationToken);

        return Ok(ApiResponse<PagedResult<MovieDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MovieDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<MovieDto>>> GetMovieById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var movie = await _movieService.GetMovieByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<MovieDto>.Ok(movie));
    }

    [HttpPost]
    [HasPermission("movies:create")]
    [ProducesResponseType(typeof(ApiResponse<MovieDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<MovieDto>>> CreateMovie(
        [FromForm] MovieCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var movie = await _movieService.CreateMovieAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetMovieById),
            new { id = movie.Id },
            ApiResponse<MovieDto>.Ok(movie, "Movie created successfully."));
    }

    [HttpPut("{id:guid}")]
    [HasPermission("movies:update")]
    [ProducesResponseType(typeof(ApiResponse<MovieDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<MovieDto>>> UpdateMovie(
        Guid id,
        [FromForm] MovieUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var movie = await _movieService.UpdateMovieAsync(id, request, cancellationToken);
        return Ok(ApiResponse<MovieDto>.Ok(movie, "Movie updated successfully."));
    }

    [HttpPost("sync-genres")]
    [HasPermission("genres:sync")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<int>>> SyncGenres(CancellationToken cancellationToken = default)
    {
        var importedCount = await _movieService.SyncGenresAsync(cancellationToken);
        return Ok(ApiResponse<int>.Ok(importedCount, "Genres synced successfully."));
    }

    [HttpPost("import/single/{tmdbId:int}")]
    [HasPermission("movies:import")]
    [ProducesResponseType(typeof(ApiResponse<TmdbImportResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TmdbImportResultDto>>> ImportSingle(
        int tmdbId,
        CancellationToken cancellationToken = default)
    {
        var result = await _movieService.ImportMovieFromTmdbAsync(tmdbId, cancellationToken);
        return Ok(ApiResponse<TmdbImportResultDto>.Ok(result));
    }

    [HttpPost("import/bulk")]
    [HasPermission("movies:import")]
    [ProducesResponseType(typeof(ApiResponse<BulkImportResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BulkImportResultDto>>> ImportBulk(
        [FromBody] BulkImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _movieService.ImportBulkPopularMoviesAsync(request.Count, cancellationToken);
        return Ok(ApiResponse<BulkImportResultDto>.Ok(result));
    }
}
