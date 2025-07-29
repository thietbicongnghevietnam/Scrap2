using System.ComponentModel.DataAnnotations;

namespace ScrapSystem.Api.Application.DTOs.UserDtos
{
    public class RefreshTokenRequest
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }

    public class RevokeTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }

    
}
