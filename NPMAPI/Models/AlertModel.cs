
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NPMAPI.Models
{
    public class AlertModel
    {
        public long AlertID { get; set; }
        public string Type { get; set; }
        public Nullable<System.DateTime> EffectiveFrom { get; set; }
        public string Priority { get; set; }
        public Nullable<System.DateTime> EffectiveTo { get; set; }
        public string ApplicableFor { get; set; }
        public string AlertMessage { get; set; }
        public Nullable<bool> Demographics { get; set; }
        public Nullable<bool> ClaimSummary { get; set; }
        public Nullable<bool> Claim { get; set; }
        public string ClaimText { get; set; }
        public Nullable<bool> AddNewClaim { get; set; }
        public Nullable<bool> AddNewPayment { get; set; }
        public Nullable<long> Modified_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public Nullable<bool> Inactive { get; set; }
        public Nullable<System.DateTime> Created_Date { get; set; }
        public Nullable<long> Created_By { get; set; }
        public Nullable<long> Patient_Account { get; set; }

    }
}