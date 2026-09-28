using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    // Constructor me ILogger inject karein
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Request aane par log karein
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("️ Incoming Request: {Method} {Url}", context.Request.Method, context.Request.Path);

        // 2. Agli middleware ya controller ko call karein
        await _next(context);

        // 3. Response jane par log karein
        stopwatch.Stop();
        _logger.LogInformation("⬅️ Outgoing Response: {StatusCode} (Time taken: {Time}ms)",
            context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
    }
}