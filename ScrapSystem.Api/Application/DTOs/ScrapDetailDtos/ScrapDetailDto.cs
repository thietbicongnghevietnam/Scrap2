namespace ScrapSystem.Api.Application.DTOs.ScrapDetailDtos
{
    public class ScrapDetailDto
    {
        public int SanctionId { get; set; }
        public string Material { get; set; }
        public float Qty { get; set; }
        public float QtyActual { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public string CostCenter { get; set; }
        public string Reason { get; set; }
        public string Plant { get; set; }
        public string Sloc { get; set; }
        public string NameCost { get; set; }
        public string Pallet { get; set; }
        public int Noid { get; set; }               // mau issuout  moi cua logictic 12.10.2025
        public string Barcode { get; set; }               // mau issuout  moi cua logictic 12.10.2025

        //them moi cac cot 25.10.2025
        public string issue_out_sloc { get; set; }
        public string SoTK { get; set; }
        public DateTime? NgayTK { get; set; }
        public string Phuluc { get; set; }
        public string Vendor { get; set; }
        public string Sotaisan { get; set; }
        public string BookValue { get; set; }
        public string Picture { get; set; }
        public string ControlNo { get; set; }
        public string Category { get; set; }
        public string Currency { get; set; }
        public string MVT { get; set; }
        public string TypeName { get; set; }
        public string MoveType { get; set; }
        public decimal UnitPriceAC { get; set; }
        public decimal AmountAC { get; set; }
        public string Remark { get; set; }  //them cot 
    }
}
