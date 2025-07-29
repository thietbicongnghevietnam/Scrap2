using System.ComponentModel.DataAnnotations;

namespace ScrapSystem.Api.Application.DTOs.UserDtos
{
    public class UserChangePasswordDto
    {
        public string CurrentPassword { get; set; }

        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
