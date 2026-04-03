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
        services.Configure<TmdbSettings>(configuration.GetSection(TmdbSettings.SectionName));
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddHttpClient<ITmdbSyncService, TmdbSyncService>((serviceProvider, httpClient) =>
        {
            var tmdbSettings = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<TmdbSettings>>()
                .Value;

            httpClient.BaseAddress = new Uri(tmdbSettings.BaseUrl);
        });
        services.AddScoped<IMovieService, MovieService>();

        return services;
    }
}
