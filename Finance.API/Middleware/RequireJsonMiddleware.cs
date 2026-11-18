using System.Text.Json;

namespace Finance.API.Middleware
{
    /// <summary>
    /// Rejects write requests under /api that carry a body with a non-JSON Content-Type (415).
    /// </summary>
    public class RequireJsonMiddleware
    {
        private static readonly HashSet<string> WriteMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            HttpMethods.Post, HttpMethods.Put, HttpMethods.Patch, HttpMethods.Delete
        };

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly RequestDelegate _next;

        public RequireJsonMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            if (WriteMethods.Contains(request.Method)
                && request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
                && HasBody(request)
                && !IsJson(request.ContentType))
            {
                context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["code"] = "UNSUPPORTED_MEDIA_TYPE",
                    ["message"] = "Content-Type must be application/json"
                }, JsonOptions));
                return;
            }

            await _next(context);
        }

        private static bool HasBody(HttpRequest request) =>
            request.ContentLength > 0
            || (request.ContentLength is null
                && request.Headers.TransferEncoding.ToString().Contains("chunked", StringComparison.OrdinalIgnoreCase));

        private static bool IsJson(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType)) return false;
            var mediaType = contentType.Split(';', 2)[0].Trim();
            return mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase);
        }
    }
}
