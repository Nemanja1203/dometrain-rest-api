using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Movies.Api.Sdk;
using Movies.Api.Sdk.Consumer;
using Movies.Contracts.Requests;
using Refit;

// var moviesApi = RestService.For<IMoviesApi>("https://localhost:5001");

var services = new ServiceCollection();

// This whole section should be an extension method in the SDK to make user experience smooth
// or provided in documentation
services
    .AddHttpClient()
    .AddSingleton<AuthTokenProvider>()
    .AddRefitClient<IMoviesApi>(s => new RefitSettings
    {
        AuthorizationHeaderValueGetter = async (_, _) =>
            await s.GetRequiredService<AuthTokenProvider>().GetTokenAsync()
    })
    .ConfigureHttpClient(x =>
        x.BaseAddress = new Uri("https://localhost:5001"));

var provider = services.BuildServiceProvider();

var moviesApi = provider.GetRequiredService<IMoviesApi>();

// var movie = await moviesApi.GetMovieAsync("nick-the-greek-2023");

var newMovie = await moviesApi.CreateMovieAsync(new CreateMovieRequest
{
    Title = "Nemanja New Movie", 
    YearOfRelease = 2025, 
    Genres = ["Adventure", "Comedy"]
});

await moviesApi.UpdateMovieAsync(newMovie.Id, new UpdateMovieRequest
{
    Title = "Nemanja New Movie", 
    YearOfRelease = 2024, 
    Genres = ["Adventure", "Comedy"]
});

await moviesApi.DeleteMovieAsync(newMovie.Id);

var request = new GetAllMoviesRequest
{
    Title = null,
    Year = null,
    SortBy = null,
    Page = 1,
    PageSize = 3
};
var movies = await moviesApi.GetMoviesAsync(request);

foreach (var movieResponse in movies.Items)
{
    Console.WriteLine(JsonSerializer.Serialize(movieResponse));
}