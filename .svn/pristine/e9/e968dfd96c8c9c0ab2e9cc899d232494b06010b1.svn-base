using ScrapSystem.Api.Application.DTOs.ScrapDtos;
using ScrapSystem.Api.Data.Repositories.IRepositories;
using ScrapSystem.Api.Domain.Models;

namespace ScrapSystem.Api.Infrastructure.Repositories.IRepositories
{
    public interface IScrapRepository: IGenericRepository<Scrap>
    {
        Task<bool> DeleteListCraps(List<Scrap> scraps);
        Task<Scrap> GetScrapBySanctionAndStatusAsync(string sanction, int status);

        Task<(List<ScrapViewDto> Data, int TotalCount)> GetReportScrapByDate(DateTime startDate, DateTime endDate, string sanction, int status = -1, int page = 1, int pageSize = 25);
    }
}
