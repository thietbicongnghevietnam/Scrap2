using ScrapSystem.Api.Application.DTOs.ScrapDtos;
using X.PagedList;

namespace ScrapSystem.Web.Models
{
    public class ReportViewModel<T>
    {
        public int SelectedStatus { get; set; }
        public IPagedList<T> PageLists { get; set; }
    }


}
