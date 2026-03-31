using ReviewFilms.Api.Interfaces;
using ReviewFilms.Api.Security;
using ReviewFilms.Api.Services;

namespace ReviewFilms.Api.Extensions;

public static class NotificationModuleExtensions
{
    public static IServiceCollection AddNotificationModule(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, MockCurrentUserService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
