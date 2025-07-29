using ScrapSystem.Api.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScrapSystem.Api.Domain.Models
{
    public class User : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string UserId { get; set; }

        public string Password { get; set; }

        public string? Department { get; set; }

        public string? Section { get; set; }

    }

    

    

    
}
