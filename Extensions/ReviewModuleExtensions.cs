using ReviewFilms.Api.Interfaces;
using ReviewFilms.Api.Services;

namespace ReviewFilms.Api.Extensions;

public static class ReviewModuleExtensions
{
    public static IServiceCollection AddReviewModule(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IReviewService, ReviewService>();

        return services;
    }
}
