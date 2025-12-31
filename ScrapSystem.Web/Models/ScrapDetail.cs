using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScrapSystem.Web.Models
{
    [Table("ScrapDetails")]
    public class ScrapDetail
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int SanctionId { get; set; }

        [Required]
        [StringLength(50)]
        public string Material { get; set; }
        [Required]
        public double Qty { get; set; }
        [Required]
        public double QtyActual { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal? UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal? Amount { get; set; }

        [StringLength(20)]
        public string? CostCenter { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(20)]
        public string? Plant { get; set; }

        [StringLength(20)]
        public string? Sloc { get; set; }

        [StringLength(50)]
        public string? NameCost { get; set; }

        [StringLength(20)]
        public string? Pallet { get; set; }

        [StringLength(50)]
        public string? Barcode { get; set; }

        [StringLength(20)]
        public string? ScrapSloc { get; set; }

        [StringLength(50)]
        public string? ControlNo { get; set; }

        [StringLength(50)]
        public string? FaTool { get; set; }

        [StringLength(50)]
        public string? TypeName { get; set; }

        [StringLength(20)]
        public string? MVT { get; set; }

        [StringLength(20)]
        public string? MoveType { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal? UnitPriceAC { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal? AmountAC { get; set; }

        [StringLength(50)]
        public string? Vendor { get; set; }

        [StringLength(50)]
        public string? type_convert { get; set; }

        public DateTime? CreatedDate { get; set; }
        public int? CreatedId { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedId { get; set; }

        public long? ID2 { get; set; }

        [StringLength(500)]
        public string? IssueOut { get; set; }

        public int? Noid { get; set; }

        [StringLength(50)]
        public string? SoTK { get; set; }

        public DateTime? NgayTK { get; set; }

        [StringLength(20)]
        public string? Phuluc { get; set; }

        [StringLength(50)]
        public string? Sotaisan { get; set; }

        [StringLength(50)]
        public string? BookValue { get; set; }

        [StringLength(50)]
        public string? Picture { get; set; }

        [StringLength(20)]
        public string? Category { get; set; }

        [StringLength(20)]
        public string? Currency { get; set; }

        public byte? FlagEpro { get; set; }

        [ForeignKey(nameof(SanctionId))]
        public Scrap? Scrap { get; set; }
    }
}
