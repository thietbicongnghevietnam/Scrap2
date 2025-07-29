using ScrapSystem.Api.Application.DTOs.ScrapDtos;
using ScrapSystem.Api.Application.Request;
using ScrapSystem.Api.Application.Response;
using ScrapSystem.Web.Dtos;
using X.PagedList;

namespace ScrapSystem.Web.Service.Interface
{
    public interface IApiClientService
    {
        Task<ApiResult<LoginResponse>> LoginAsync(Dtos.LoginRequest request);

        Task<ApiResult<bool>> LogoutAsync();
        Task<T> GetAsync<T>(string endpoint);
        Task<T> GetAsync<T>(string endpoint, Dictionary<string, string> queryParams);
        Task<T> PostAsync<T>(string endpoint, object data);
        Task<T> PutAsync<T>(string endpoint, object data);
        Task DeleteAsync(string endpoint);
        void SetAuthToken(string token);
        Task<bool> RefreshTokenAsync();

        Task<IPagedList<ScrapViewDto>> LoadScrapList(ScrapRequest request);

        Task<ApiResult<object>> PostFileAsync(string endPoint, Dictionary<string, IFormFile> files, Dictionary<string, string> contents);
    }
}
