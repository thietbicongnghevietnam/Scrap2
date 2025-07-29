using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Sorting;
using ScrapSystem.Api.Application.DTOs.ScrapDtos;
using ScrapSystem.Api.Application.DTOs.VerifyDataDtos;
using ScrapSystem.Api.Domain.Models;
using ScrapSystem.Api.Infrastructure.Repositories.IRepositories;
using ScrapSystem.Api.Repositories;

namespace ScrapSystem.Api.Infrastructure.Repositories
{
    public class ScrapDetailRepository : GenericRepository<ScrapDetail>, IScrapDetailRepository
    {
        private readonly DbSet<ScrapDetail> _dbSet;
        private readonly AppDbContext _context;
 
        public ScrapDetailRepository(AppDbContext context) : base(context)
        {
            _dbSet = context.Set<ScrapDetail>();
            _context = context;
        }

        public async Task<bool> DeleteBySanctionId(int sanctionId)
        {
            try
            {
                int affectedRows = await _dbSet.Where(sd => sd.SanctionId == sanctionId).ExecuteDeleteAsync();
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateScrapDetail(ScrapDetail scrap)
        {
            try
            {
               int affectedRows = await _dbSet.Where(x => x.SanctionId == scrap.SanctionId && x.Material == scrap.Material)
                    .ExecuteUpdateAsync(x => x.SetProperty(p => p.Qty, scrap.Qty));
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateScrapDetailById(int id, int qty)
        {
            try
            {
                int affectedRows = await _dbSet.Where(x => x.Id == id)
                     .ExecuteUpdateAsync(x => x.SetProperty(p => p.Qty, qty));
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<VerificationResult>> GetScrapDetailsWithSanctionInfo(List<string> sanctions)
        {
            try
            {
                var result = await (from scrapDetail in _dbSet
                                    join scrap in _context.Scraps
                                    on scrapDetail.SanctionId equals scrap.Id
                                    where sanctions.Contains(scrap.Sanction)
                                    select new VerificationResult
                                    {
                                        Sanction = scrap.Sanction,
                                        Material = scrapDetail.Material,
                                        Sloc = scrapDetail.Sloc,
                                        Qty = scrapDetail.Qty,
                                    }).ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                return new List<VerificationResult>();
            }
        }
        public async Task<List<VerificationResult>> GetScrapDetailsWithMaterial(List<string> materials)
        {
            try
            {
                var result = await (from scrapDetail in _dbSet
                                    join scrap in _context.Scraps
                                    on scrapDetail.SanctionId equals scrap.Id
                                    where materials.Contains(scrapDetail.Material)
                                    select new VerificationResult
                                    {
                                        Sanction = scrap.Sanction,
                                        Material = scrapDetail.Material,
                                        Sloc = scrapDetail.Sloc,
                                        Qty = scrapDetail.Qty,
                                    }).ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                return new List<VerificationResult>();
            }
        }



    }
}
