namespace ScrapSystem.Api.Application.DTOs.VerifyDataDtos
{
    public class VerifyDataViewModel
    {
        public IFormFile ExcelFile { get; set; } 
        public string SheetName { get; set; } 
    }
}
