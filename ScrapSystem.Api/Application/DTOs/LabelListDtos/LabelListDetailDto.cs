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
    }
}
