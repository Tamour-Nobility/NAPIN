using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class TicketAssignmentRequest
    {
        public int UserId { get; set; }
        public int CurrentUser { get; set; }
        public List<int> TicketIds { get; set; }
    }
}