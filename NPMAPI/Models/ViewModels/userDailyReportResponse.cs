using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models.ViewModels
{
    public class userDailyReportResponse
    {
        public long Practice_Code { get; set; }
        public string Practice_Name { get; set; }
        public long Patient_Account { get; set; }
        public string Patient_Name { get; set; }
        public long CLAIM_NO { get; set; }
        public string Location_Name { get; set; }
        public string Facility{ get; set; }
        public string Attending_Physician { get; set; }
        public string RENDERING_PROVIDER { get; set; }
        public string RESOURCE_PROVIDER { get; set; }
        public string PROCEDURE_CODE { get; set; }
        public string Procedure_Description { get; set; }
        public string Primary_Carrier { get; set; }
        public string Primary_Policy_Number { get; set; }
        public string Secondary_Carrier { get; set; }
        public string Secondary_Policy_Number { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> DOS { get; set; }
        public Nullable<System.DateTime> Entry_Date { get; set; }
        public Nullable<decimal> BILLED_CHARGE { get; set; }
        public Nullable<int> UNITS { get; set; }
        public long claim_charges_id { get; set; }
        public string Modifier_1 { get; set; }
        public string Modifier_2 { get; set; }
        public string Modifier_3 { get; set; }
        public string Modifier_4 { get; set; }
        public string Diagnosis_1 { get; set; }
        public string Diagnosis_2 { get; set; }
        public string Diagnosis_3 { get; set; }
        public string Diagnosis_4 { get; set; }
        public string Diagnosis_5 { get; set; }
        public string Diagnosis_6 { get; set; }
        public string Diagnosis_7 { get; set; }
        public string Diagnosis_8 { get; set; }
        public string Diagnosis_9 { get; set; }
        public string Diagnosis_10 { get; set; }
        public string Diagnosis_11 { get; set; }
        public string Diagnosis_12 { get; set; }
    }
}