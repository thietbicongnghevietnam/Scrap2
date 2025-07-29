using ScrapSystem.Api.Application.Response;
using Serilog;
using System.Text.Json;

namespace ScrapSystem.Api.Application.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = new ApiResult<object>
            {
                IsSuccess = false,
                Message = "An error occurred while processing your request"
            };

            switch (exception)
            {
                case UnauthorizedAccessException:
                    context.Response.StatusCode = 401;
                    response.Message = exception.Message;
                    break;
                case KeyNotFoundException:
                    context.Response.StatusCode = 404;
                    response.Message = "Resource not found";
                    break;
                case ArgumentException:
                    context.Response.StatusCode = 400;
                    response.Message = exception.Message;
                    break;
                default:
                    context.Response.StatusCode = 500;
                    break;
            }

            context.Response.ContentType = "application/json";
            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
