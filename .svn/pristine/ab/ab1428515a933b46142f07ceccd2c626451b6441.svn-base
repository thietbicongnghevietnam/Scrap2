using ScrapSystem.Api.Application.DTOs.LabelListDtos;

namespace ScrapSystem.Api.Application.Response
{
    public class ApiResult<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Item { get; set; }
        public List<T> Items { get; set; } = new();
        public PaginatedResult<T> PagedResult { get; set; } = new();
        public T MasterDetail { get; set; } 
        public List<string> Errors { get; set; } = new();
    }

    public class PaginatedResult<T>
    {
        public int TotalCount { get; set; }

        public List<T> Records { get; set; } = new();
    }

    public class ParentWithChildren<TParent, TChild>
    {
        public TParent Parent { get; set; }
        public List<TChild> Children { get; set; } = new();
    }
    public class MasterDetailDto<TItem1, TItem2>
    {
        public List<TItem1> Masters { get; set; }
        public List<TItem2> Details { get; set; }
    }

}


