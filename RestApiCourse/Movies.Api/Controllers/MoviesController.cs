using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
//[Route("api/")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    //[HttpPost("movies")]
    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateMovieRequest request)
    {
        var movie = request.MapToMovie();
        await _movieService.CreateAsync(movie);
        //return Ok(movie);
        //return Created($"/{ApiEndpoints.Movies.Create}/{movie.Id}", movie.MapToResponse());
        return CreatedAtAction(nameof(GetAsync), new { idOrSlug = movie.Id }, movie.MapToResponse());
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> GetAsync([FromRoute] string idOrSlug)
    {
        //var movie = await _movieRepository.GetByIdAsync(id);
        var movie = Guid.TryParse(idOrSlug, out Guid id)
            ? await _movieService.GetByIdAsync(id)
            : await _movieService.GetBySlugAsync(idOrSlug);
        if (movie is null)
        {
            return NotFound();
        }

        var response = movie.MapToResponse();
        return Ok(response);
    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAllAsync()
    {
        var movies = await _movieService.GetAllAsync();
        var moviesResponse = movies.MapToResponse();
        return Ok(moviesResponse);
    }

    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateMovieRequest request)
    {
        var movie = request.MapToMovie(id);
        var updatedMovie = await _movieService.UpdateAsync(movie);
        if (updatedMovie is null)
        {
            return NotFound();
        }

        var response = updatedMovie.MapToResponse();
        return Ok(response);
    }

    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
    {
        var deleted = await _movieService.DeleteByIdAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return Ok();
    }
}
