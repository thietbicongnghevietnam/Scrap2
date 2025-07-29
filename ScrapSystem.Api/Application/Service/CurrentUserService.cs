using ScrapSystem.Api.Application.Service.IServices;

namespace ScrapSystem.Api.Application.Service
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId => _httpContextAccessor.HttpContext?.User?.Claims
            ?.FirstOrDefault(x => x.Type == "sub" || x.Type == "name" || x.Type == "id")?.Value;

        public string UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        public string Email => _httpContextAccessor.HttpContext?.User?.Claims
            ?.FirstOrDefault(x => x.Type == "email")?.Value;

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?.Claims
            ?.Where(x => x.Type == "role")?.Select(x => x.Value) ?? Enumerable.Empty<string>();
    }
}
