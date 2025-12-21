using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScrapSystem.Web.Models
{
    [Table("Users")]
    public class Users
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "UserID không được để trống")]
        [StringLength(20)]
        public string UserID { get; set; }

        [StringLength(100)]
        public string? Password { get; set; }

        [StringLength(200)]
        public string? Section { get; set; }

        [StringLength(200)]
        public string? Department { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CreatedDate { get; set; }

        public int? CreatedId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedDate { get; set; }

        //[StringLength(20)]
        //public string? UpdatedId { get; set; }
        public int? UpdatedId { get; set; }

        [StringLength(20)]
        public string? U_Pass { get; set; }
    }
}
