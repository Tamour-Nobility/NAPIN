using System;
using System.ComponentModel.DataAnnotations;

namespace NPMSyncWorker.Entities
{
    internal class Vendor
    {
        [Key]
        public long VendorId { get; set; }
        public string VendorName { get; set; }
        public string Url { get; set; }
        public string RestBaseUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public long ModifiedBy { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
    }
}
