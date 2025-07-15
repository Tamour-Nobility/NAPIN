using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models.ViewModels
{
    public class VisitClaimActivityReportResponse
    {
        public string POSTING { get; set; }
        public string PATIENT_NAME { get; set; }
        public string DOS { get; set; }
        public string RESP { get; set; }
        public string CPT_CODE { get; set; }
        public string Description { get; set; }
        public string Billed { get; set; }
        public string PROVIDER { get; set; }
        public string LOCATION { get; set; }
        public decimal? Amount { get; set; }
    }
}