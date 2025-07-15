using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models.ViewModels
{
    public class AccountAssigneeResponse
    {
        public long? Account_No { get; set; }
        public DateTime? Task_Created_Date { get; set; }
        public string Priority { get; set; }
        public string Assigned_By { get; set; }
        public DateTime? Start_Date { get; set; }
        public DateTime? Due_Date { get; set; }
        public string Patient_Name { get; set; }
        public string Task_Status { get; set; }
        public string Assigned_To { get; set; }
        public long Account_AssigneeID { get; set; }
        public long? AssignedToUserId { get; set; }
        public string AssignedToUserName { get; set; }
        public long? AssignedByUserId { get; set; }
        public string AssignedByUserName { get; set; }
        public long? PracticeCode { get; set; }
        public bool? Deleted { get; set; }
        public long? CreatedBy { get; set; }
  
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? ModificationAllowed { get; set; }
    }
}