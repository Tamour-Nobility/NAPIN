using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPMAPI.Models.ViewModels
{
    public class Plan_Aging_Response
    {
        public long PRACTICE_CODE { get; set; }
        public string PRAC_NAME { get; set; }
        public string GROUP_NAME { get; set; }
        public string AGING_PAYER { get; set; }
        public Nullable<decimal> BALANCE { get; set; }
        public Nullable<decimal> Current { get; set; }
        public Nullable<decimal> C30_Days { get; set; }
        public Nullable<decimal> C60_Days { get; set; }
        public Nullable<decimal> C90_Days { get; set; }
        public Nullable<decimal> C120_Days { get; set; }
    }
}

