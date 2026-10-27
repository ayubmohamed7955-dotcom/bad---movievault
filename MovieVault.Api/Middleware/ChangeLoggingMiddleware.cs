using System.Diagnostics;

namespace MovieVault.Api.Middleware;

// Logger alle POST, PUT og DELETE requests.
// Middlewaren bestemmer HVAD der logges; Serilog-konfigurationen i appsettings.json
// bestemmer HVOR det havner (MongoDB).
public class ChangeLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ChangeLoggingMiddleware> _logger;

    public ChangeLoggingMiddleware(RequestDelegate next, ILogger<ChangeLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;

        // GET og andre læse-requests sendes bare videre uden logging
        if (!HttpMethods.IsPost(method) && !HttpMethods.IsPut(method) && !HttpMethods.IsDelete(method))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        int? statusCode = null;

        try
        {
            await _next(context);
        }
        catch
        {
            // Ubehandlet fejl: responsen bliver 500
            statusCode = StatusCodes.Status500InternalServerError;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "HTTP {HttpMethod} {RequestPath} responded {StatusCode} at {RequestTimestamp} in {ElapsedMs} ms",
                method,
                context.Request.Path.Value,
                statusCode ?? context.Response.StatusCode,
                DateTime.UtcNow,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
