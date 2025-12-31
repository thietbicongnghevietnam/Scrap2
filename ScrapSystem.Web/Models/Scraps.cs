//namespace ScrapSystem.Web.Models
//{
//    public class Scraps
//    {
//    }
//}
using ScrapSystem.Api.Domain.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Scraps")]
public class Scrap
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Sanction { get; set; }

    [StringLength(50)]
    public string Section { get; set; }

    public ICollection<ScrapDetail>? ScrapDetails { get; set; }
}
