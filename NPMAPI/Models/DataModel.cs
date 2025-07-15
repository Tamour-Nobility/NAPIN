using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NPMAPI.Models
{
    public class DataModel
    {
        public Nullable<decimal> Amount_Approved { get; set; }
        public decimal? Amount_Paid { get; set; }
        public Nullable<decimal> Amount_Adjusted { get; set; }
        public string Reject_Type { get; set; }
        public Nullable<decimal> Reject_Amount { get; set; }
        public string Paid_Proc_Code { get; set; }
        public string Charged_Proc_Code { get; set; }
        public Nullable<long> Insurance_Id { get; set; }
        public string ERA_CATEGORY_CODE { get; set; }
        public string ERA_ADJUSTMENT_CODE { get; set; }
        public string ERA_Rejection_CATEGORY_CODE { get; set; }
        public Nullable<System.DateTime> DOS_From { get; set; }
        public Nullable<System.DateTime> Dos_To { get; set; }
        public Nullable<System.DateTime> Date_Filing { get; set; }
        public string payment_source { get; set; }
        public string ICN { get; set; }

    }
}