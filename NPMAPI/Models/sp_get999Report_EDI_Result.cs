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
    
    public partial class sp_get999Report_EDI_Result
    {
        public long Batch_Id { get; set; }
        public string Batch_Name { get; set; }
        public string Provider { get; set; }
        public string Batch_Status { get; set; }
        public Nullable<System.DateTime> Created_Date { get; set; }
        public Nullable<System.DateTime> Uploaded_Date { get; set; }
        public string Uploaded_User_Name { get; set; }
        public Nullable<int> Aging { get; set; }
        public string Batch_Status999 { get; set; }
    }
}
