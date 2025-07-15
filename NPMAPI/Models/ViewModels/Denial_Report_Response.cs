using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPMAPI.Models.ViewModels
{
    public class Denial_Report_Response
    {
        public long PRACTICE_CODE { get; set; }
        public string PRACTICE_NAME { get; set; }
        public long CLAIM_NUMBER { get; set; }
        public string DOS { get; set; }
        public string CLAIM_DOE { get; set; }
        public string PATIENT_NAME { get; set; }
        public long PATIENT_ACCOUNT { get; set; }
        public string BILLING_PROVIDER { get; set; }
        public string RESOURCE_PROVIDER { get; set; }
        public string DENIAL_DATE { get; set; }
        public string PROCEDURE_CODE { get; set; }
        public decimal AMOUNT_PAID { get; set; }
        public decimal AMOUNT_ADJUSTED { get; set; }
        public decimal REJECT_AMOUNT { get; set; }
        public string PAYMENT_TYPE { get; set; }
        public string PAYMENT_SOURCE { get; set; }
        public string CHEQUE_DATE { get; set; }
        public string CHEQUE_NO { get; set; }
        public string DENIAL_CODE { get; set; }
        public string DENIAL_CODE_DESCRIPTION { get; set; }

    }
}
