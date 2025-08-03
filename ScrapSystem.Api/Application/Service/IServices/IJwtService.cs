using ScrapSystem.Api.Application.DTOs.UserDtos;
using ScrapSystem.Api.Domain.Models;
using System.Security.Claims;

namespace ScrapSystem.Api.Application.Service.IServices
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateAccessToken(UserDto user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        bool ValidateRefreshToken(string refreshToken);
    }
}
