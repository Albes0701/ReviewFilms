using Microsoft.AspNetCore.Mvc;
using ReviewFilms.Api.DTOs.Common;
using ReviewFilms.Api.DTOs.Films;
using ReviewFilms.Api.Interfaces;

namespace ReviewFilms.Api.Controllers;

[ApiController]
[Route("api/persons")]
public sealed class PersonsController : ControllerBase
{
    private readonly IMovieService _movieService;

    public PersonsController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<PersonListItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<PersonListItemDto>>>> GetPersons(
        CancellationToken cancellationToken = default)
    {
        var persons = await _movieService.GetPersonsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<PersonListItemDto>>.Ok(persons));
    }
}
