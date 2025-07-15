using System;

namespace NPMAPI.Models.ViewModels
{
    public class TimingSearchViewModel
    {
        public long practiceCode { get; set; }
        public long providerCode { get; set; }
        public long locationCode { get; set; }
        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
    }
}