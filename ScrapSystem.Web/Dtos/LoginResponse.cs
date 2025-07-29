using ScrapSystem.Api.Application.DTOs.UserDtos;
using System.ComponentModel.DataAnnotations;

namespace ScrapSystem.Web.Dtos
{
    
    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; }
    }


    

    
}
