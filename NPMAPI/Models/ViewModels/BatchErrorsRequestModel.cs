using System;

namespace NPMAPI.Models.ViewModels
{
    public class BatchErrorsRequestModel
    {
        public long practiceCode { get; set; }
        public long? bactchId { get; set; }
        public long? providerCode { get; set; }
        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }
        public string batch_type { get; set; }
    }
}