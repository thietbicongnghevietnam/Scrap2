using Microsoft.Extensions.Caching.Memory;

namespace ScrapSystem.Api.Application.Middleware
{
    // JWT Blacklist Middleware
    public class JwtBlacklistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public JwtBlacklistMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var cacheKey = $"blacklist_{token}";

                // Check if token is blacklisted
                if (_cache.TryGetValue(cacheKey, out _))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Token has been revoked");
                    return;
                }
            }

            await _next(context);
        }
    }
}
