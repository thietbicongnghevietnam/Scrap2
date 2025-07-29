using Azure.Core;
using Microsoft.EntityFrameworkCore;
using ScrapSystem.Api.Application.DTOs.UserDtos;
using ScrapSystem.Api.Domain.Models;
using ScrapSystem.Api.Infrastructure.Repositories.IRepositories;
using ScrapSystem.Api.Repositories;

namespace ScrapSystem.Api.Infrastructure.Repositories
{
    public class UserRefreshTokenRepository : GenericRepository<UserRefreshToken>, IUserRefreshTokenRepository
    {

        private readonly DbSet<UserRefreshToken> _dbSet;

        public UserRefreshTokenRepository(AppDbContext context) : base(context)
        {
            _dbSet = context.Set<UserRefreshToken>();
        }

        public async Task<UserRefreshToken> GetUserRefreshTokenByUserId(RefreshTokenRequest request, string userId)
        {
            var userRefreshToken = await _dbSet.FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken &&
                                         rt.UserId == userId &&
                                         rt.IsActive &&
                                         rt.ExpiryDate > DateTime.UtcNow);
            return userRefreshToken;
        }

        public async Task<UserRefreshToken> GetUserRefreshTokenByToken( string token)
        {
            var userRefreshToken = await _dbSet
                .FirstOrDefaultAsync(rt => rt.Token == token && rt.IsActive);
            return userRefreshToken;
        }

        public async Task<List<UserRefreshToken>> GetListToken(string userId)
        {
            var userRefreshToken = await _dbSet.Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync();
            return userRefreshToken;
        }
    }
}
