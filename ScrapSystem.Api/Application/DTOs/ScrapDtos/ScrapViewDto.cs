namespace ScrapSystem.Api.Application.DTOs.ScrapDtos
{
    public class ScrapViewDto
    {
        public int SanctionId { get; set; }
        public string Sanction { get; set; }
        public string Material { get; set; }
        public string CostCenter { get; set; }
        public string Sloc { get; set; }
        public decimal? Qty { get; set; }
        public decimal? QtyActual { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? IssueOutDate { get; set; }
        public string MoveType { get; set; }
        public string Plant { get; set; }
        public int? Status { get; set; }
        public string SubType { get; set; }
        public string NameCost { get; set; }
        public string Reason { get; set; }
        public string Section { get; set; }
        public decimal? UnitPrice { get; set; }
        public string Pallet { get; set; }
        public string EnglishName { get; set; }
        public string VietnameseName { get; set; }
        public string Unit { get; set; }
        public string UnitEcus { get; set; }
        public string IssueOut { get; set; } //sua 01.08.2025
    }
}
