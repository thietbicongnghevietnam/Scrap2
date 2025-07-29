using Dapper;
using Microsoft.EntityFrameworkCore;
using ScrapSystem.Api.Application.DTOs.ScrapDtos;
using ScrapSystem.Api.Application.DTOs.ScrapImageDtos;
using ScrapSystem.Api.Domain.Models;
using ScrapSystem.Api.Infrastructure.Repositories.IRepositories;
using ScrapSystem.Api.Repositories;

namespace ScrapSystem.Api.Infrastructure.Repositories
{
    public class ScrapRepository : GenericRepository<Scrap>, IScrapRepository
    {
        private readonly DbSet<Scrap> _dbSet;
        private readonly AppDbContext _context;
        public ScrapRepository(AppDbContext context) : base(context)
        {
            _dbSet = context.Set<Scrap>();
            _context = context;
        }

        public async Task<bool> DeleteListCraps(List<Scrap> scraps)
        {
            try
            {
                _dbSet.RemoveRange(scraps);
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public async Task<Scrap> GetScrapBySanctionAndStatusAsync(string sanction, int status)
        {

            var scrap = await _dbSet
           .Where(s => s.Sanction == sanction && s.Status == status)
           .FirstOrDefaultAsync();

            return scrap;
        }

        public async Task<(List<ScrapViewDto> Data, int TotalCount)> GetReportScrapByDate(DateTime startDate, DateTime endDate, string sanction, int status = -1, int page = 1, int pageSize = 25)
        {
            var conn = _context.Database.GetDbConnection();
            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                    await conn.OpenAsync();

                var parameters = new
                {
                    startDate,
                    endDate,
                    sanction,
                    status,
                    page,
                    pageSize
                };

                using var multi = await conn.QueryMultipleAsync(
                    "GetDataScraps_Web",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure);

                var data = (await multi.ReadAsync<ScrapViewDto>()).ToList();
                var totalCount = await multi.ReadFirstAsync<int>();

                return (data, totalCount);
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    await conn.CloseAsync();
            }
        }

        

        



    }
}
