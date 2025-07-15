using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPMAPI.Models.ViewModels
{
    public  class Patient_ASR_Response
    {
        public long PRACTICE_CODE { get; set; }
        public string PRAC_NAME { get; set; }
        public string PATIENT_NAME { get; set; }
        public long PATIENT_ACCOUNT { get; set; }
        public string POLICY_NUMBER { get; set; }
        public Nullable<decimal> BALANCE { get; set; }
        public Nullable<decimal> Current { get; set; }
        public Nullable<decimal> C30_Days { get; set; }
        public Nullable<decimal> C60_Days { get; set; }
        public Nullable<decimal> C90_Days { get; set; }
        public Nullable<decimal> C120_Days { get; set; }
    }
}

