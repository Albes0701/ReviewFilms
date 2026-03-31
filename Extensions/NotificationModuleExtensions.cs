using ReviewFilms.Api.Interfaces;
using ReviewFilms.Api.Services;

namespace ReviewFilms.Api.Extensions;

public static class NotificationModuleExtensions
{
    public static IServiceCollection AddNotificationModule(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
