using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class TicketMessageDetail
    {
        public long Trail_id { get; set; }
        public long Ticket_Id { get; set; }
        public string Ticket_Message { get; set; }
        public string Ticket_Status { get; set; }
        public string Created_By_Name { get; set; }
        public string Created_By { get; set; }
        public string AssignedDep { get; set; }
        public string AssignedUser { get; set; }
        public string Closing_Remarks { get; set; }
        public DateTime? Created_Date { get; set; }
    }
}