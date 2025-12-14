using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

//namespace ScrapSystem.Web.Models
//{
//    public class MaterSection
//    {
//    }
//}

namespace ScrapSystem.Web.Models
{
[Table("MaterSection")] // Map đúng bảng trong database
public class MaterSection
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên Section không được để trống")]
    [StringLength(100)]
    public string Section { get; set; }

    [StringLength(255)]
    public string Description { get; set; }
    public byte Flag_del { get; set; }

    // Thêm ngày tạo
    [DataType(DataType.DateTime)]
    public DateTime? Createdate { get; set; }

    }
}
