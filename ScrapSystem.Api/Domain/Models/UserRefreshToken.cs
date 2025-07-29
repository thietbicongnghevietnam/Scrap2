using System.ComponentModel.DataAnnotations;

namespace ScrapSystem.Api.Domain.Models
{
    public class UserRefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? RevokedDate { get; set; }
        public bool IsActive { get; set; }

    }
}
