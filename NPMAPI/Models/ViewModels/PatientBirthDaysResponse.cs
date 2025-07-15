using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models.ViewModels
{
    public class PatientBirthDaysResponse
    {
        public long PATIENT_ACCOUNT { get; set; }
        public string PATIENT_NAME { get; set; }
        public DateTime? PATIENT_DOB { get; set; }
        public int? PATIENT_AGE { get; set; }
        public string HOME_PHONE { get; set; }
        public string INSURANCE_NAME { get; set; }
        public DateTime? RECENT_DOS { get; set; }
        public string PROVIDER_NAME { get; set; }
    }
}