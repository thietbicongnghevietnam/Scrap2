using ScrapSystem.Api.Application.Middleware;

namespace ScrapSystem
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtBlacklist(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtBlacklistMiddleware>();
        }
    }
}
