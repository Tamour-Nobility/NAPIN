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
    
    public partial class Ticket_Tracking
    {
        public long Ticket_Trail_Id { get; set; }
        public Nullable<long> Ticket_Id { get; set; }
        public string Ticket_Message { get; set; }
        public Nullable<long> Department_Id { get; set; }
        public Nullable<long> Assigned_User { get; set; }
        public string Ticket_Status { get; set; }
        public Nullable<long> Created_By { get; set; }
        public Nullable<System.DateTime> Created_Date { get; set; }
        public Nullable<long> Modified_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public Nullable<bool> Deleted { get; set; }
    }
}
