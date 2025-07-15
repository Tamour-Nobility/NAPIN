using NPMAPI.Models;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;

namespace NPMAPI.Models
{
    public class ClaimsViewModel
    {
        public long PatientAccount { get; set; }
        public long PracticeCode { get; set; }
        public Claim ClaimModel { get; set; }
   
        public string PTLReasonDetail { get; set; }
       
        public string PTLReasonDoctorFeedback { get; set; }
      
        public Admission_Detail Admission_Details { get; set; }
        public UbClaimDropdowns UbClaimDropdown { get; set; }

        /////////// Below are the prefilled lists for claim
        public List<SelectListViewModel> PTLReasons { get; set; }
        public List<PatientInsuranceViewModel> PatientInsuranceList { get; set; }
        public List<ClaimInsuranceViewModel> claimInusrance { get; set; }
        public List<SelectListViewModelForProvider> AttendingPhysiciansList { get; set; }
        public List<SelectListViewModelForProvider> BillingPhysiciansList { get; set; }
        public List<SelectListViewModel> PracticeLocationsList { get; set; }
        public List<SelectListViewModel> ReferralPhysiciansList { get; set; }
        public List<Place_Of_Services> POSList { get; set; }
        public List<EOB_Adjustment_Codes> AdjustCodeList { get; set; }
        public List<ClaimChargesViewModel> claimCharges { get; set; }
        public List<ClaimPaymentViewModel> claimPayments { get; set; }
        public CLAIM_NOTES claimNotes { get; set; }
        public DateTime ClaimDate { get; set; }
        public string DX1Description { get; set; }
        public DateTime? DX1ExpiryDate { get; set; }
        public DateTime? DX1EffectiveDate { get; set; }

        public string DX2Description { get; set; }
        public DateTime? DX2ExpiryDate { get; set; }
        public DateTime? DX2EffectiveDate { get; set; }

        public string DX3Description { get; set; }
        public DateTime? DX3ExpiryDate { get; set; }
        public DateTime? DX3EffectiveDate { get; set; }


        public string DX4Description { get; set; }
        public DateTime? DX4ExpiryDate { get; set; }
        public DateTime? DX4EffectiveDate { get; set; }


        public string DX5Description { get; set; }
        public DateTime? DX5ExpiryDate { get; set; }
        public DateTime? DX5EffectiveDate { get; set; }


        public string DX6Description { get; set; }
        public DateTime? DX6ExpiryDate { get; set; }
        public DateTime? DX6EffectiveDate { get; set; }

        public string DX7Description { get; set; }
        public DateTime? DX7ExpiryDate { get; set; }
        public DateTime? DX7EffectiveDate { get; set; }

        public string DX8Description { get; set; }
        public DateTime? DX8ExpiryDate { get; set; }
        public DateTime? DX8EffectiveDate { get; set; }

        public string DX9Description { get; set; }
        public DateTime? DX9ExpiryDate { get; set; }
        public DateTime? DX9EffectiveDate { get; set; }

        public string DX10Description { get; set; }
        public DateTime? DX10ExpiryDate { get; set; }
        public DateTime? DX10EffectiveDate { get; set; }

        public string DX11Description { get; set; }
        public DateTime? DX11ExpiryDate { get; set; }
        public DateTime? DX11EffectiveDate { get; set; }

        public string DX12Description { get; set; }
        public DateTime? DX12ExpiryDate { get; set; }
        public DateTime? DX12EffectiveDate { get; set; }
        public List<SelectListViewModel> ResubmissionCodes { get; set; }
        public ClaimsViewModel()
        {
            UbClaimDropdown = new UbClaimDropdowns();
            ClaimModel = new Claim();
            claimNotes = new CLAIM_NOTES();
        }
    }

    public  class Admission_Detail
    {
        public long Id { get; set; }
        public Nullable<long> Type_Of_Admission_Id { get; set; }
        public Nullable<long> Admhour { get; set; }
        public string AdmSource { get; set; }
        public Nullable<long> Dischargehour { get; set; }
        public Nullable<long> Discharge_status_Id { get; set; }
        public Nullable<long> Type_of_Bill { get; set; }
        public Nullable<long> Claim_No { get; set; }
        public Nullable<long> Practice_code { get; set; }
        public Nullable<long> Created_By { get; set; }
        public Nullable<System.DateTime> Created_Date { get; set; }
        public Nullable<long> Modified_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
    }

