using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class GetTicketTrackDetail
    {
     
        
            // Tickets table
            public int? Ticket_Id { get; set; }
            public int Department_Id { get; set; }
            public string Ticket_Type { get; set; }
            public string Ticket_Reason { get; set; }
            public string Ticket_Priority { get; set; }
            public string Ticket_Title { get; set; }
            public string Ticket_Status { get; set; } = "New";
            public long Ticket_Aging { get; set; }
            public string Closing_Remarks { get; set; }
            public string Practice_Code { get; set; }
            public string RProvider_Id { get; set; }
            public string BProvider_Id { get; set; }
            public string Assigned_User { get; set; }
            public string Created_By { get; set; }

            // Ticket_Patient_Claims_Info table
            public string Patient_Account { get; set; }
            public string Claim_No { get; set; }
            public string First_Name { get; set; }
            public string Last_Name { get; set; }
            public string MI { get; set; }
            public string PatientCell_Phone { get; set; }
            public string PatientHome_Phone { get; set; }
            public DateTime? Date_Of_Birth { get; set; }
            public string SSN { get; set; }
            public DateTime? DOS { get; set; }
            public decimal? Total_Billed { get; set; }
            public decimal? Patient_Due { get; set; }
            public decimal? Claim_Due { get; set; }
            public string Ins_Mode { get; set; }
            public string Payer_Name { get; set; }
            public string Payer_Id { get; set; }
            public string Policy_No { get; set; }
        public string DepartmentName { get; set; }      
        public string Practice_Name { get; set; }       
        public string CreatedByName { get; set; }       
        public string AssignedUserName { get; set; }

        //public string Ticket_Message { get; set; }
        public List<TicketMessageDetail> TicketMessages { get; set; }
        public bool? Soft { get; set; } = false;
        }
    
}