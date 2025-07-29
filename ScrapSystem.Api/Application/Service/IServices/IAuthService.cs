using ScrapSystem.Api.Application.Response;
using ScrapSystem.Api.Application.DTOs.UserDtos;
using ScrapSystem.Api.Application.Response;
using Microsoft.AspNetCore.Identity.Data;

namespace ScrapSystem.Api.Application.Service.IServices
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(UserLoginDto request);
        Task<ApiResult<UserDto>> RegisterAsync(UserRegisterDto model);
        Task<bool> LogoutAsync(string token);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task<bool> ChangePasswordAsync(string userId, UserChangePasswordDto request);
    }
}
