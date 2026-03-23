using ReviewFilms.Api.Middlewares;

namespace ReviewFilms.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseGlobalExceptionMiddleware(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        return app;
    }

    public static WebApplication UseApiSwagger(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "ReviewFilms API v1");
            });
        }

        return app;
    }
}
