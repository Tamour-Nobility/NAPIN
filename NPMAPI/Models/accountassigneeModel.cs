using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class accountassigneeModel
    {
        public long Account_AssigneeID { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public Nullable<System.DateTime> Start_Date { get; set; }
        public Nullable<System.DateTime> Due_Date { get; set; }
        public Nullable<long> Assignedto_UserId { get; set; }
        public string Assignedto_UserName { get; set; }
        public string Assignedto_FullName { get; set; }
        public Nullable<long> AssignedBy_UserId { get; set; }
        public string AssignedBy_UserName { get; set; }
        public string AssignedBy_FullName { get; set; }
        public Nullable<long> PracticeCode { get; set; }
        public Nullable<long> PatientAccount { get; set; }
        public string PatientFullName { get; set; }
        public string Account_notes { get; set; }
        public Nullable<bool> Deleted { get; set; }
        public Nullable<long> Created_By { get; set; }
        public Nullable<System.DateTime> Created_Date { get; set; }
        public Nullable<long> Modified_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public Nullable<bool> modification_allowed { get; set; }
    }
}