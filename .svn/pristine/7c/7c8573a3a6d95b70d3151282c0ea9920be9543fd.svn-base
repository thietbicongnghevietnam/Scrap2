using ScrapSystem.Api.Application.DTOs.UserDtos;
using ScrapSystem.Api.Data.Repositories.IRepositories;
using ScrapSystem.Api.Domain.Models;

namespace ScrapSystem.Api.Infrastructure.Repositories.IRepositories
{
    public interface IUserRefreshTokenRepository : IGenericRepository<UserRefreshToken>
    {
        Task<UserRefreshToken> GetUserRefreshTokenByUserId(RefreshTokenRequest request, string userId);
        Task<UserRefreshToken> GetUserRefreshTokenByToken(string token);

        Task<List<UserRefreshToken>> GetListToken(string userId);
    }
}