    public class ClaimPaymentViewModel
    {
        public string InsurancePayerName { get; set; }
        public Claim_Payments claimPayments { get; set; }
    }

    public class ClaimInsuranceViewModel
    {
        public string InsurancePayerName { get; set; }
        public string SubscriberName { get; set; }
        public string Status277 { get; set; }
        public Claim_Insurance claimInsurance { get; set; }
    }
    public class ClaimChargesViewModel
    {
        public string amt { get; set; }
        public string Description { get; set; }
        public string Drug_Code { get; set; }
        public bool IsAnesthesiaCpt { get; set; }
        public Claim_Charges claimCharges { get; set; }
    }


    public class CPTWiseCharges
    {
        public string ProviderCode { get; set; }
        public string ProcedureCode { get; set; }
        public string LocationCode { get; set; }
        public string ModifierCode { get; set; }
        public string FacilityCode { get; set; }
        public string IsSelfPay { get; set; }
        public string InsuranceID { get; set; }
        public string PracticeCode { get; set; }
        public string PracticeState { get; set; }
        public string Alternate_Code { get; set; }
        public DateTime Dos_From { get; set; }
    }
    public class ClaimSearchViewModel
    {
        public DateTime? DOSFrom { get; set; }
        public DateTime? DOSTo { get; set; }
        public List<long> PatientAccount { get; set; }
        public List<long> Provider { get; set; }
        public bool? icd9 { get; set; }
        public string type { get; set; }
        public string billedTo { get; set; }
        public string status { get; set; }
        public List<long> insurance { get; set; }
        public List<long> location { get; set; }
        public long PracticeCode { get; set; }
    }
    public class GetPatientForClaims
    {
        public string Address { get; set; }
        public long practiceCode { get; set; }
    }

    public class UbClaimDropdowns
    {
        public List<OccurrenceCodeModel> OccCode { get; set; } = null;
        public List<ConditionCodeModel> CcOde { get; set; } = null;
        public List<OccurenceSpanModel> OccSpanCode { get; set; } = null;
        public List<ValueeCode> ValueCode { get; set; } = null;

        public UbClaimDropdowns()
        {
            OccCode = new List<OccurrenceCodeModel>();
            CcOde = new List<ConditionCodeModel>();
            OccSpanCode = new List<OccurenceSpanModel>();
            ValueCode = new List<ValueeCode>();
        }
    }

    //public class UbClaimDropdowns
    //{
    //    public List<OccurrenceCodeModel> Occcode { get; set; }
    //    public List<ConditionCodeModel> CcOde { get; set; }
    //    public List<OccurenceSpanModel> OccspanCode { get; set; }
    //    public List<ValueeCode> ValueCode { get; set; }




    //}

    public class OccurenceSpanModel
    {
        public Nullable<long> OSCID { get; set; }
        public Nullable<long> Practice_Code { get; set; }
        public Nullable<long> ClaimNo { get; set; }
        public string OccSpanCode { get; set; }
        public string DateFrom { get; set; }
        public string DateThrough { get; set; }
        public string Descriptions { get; set; } = "";
        public Nullable<bool> Isdeleted { get; set; }
}

    public class OccurrenceCodeModel
    {

        public Nullable<long> OCID { get; set; }
        public Nullable<long> Practice_Code { get; set; }
        public Nullable<long> Claim_no { get; set; }
        public string OccCode { get; set; }
        public string Descriptions { get; set; } = "";
        public string Date2 { get; set; }
        public Nullable<bool> Isdeleted { get; set; }


}
  
    public class ConditionCodeModel
    {

        public Nullable<long> CCID { get; set; }
        public Nullable<long> Practice_Code { get; set; }
        public Nullable<long> Claim_No { get; set; }
        public string ConditionCode { get; set; }
        public string Descriptions { get; set; } = "";
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<bool> Isdeleted { get; set; }


}
 
    public class ValueeCode
    {
        public Nullable<long> VCID { get; set; }
        public Nullable<long> Practice_Code { get; set; }
        public Nullable<long> Claim_No { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public string Value_Codes_Id { get; set; }
        public Nullable<bool> Isdeleted { get; set; }
    }

}