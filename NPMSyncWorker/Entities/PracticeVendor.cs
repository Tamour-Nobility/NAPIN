using System;
using System.ComponentModel.DataAnnotations;

namespace NPMSyncWorker.Entities
{
    internal class PracticeVendor
    {
        [Key]
        public long PracticeVendorId { get; set; }
        public long PracticeId { get; set; }
        public long VendorId { get; set; }
        public long CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public long ModifiedBy { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
    }
}
