namespace ScrapSystem.Api.Application.DTOs.LabelListDtos
{
    public class LabelListDetailDto
    {
        public int Id { get; set; }
        public string Barcode { get; set; }
        public string Material { get; set; }
        public decimal Qty { get; set; }
        public decimal QtyActual { get; set; }
        public string Unit { get; set; }
        public string Pallet { get; set; }
        public string EnglishName { get; set; }
        public int Noid { get; set; }   //them 1 truong Noid   12.10.2025
        public string boxno { get; set; }   //them 1 truong BoxNO   23.03.2026
        public string sloc { get; set; }   //them 1 truong Sloc   23.03.2026
        public string scrapsloc { get; set; }   //them 1 truong ScrapSloc   23.03.2026
    }
}
