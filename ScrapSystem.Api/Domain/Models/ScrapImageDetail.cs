using System.ComponentModel.DataAnnotations;

namespace ScrapSystem.Api.Domain.Models
{
    public class ScrapImageDetail
    {
        [Key]
        public int Id { get; set; }
        public int SanctionId { get; set; }
        public int MaterialId { get; set; }
        public string ImagePath { get; set; }
        public string ImageType { get; set; } 
    }

}
