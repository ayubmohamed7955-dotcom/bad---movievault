using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace MovieVault.Api.Middleware;

// Logger alle POST, PUT og DELETE requests som strukturerede logs.
// Middlewaren bestemmer HVAD der logges; Serilog-opsætningen i Program.cs bestemmer HVOR (MongoDB).
//
// Placering i pipelinen: efter UseAuthentication (så vi kender brugeren)
// og før UseAuthorization (så 401 og 403 fra authorization også bliver logget).
public class ChangeLoggingMiddleware
{
    public const string AnonymousUser = "anonymous";

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

        // GET og andre læse-requests logges ikke
        if (!HttpMethods.IsPost(method) && !HttpMethods.IsPut(method) && !HttpMethods.IsDelete(method))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        int? statusCodeOverride = null;

        try
        {
            await _next(context);
        }
        catch
        {
            // Ubehandlet fejl: klienten får 500
            statusCodeOverride = StatusCodes.Status500InternalServerError;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Kun metode, sti, bruger og statuskode logges.
            // Body (passwords), headers (JWT) og connection strings logges ALDRIG.
            _logger.LogInformation(
                "{requestType} {route} by {username} responded {statusCode} at {timestamp} in {elapsedMs} ms",
                method,
                context.Request.Path.Value,
                ResolveUsername(context),
                statusCodeOverride ?? context.Response.StatusCode,
                DateTime.UtcNow.ToString("O"), // ISO 8601 i UTC, fx 2026-11-24T20:15:03.1234567Z
                stopwatch.ElapsedMilliseconds);
        }
    }

    // "anonymous" for offentlige endpoints (fx login og læsning af reviews), også selvom der sendes et token,
    // og for requests uden en gyldig bruger. Ellers brugernavnet fra JWT'en.
    private static string ResolveUsername(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        var isPublicEndpoint =
            endpoint is null
            || endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null
            || endpoint.Metadata.GetMetadata<IAuthorizeData>() is null;

        if (isPublicEndpoint || context.User.Identity?.IsAuthenticated != true)
            return AnonymousUser;

        return context.User.Identity.Name ?? AnonymousUser;
    }
}
