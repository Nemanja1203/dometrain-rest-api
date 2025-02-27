using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Movies.Application.Database;
using Movies.Application.Repositories;
using Movies.Application.Services;

namespace Movies.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        //services.AddSingleton<IMovieRepository, MovieRepositoryInMemory>();
        services.AddSingleton<IMovieRepository, MovieRepository>();
        services.AddSingleton<IMovieService, MovieService>(); // Doesn't have any state so that is why it can be singleton
        
        services.AddSingleton<IRatingService, RatingService>();
        services.AddSingleton<IRatingRepository, RatingRepository>();
        
        services.AddValidatorsFromAssemblyContaining<IApplicationMarker>(ServiceLifetime.Singleton);
        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services,
        string connectionString)
    {
        // NOTE: This is singleton masking transient, because new db connection is created every time
        services.AddSingleton<IDbConnectionFactory>(_ =>
            new NpgsqlConnectionFactory(connectionString));
        services.AddSingleton<DbInitializer>();
        return services;
    }
}
