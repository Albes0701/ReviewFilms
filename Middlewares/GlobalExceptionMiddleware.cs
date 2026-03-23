using System.ComponentModel.DataAnnotations;

namespace ReviewFilms.Api.Middlewares;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(exception, "Unhandled exception occurred after the response started.");
                throw;
            }

            var (statusCode, message, errors) = MapException(exception);

            _logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new ErrorResponse(false, message, errors));
        }
    }

    private static (int StatusCode, string Message, string[] Errors) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException =>
                (StatusCodes.Status400BadRequest, validationException.Message, [validationException.Message]),
            BadHttpRequestException badHttpRequestException =>
                (StatusCodes.Status400BadRequest, badHttpRequestException.Message, [badHttpRequestException.Message]),
            ArgumentException argumentException =>
                (StatusCodes.Status400BadRequest, argumentException.Message, [argumentException.Message]),
            FormatException formatException =>
                (StatusCodes.Status400BadRequest, formatException.Message, [formatException.Message]),
            UnauthorizedAccessException unauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, unauthorizedAccessException.Message, [unauthorizedAccessException.Message]),
            KeyNotFoundException keyNotFoundException =>
                (StatusCodes.Status404NotFound, keyNotFoundException.Message, [keyNotFoundException.Message]),
            InvalidOperationException invalidOperationException =>
                (StatusCodes.Status409Conflict, invalidOperationException.Message, [invalidOperationException.Message]),
            _ =>
                (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", ["Please contact support."])
        };
    }

    private sealed record ErrorResponse(bool Success, string Message, string[] Errors);
}
