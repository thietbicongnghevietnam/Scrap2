using ScrapSystem.Api.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace ScrapSystem.Api.Domain.Models
{
    public class ScrapImage : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public int SanctionId { get; set; }
        public string ImagePath { get; set; }
        public int ImageType { get; set; }
    }

}
