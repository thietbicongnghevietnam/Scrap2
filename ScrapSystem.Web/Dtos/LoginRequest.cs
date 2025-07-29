using System.ComponentModel.DataAnnotations;

namespace ScrapSystem.Web.Dtos
{
    public class LoginRequest
    {
        public string UserId { get; set; }

        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
