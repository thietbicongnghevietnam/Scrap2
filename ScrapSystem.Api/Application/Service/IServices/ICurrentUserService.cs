namespace ScrapSystem.Api.Application.Service.IServices
{
    public interface ICurrentUserService
    {
        string UserId { get; }
        string UserName { get; }
        string Email { get; }
        bool IsAuthenticated { get; }
        IEnumerable<string> Roles { get; }
    }
}
