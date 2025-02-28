using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers.V2;

//[Authorize]
[ApiController]
//[Route("api/")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }
    
    // [HttpGet(ApiEndpoints.V1.Movies.Get)]
    // public async Task<IActionResult> GetAsync(
    //     [FromRoute] string idOrSlug,
    //     CancellationToken token)
    // {
    //     var userId = HttpContext.GetUserId();
    //     //var movie = await _movieRepository.GetByIdAsync(id);
    //     var movie = Guid.TryParse(idOrSlug, out Guid id)
    //         ? await _movieService.GetByIdAsync(id, userId, token)
    //         : await _movieService.GetBySlugAsync(idOrSlug, userId, token);
    //     if (movie is null)
    //     {
    //         return NotFound();
    //     }
    //
    //     var response = movie.MapToResponse();
    //     return Ok(response);
    // }
    
    [HttpGet(ApiEndpoints.V2.Movies.Get)]
    public async Task<IActionResult> GetAsyncById(
        [FromRoute] Guid idOrSlug,
        CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        //var movie = await _movieRepository.GetByIdAsync(id);
        var movie = await _movieService.GetByIdAsync(idOrSlug, userId, token);
        if (movie is null)
        {
            return NotFound();
        }

        var response = movie.MapToResponse();
        return Ok(response);
    }
    
    [HttpGet(ApiEndpoints.V2.Movies.Get)]
    public async Task<IActionResult> GetAsyncBySlug(
        [FromRoute] string idOrSlug,
        CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        //var movie = await _movieRepository.GetByIdAsync(id);
        var movie = await _movieService.GetBySlugAsync(idOrSlug, userId, token);
        if (movie is null)
        {
            return NotFound();
        }

        var response = movie.MapToResponse();
        return Ok(response);
    }
}
