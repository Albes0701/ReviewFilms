using EFCore.NamingConventions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using ReviewFilms.Api.Data;
using ReviewFilms.Api.Enums;

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
                .UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MapEnum<UserStatus>("user_status");
                    npgsqlOptions.MapEnum<MovieStatus>("movie_status");
                    npgsqlOptions.MapEnum<CreditType>("credit_type");
                    npgsqlOptions.MapEnum<CommentStatus>("comment_status");
                    npgsqlOptions.MapEnum<WatchlistStatus>("watchlist_status");
                    npgsqlOptions.MapEnum<ReportTargetType>("report_target_type");
                    npgsqlOptions.MapEnum<ReportStatus>("report_status");
                    npgsqlOptions.MapEnum<NotificationType>("notification_type");
                    npgsqlOptions.MapEnum<VoteType>("vote_type");
                })
                .UseSnakeCaseNamingConvention();
        });

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
