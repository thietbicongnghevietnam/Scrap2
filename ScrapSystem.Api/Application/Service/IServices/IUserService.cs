using ScrapSystem.Api.Data.Repositories.IRepositories;
using ScrapSystem.Api.Application.DTOs.UserDtos;
using ScrapSystem.Api.Application.Response;

namespace ScrapSystem.Api.Application.Service.IServices
{
    public interface IUserService
    {
        Task<UserDto> GetUserById(int id);
        
    }
}
