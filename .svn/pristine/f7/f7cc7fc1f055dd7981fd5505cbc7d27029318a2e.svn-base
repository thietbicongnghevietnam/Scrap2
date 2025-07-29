using ScrapSystem.Api.Data.Repositories.IRepositories;
using ScrapSystem.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using ScrapSystem.Api.Domain.Models;

namespace ScrapSystem.Api.Data.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly DbSet<User> _dbSet;

        public UserRepository(AppDbContext context) : base(context)
        {
            _dbSet = context.Set<User>();
        }


        public async Task<User> GetUserByUserId(string userId)
        {
            var user = await _dbSet.Where(x => x.UserId == userId).FirstOrDefaultAsync();

            return user;
        }

      
    }
}
