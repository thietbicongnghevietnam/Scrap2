namespace ScrapSystem.Api.Application.DTOs.LabelListDtos
{
    public class LabelListMasterDto
    {
        public string Barcode { get; set; }
        public string Pallet { get; set; }
        public string Sanction { get; set; }
        public string Section { get; set; }
        public DateTime IssueOutDate { get; set; }
    }
}
