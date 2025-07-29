using Microsoft.EntityFrameworkCore;
using ScrapSystem.Api.Domain.Models;
using ScrapSystem.Api.Infrastructure.Repositories.IRepositories;
using ScrapSystem.Api.Repositories;

namespace ScrapSystem.Api.Infrastructure.Repositories
{
    public class MaterialNameRepository : GenericRepository<MaterialName>, IMaterialNameRepository
    {
        private readonly DbSet<MaterialName> _dbSet;
        public MaterialNameRepository(AppDbContext context) : base(context)
        {
            _dbSet = context.Set<MaterialName>();
        }

        public async Task<List<MaterialName>> AddMultiEntities(List<MaterialName> models)
        {

            var newEntities = new List<MaterialName>();

            foreach (var model in models)
            {
                var exists = await _dbSet.FindAsync(model.Material);

                if (exists == null)
                {
                    newEntities.Add(model);
                }
            }

            if (newEntities.Any())
            {
                await _dbSet.AddRangeAsync(newEntities);
            }

            return newEntities;

        }

    }
}
