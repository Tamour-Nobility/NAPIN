using System.Collections.Generic;

namespace NPMAPI.Models
{
    public class ClaimSubmissionModel
    {
        public long claim_No { get; set; }

        public spGetBatchClaimsInfo_Result claimInfo { get; set; }

        public List<spGetBatchClaimsInsurancesInfo_Result> claimInsurance { get; set; }

        public spGetBatchClaimsDiagnosis_Result claimDiagnosis { get; set; }
        public List<SPDataModel> sPDataModel { get; set; }

        public List<spGetBatchClaimsProcedurestest_Result> claimProcedures { get; set; }

        public uspGetBatchClaimsProviderPayersDataFromUSP_Result claimBillingProviderPayer { get; set; }
        /*
      
        private ORMGetClaimBatchInstitutionalInfo ClaimInstitutionalInfo;
        private List<ORMGetClaimBatchOccurenceSpan> ClaimOccuranceSpan;
        private List<ORMGetClaimBatchOccurence> ClaimOccurance;
        private List<ORMGetClaimBatchValue> ClaimValue;
        private List<ORMGetClaimBatchCondition> ClaimCondition;
        */
    }
}