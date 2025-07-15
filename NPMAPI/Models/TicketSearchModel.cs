using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class TicketSearchModel
    {
        public long? Ticket_Id { get; set; }
        public string Ticket_Type { get; set; }
        public string Ticket_Reason { get; set; }
        public string Ticket_Priority { get; set; }
        public long? Practice_Code { get; set; }
        public string Claim_No { get; set; }
        public string Ticket_Status { get; set; }
        public int? Department_Id { get; set; }
        public long? Created_By { get; set; }
        public long? Assigned_User { get; set; }
        public string Payer_Name { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}