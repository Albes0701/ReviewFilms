using ReviewFilms.Api.Configurations;
using ReviewFilms.Api.Interfaces;
using ReviewFilms.Api.Services;

namespace ReviewFilms.Api.Extensions;

public static class FilmModuleExtensions
{
    public static IServiceCollection AddFilmModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CloudinarySettings>(configuration.GetSection(CloudinarySettings.SectionName));
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<IMovieService, MovieService>();

        return services;
    }
}
