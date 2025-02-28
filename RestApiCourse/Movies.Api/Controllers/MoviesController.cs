using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Models;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiVersion(1.0)]
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

    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    //[HttpPost("movies")]
    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateMovieRequest request,
        CancellationToken token)
    {
        var movie = request.MapToMovie();
        await _movieService.CreateAsync(movie, token);
        //return Ok(movie);
        //return Created($"/{ApiEndpoints.Movies.Create}/{movie.Id}", movie.MapToResponse());
        // return CreatedAtAction(nameof(GetAsync), new { idOrSlug = movie.Id }, movie.MapToResponse());
        return CreatedAtAction(nameof(Get),new { idOrSlug = movie.Id }, movie.MapToResponse());
    }

    // [ApiVersion(1.0, Deprecated = true)]
    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get(
        [FromRoute] string idOrSlug,
        CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        //var movie = await _movieRepository.GetByIdAsync(id);
        var movie = Guid.TryParse(idOrSlug, out Guid id)
            ? await _movieService.GetByIdAsync(id, userId, token)
            : await _movieService.GetBySlugAsync(idOrSlug, userId, token);
        if (movie is null)
        {
            return NotFound();
        }
    
        var response = movie.MapToResponse();
        return Ok(response);
    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAllMoviesRequest request,
        CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        var options = request.MapToOptions()
            .WithUser(userId);
        
        var movies = await _movieService.GetAllAsync(options, token);
        var moviesCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, token);
        
        var moviesResponse = movies.MapToResponse(request.Page, request.PageSize, moviesCount);
        return Ok(moviesResponse);
    }

    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateMovieRequest request,
        CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        var movie = request.MapToMovie(id);
        var updatedMovie = await _movieService.UpdateAsync(movie, userId, token);
        if (updatedMovie is null)
        {
            return NotFound();
        }

        var response = updatedMovie.MapToResponse();
        return Ok(response);
    }

    [Authorize(AuthConstants.AdminUserPolicyName)]
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken token)
    {
        var deleted = await _movieService.DeleteByIdAsync(id, token);
        if (!deleted)
        {
            return NotFound();
        }

        return Ok();
    }
}
