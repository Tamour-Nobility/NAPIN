using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using System.Threading.Tasks;
using static NPMAPI.Controllers.SubmissionController;

namespace NPMAPI.Repositories
{
    public interface ISubmissionRepository
    {
        ResponseModel GenerateBatch_5010_P_S(long practice_id, long claim_id);
        ResponseModel GenerateBatch_5010_P_P(long practice_id, long claim_id);
        ResponseModel GenerateBatch_For_Packet_837i_5010_I(long practice_id, long claim_id);
        ResponseModel SearchClaim(ClaimSearchViewModel model);
        ResponseModel AddUpdateBatch(BatchCreateViewModel model, long userId);
        ResponseModel GetPendingBatchSelectListErrorCom(long practiceCode, long? providerCode, string practype);
        ResponseModel GetPendingBatchSelectList(long practiceCode, long? providerCode,string practype,string batch_claim_type);
        ResponseModel GetSentBatchSelectList(string searchText, long practiceCode, long? providerCode);
        ResponseModel GetBatchesDetail(BatchListRequestViewModel model);
        ResponseModel AddInBatch(AddInBatchRequestViewModel model, long userId);
        ResponseModel LockBatch(LockBatchRequestViewModel model);
        ResponseModel HoldBatch(HoldBatchRequestViewModel model);
        ResponseModel View837(long batchId, long practice_id);
        ResponseModel GetBatchFileErrors(BatchErrorsRequestModel model);
        ResponseModel UploadBatches(BatchUploadRequest model, long userId);
        //Task<ResponseModel> UploadBatches(BatchUploadRequest model, long userId);

        ResponseModel GetBatchesHistory(BatchesHistoryRequestModel model);
        ResponseModel Get999Report(long practice_code);
        ResponseModel GetBatcheDetalis(long batchId);
        ResponseModel GetCSIBatcheDetalis(int batchId,string claimId);
        ResponseModel GetCSIBatcheResponseDetalis(long batchId,long claimId);
        ResponseModel GetCSIBatcheDetalisStatus(long claimId);
        ResponseModel CheckMedicareClaim(long? claimId, long batch_id);
        ResponseModel GetBatchFilePath(long batchId);
        ResponseModel RegenerateBatchFile(RegenerateBatchFileRequestModel model, long userId);
        ResponseModel Read();
        ResponseModel SearchERA(ERASearchRequestModel model);
        ResponseModel EraSummary(EraSummaryRequest model);
        ResponseModel ViewEraSummary(ViewERASummaryRequest model);
        ResponseModel ERAClaimSummary(claimsummaryrequest model);
        ResponseModel ApplyERA(ApplyERARequestModel req, long userId);
        ResponseModel AutoPost(ERAAutoPostRequestModel request, long v);
        ResponseModel ERAClaimsOverPayment(claimsummaryrequest req);
        ResponseModel GetBatchExceptions(BatchErrorsRequestModel model);
    }
}
