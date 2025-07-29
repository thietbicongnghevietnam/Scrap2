using Microsoft.EntityFrameworkCore;
using ScrapSystem.Api.Application.DTOs.ScrapDtos;
using ScrapSystem.Api.Domain.Models;
using ScrapSystem.Api.Infrastructure.Repositories.IRepositories;
using ScrapSystem.Api.Repositories;

namespace ScrapSystem.Api.Infrastructure.Repositories
{
    public class ImageScrapRepository : GenericRepository<ScrapImage>, IImageScrapRepository
    {
        private readonly DbSet<ScrapImage> _dbSet;
        private readonly AppDbContext _context;
        public ImageScrapRepository(AppDbContext context) : base(context)
        {
            _dbSet = context.Set<ScrapImage>();
            _context = context;
        }

        public async Task<List<ScrapImage>> LoadImageBySanctionId(string sanctionId, string pallet)
        {
            var rs = await _dbSet
                
                .ToListAsync();

            return rs;
        }
    }
}

