//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NPMAPI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class PracticeVendor
    {
        public long PracticeVendorId { get; set; }
        public long PracticeId { get; set; }
        public long VendorId { get; set; }
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTimeOffset> CreatedDate { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTimeOffset> ModifiedDate { get; set; }
    }
}
