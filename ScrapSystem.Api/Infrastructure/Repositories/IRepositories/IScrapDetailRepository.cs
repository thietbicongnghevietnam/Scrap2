using ScrapSystem.Api.Application.DTOs.VerifyDataDtos;
using ScrapSystem.Api.Data.Repositories.IRepositories;
using ScrapSystem.Api.Domain.Models;

namespace ScrapSystem.Api.Infrastructure.Repositories.IRepositories
{
    public interface IScrapDetailRepository :IGenericRepository<ScrapDetail>
    {
        Task<bool> DeleteBySanctionId(int sanctionId);
        Task<List<VerificationResult>> GetScrapDetailsWithSanctionInfo(List<string> sanctions);
        Task<List<VerificationResult>> GetScrapDetailsWithMaterial(List<string> materials);
        Task<bool> UpdateScrapDetail(ScrapDetail scrap);
        Task<bool> UpdateScrapDetailById(int id, int qty, int QtyActual);
    }
}
