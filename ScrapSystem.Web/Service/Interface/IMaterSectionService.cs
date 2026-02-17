using ScrapSystem.Web.Models;

namespace ScrapSystem.Web.Service.Interface
{
    //public class IMaterSectionService
    //{
    //}
    public interface IMaterSectionService
    {
        Task<(List<MaterSection> data, int total)> GetPagedAsync(string keyword,int page,int pageSize,DateTime? dateFrom,DateTime? dateTo);

        Task<MaterSection> GetByIdAsync(int id);
        Task<MaterSection> CreateAsync(MaterSection model);
        Task<MaterSection> UpdateAsync(MaterSection model);
        Task<bool> DeleteAsync(int id);
    }

}
