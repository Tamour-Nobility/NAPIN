using System;

namespace NPMAPI.Models.ViewModels
{
    public class ReportRequestModel
    {
        public long PracticeCode { get; set; }
        public string DateTo { get; set; }
        public string DateFrom { get; set; }
        public long[] LocationCode { get; set; }
        public string Date { get; set; }
        public string Month { get; set; }
        public string DateType { get; set; }
        public string DataType { get; set; }
   
        public PagedRequest PagedRequest { get; set; }
    }
    public class PatelReportsRequestModel
    {
        public long PracticeCode { get; set; }
        public string ProviderCode { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }
        public PagedRequest PagedRequest { get; set; }

    }
    public class PagedRequest
    {
        public bool isExport { get; set; } = false;
        public int page { get; set; } = 1;
        public int size { get; set; } = 10;
    }
}