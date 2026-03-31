using EFCore.NamingConventions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ReviewFilms.Api.Data;

namespace ReviewFilms.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options
            .UseMySQL(connectionString)
            .UseSnakeCaseNamingConvention();
        });

        services.AddAuthModule(configuration);

        return services;
    }

    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services
            .AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(entry => entry.Value is { Errors.Count: > 0 })
                        .SelectMany(entry => entry.Value!.Errors.Select(error =>
                            string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? $"The field '{entry.Key}' is invalid."
                                : $"{entry.Key}: {error.ErrorMessage}"))
                        .ToArray();

                    return new BadRequestObjectResult(new
                    {
                        success = false,
                        message = "Validation failed.",
                        errors
                    });
                };
            });

        return services;
    }

    public static IServiceCollection AddApiSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ReviewFilms API",
                Version = "v1",
                Description = "ReviewFilms API foundation."
            });
        });

        return services;
    }
}
