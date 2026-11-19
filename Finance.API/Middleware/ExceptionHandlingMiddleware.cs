using Finance.Application.Exceptions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace Finance.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new Dictionary<string, object>();

            switch (exception)
            {
                case KeyNotFoundException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    response["code"] = "NOT_FOUND";
                    // Fixed text: framework/file-system exceptions must never leak their message.
                    response["message"] = "Resource not found.";
                    break;

                case ValidationException validationEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response["code"] = "VALIDATION_FAILED";
                    response["message"] = "Validation failed";
                    response["errors"] = validationEx.Errors
                        .Select(e => new { e.PropertyName, e.ErrorMessage, e.ErrorCode })
                        .ToList();
                    break;

                case ConflictException conflict:
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    response["code"] = conflict.Code;
                    response["message"] = conflict.Message;
                    break;

                case ForbiddenException forbidden:
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    response["code"] = forbidden.Code;
                    response["message"] = forbidden.Message;
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    response["code"] = "UNAUTHORIZED";
                    response["message"] = "Unauthorized.";
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response["code"] = "INTERNAL_ERROR";
                    response["message"] = "Internal server error";
                    break;
            }

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
        }
    }
}
