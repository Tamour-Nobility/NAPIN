using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class SaveTrackDetails
    {
        public long? Ticket_Id { get; set; }
        public string Ticket_Message { get; set; }
        public int Department_Id { get; set; }
        public string Assigned_User { get; set; }
        public string Ticket_Status { get; set; }
        public string Closing_Remarks { get; set; }
        public string Created_By { get; set; }
        public string Modified_By { get; set; }

    }
}