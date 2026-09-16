using System.Net;
using System.Text.Json;

namespace Studentst.Middlewares
{
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionMiddleware> _logger;

        public CustomExceptionMiddleware(RequestDelegate next, ILogger<CustomExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                // Agli middleware ya controller ko call karein
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // Agar koi error aaye, toh yahan pakdein
                _logger.LogError(ex, "Kuch gadbad ho gayi: {Message}", ex.Message);

                // Response ko clean JSON format mein set karein
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = new
                {
                    success = false,
                    message = "Internal Server Error. Kripya baad mein try karein.",
                    // details = ex.Message // Production mein error details client ko nahi dikhate (security risk)
                };

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, options);

                await httpContext.Response.WriteAsync(json);
            }
        }
    }
}