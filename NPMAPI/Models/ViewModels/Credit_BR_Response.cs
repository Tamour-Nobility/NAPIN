using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPMAPI.Models.ViewModels
{
    public class Credit_BR_Response
    {
        public long PRACTICE_CODE { get; set; }
        public string PRAC_NAME { get; set; }
        public long PATIENT_ACCOUNT { get; set; }
        public string BILL_TO { get; set; }
        public Nullable<decimal> BALANCE { get; set; }
        public string PATIENT_NAME { get; set; }
        public string LAST_PAYMENT_DATE { get; set; }
        public string LAST_STATEMENT_DATE { get; set; }
        public string LAST_CLAIM_DATE { get; set; }
        public string LAST_CHARGE_DATE { get; set; }
    }
}
