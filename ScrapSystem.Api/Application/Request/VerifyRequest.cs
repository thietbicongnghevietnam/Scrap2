using Microsoft.AspNetCore.Mvc.Rendering;

namespace ScrapSystem.Api.Application.Request
{
    
    public class VerifyRequest
    {
        public IFormFile File { get; set; }

        public string Type { get; set; }
    }

    public class ImportRequest
    {
        public IFormFile File { get; set; }
        public string Sanction { get; set; }
        public string Section { get; set; }
        public string issueout { get; set; }


        //// Các field khác (Sanction, IssueOut, ...)   //12.10.2025  //thay combox import.cshtml
        //public string SelectedSection { get; set; }

        //// Nếu bạn muốn truyền danh sách từ controller
        //public List<SelectListItem> ListItems { get; set; } = new();
    }

  


}
