using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ScrapSystem.Api.Domain.Common;

namespace ScrapSystem.Api.Domain.Models
{
    public class Scrap : BaseEntity
    {
        public int Id { get; set; }
        public string Sanction { get; set; }
        public int Status { get; set; }
        public string MoveType { get; set; }
        public string SubType { get; set; }
        public DateTime IssueOutDate { get; set; }
        public string Section { get; set; }

    }
}
