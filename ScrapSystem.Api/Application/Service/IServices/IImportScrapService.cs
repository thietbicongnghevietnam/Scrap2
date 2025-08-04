using ScrapSystem.Api.Application.DTOs.MaterialName;
using ScrapSystem.Api.Application.DTOs.ScrapDtos;
using ScrapSystem.Api.Application.Response;
using ScrapSystem.Api.Application.DTOs.ScrapImageDtos;
using ScrapSystem.Api.Application.Request;
using ScrapSystem.Api.Application.DTOs.ScrapDetailDtos;
using ScrapSystem.Api.Application.DTOs.LabelListDtos;

namespace ScrapSystem.Api.Application.Service.IServices
{
    public interface IImportScrapService
    {
        Task<ApiResult<ParentWithChildren<ScrapDto, ScrapDetailDto>>> ImportScrapAsync(IFormFile file, string sanction, string section, string issueout);
        Task<ApiResult<MaterialNameDto>> ImportMaterialNameAsync(IFormFile file);
        Task<ApiResult<ScrapViewDto>> LoadData(ScrapRequest request);
        Task<ApiResult<MasterDetailDto<ScrapImageDto, ScrapImageDetailDto>>> LoadImage(string sanctionId, string pallet);
        Task<ApiResult<MasterDetailDto<LabelListMasterDto, LabelListDetailDto>>> GetLabelListAsync(DateTime startDate, DateTime endDate, string sanction);

        Task<ApiResult<bool>> UpdateQtyScrapDetail(int id, int qty, int QtyActual);

        Task<ApiResult<bool>> DeleleScrapDetailById(int id);
    }
}
