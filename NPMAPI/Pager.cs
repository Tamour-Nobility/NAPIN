namespace NPMAPI
{
    public class Pager
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SortOrder { get; set; }
        public string SortBy { get; set; }
        public string SearchString { get; set; }
    }
    public class PagingResponse
    {
        public int? CurrentPage { get; set; }
        public int? TotalRecords { get; set; }
        public int? FilteredRecords { get; set; }
        public dynamic data { get; set; }
    }
}