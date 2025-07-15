using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;

namespace NPMAPI.Controllers
{
    public class SubmissionController : BaseController
    {
        private readonly ISubmissionRepository _submissionService;

        public SubmissionController(ISubmissionRepository submissionService)
        {
            _submissionService = submissionService;
        }

        [HttpGet]
        public ResponseModel GenerateBatch_5010_P(long practice_id, long claim_id)
        {
            dynamic  CT_BCT = null;
            using (var ctx = new NPMDBEntities())
            {
                CT_BCT =(from c in ctx.Claims
                 //        join cbd in ctx.claim_batch on detail.batch_id equals cbd.batch_id
                 //join c in ctx.Claims on detail.claim_id equals c.Claim_No
                 where c.Claim_No == claim_id
                 select new
                 {
                     claim_type = c.Claim_Type,
                     //batch_claim_type = cbd.batch_claim_type,
                     Pri_Status = c.Pri_Status,
                     Sec_Status = c.Sec_Status
                 }).FirstOrDefault();
            }
            if ((CT_BCT.claim_type == null || CT_BCT.claim_type.ToUpper() == "P") && (CT_BCT.Pri_Status == "N" || CT_BCT.Pri_Status == "R" || CT_BCT.Pri_Status == "B"))
            {
                return _submissionService.GenerateBatch_5010_P_P(practice_id, claim_id);
            }
            else if ((CT_BCT.claim_type == null || CT_BCT.claim_type.ToUpper() == "P") && (CT_BCT.Sec_Status == "N" || CT_BCT.Sec_Status == "R" || CT_BCT.Sec_Status == "B"))

            {
                return _submissionService.GenerateBatch_5010_P_S(practice_id, claim_id);
            }
            else
                return _submissionService.GenerateBatch_For_Packet_837i_5010_I(practice_id, claim_id);
        }

        [HttpGet]
        public ResponseModel View837(long batchId, long practice_id)
        {
            return _submissionService.View837(batchId , practice_id);
        }
        [HttpGet]
        public ResponseModel GenerateBatch_5010_I(long practice_id, long claim_id)
        {
            return _submissionService.GenerateBatch_For_Packet_837i_5010_I(practice_id, claim_id);
        }
        
        [HttpPost]
        public ResponseModel SearchClaim(ClaimSearchViewModel model) => _submissionService.SearchClaim(model);
        #region ClaimsBatch
        [HttpPost]
        public ResponseModel AddUpdateBatch(BatchCreateViewModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return responseModel;
            }
            return _submissionService.AddUpdateBatch(model, GetUserId());
        }

        public ResponseModel GetPendingBatchSelectListErrorCom(long practiceCode, long? providerCode, string practype)
        {
            return _submissionService.GetPendingBatchSelectListErrorCom(practiceCode, providerCode, practype);
        }

        [HttpGet]
        public ResponseModel GetPendingBatchSelectList(long practiceCode, long? providerCode,string practype,string batch_claim_type)
        {
            return _submissionService.GetPendingBatchSelectList(practiceCode, providerCode, practype, batch_claim_type);
        }

        [HttpGet]
        public ResponseModel GetSentBatchSelectList(string searchText, long practiceCode, long? providerCode)
        {
            return _submissionService.GetSentBatchSelectList(searchText, practiceCode, providerCode);
        }

        [HttpPost]
        public ResponseModel GetBatchesDetail(BatchListRequestViewModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return responseModel;
            }
            return _submissionService.GetBatchesDetail(model);
        }

        [HttpPost]
        public ResponseModel AddInBatch(AddInBatchRequestViewModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage));
                return responseModel;
            }
            return _submissionService.AddInBatch(model, GetUserId());
        }

        [HttpPost]
        public ResponseModel LockBatch(LockBatchRequestViewModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return responseModel;
            }
            model.UserId = GetUserId();
            return _submissionService.LockBatch(model);
        }

        [HttpPost]
        public ResponseModel HoldBatch(HoldBatchRequestViewModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return responseModel;
            }
            model.UserId = GetUserId();
            return _submissionService.HoldBatch(model);
        }

        [HttpPost]
        public ResponseModel UploadBatches(BatchUploadRequest model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage));
                return responseModel;
            }
            return _submissionService.UploadBatches(model, GetUserId());
        }

        [HttpPost]
        public ResponseModel GetBatchFileErrors(BatchErrorsRequestModel model)
        {
            return _submissionService.GetBatchFileErrors(model);
        }
        [HttpPost]
        public ResponseModel GetBatchExceptions(BatchErrorsRequestModel model)
        {
            return _submissionService.GetBatchExceptions(model);
        }
        [HttpPost]
        public ResponseModel GetBatchesHistory(BatchesHistoryRequestModel model)
        {
            return _submissionService.GetBatchesHistory(model);
        }
        [HttpGet]
        public ResponseModel Get999Report(long PracticeCode)
        {
            return _submissionService.Get999Report(PracticeCode);
        }

        [HttpGet]
        public ResponseModel GetBatcheDetalis(long batchId)
        {
            return _submissionService.GetBatcheDetalis(batchId);
        }

        [HttpGet]
        public ResponseModel GetCSIBatcheDetalis(int batchId, string claimId)
        {
            return _submissionService.GetCSIBatcheDetalis(batchId, claimId);
        }
        [HttpGet]
        public ResponseModel GetCSIBatcheResponseDetalis(int batchId, long claimId)
        {
            return _submissionService.GetCSIBatcheResponseDetalis(batchId, claimId);
        }
        [HttpGet]
        public ResponseModel GetCSIBatcheDetalisStatus(long claimId)
        {
            return _submissionService.GetCSIBatcheDetalisStatus(claimId);
        }
        [HttpGet]
        public ResponseModel CheckMedicareClaim(long? claimId,long insurance_id)
        {
            return _submissionService.CheckMedicareClaim(claimId,insurance_id);
        }

        [HttpGet]
        public HttpResponseMessage DownloadEDIFile(long batchId)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            var fileInfo = _submissionService.GetBatchFilePath(batchId);
            string filePath;
            if (fileInfo.Status == "success")
            {
                filePath = HttpContext.Current.Server.MapPath($"~/{ConfigurationManager.AppSettings["ClaimBatchSubmissionPath"] + "/" + fileInfo.Response}");
                if (!File.Exists(filePath))
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.ReasonPhrase = string.Format("File not found: {0} .", fileInfo.Response);
                    return response;
                }
                byte[] bytes = File.ReadAllBytes(filePath);
                response.Content = new ByteArrayContent(bytes);
                response.Content.Headers.ContentLength = bytes.LongLength;
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = fileInfo.Response;
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(fileInfo.Response));
                return response;
            }     
            else
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.ReasonPhrase = string.Format("File not found: {0} .", fileInfo.Response);
                return response;
            }
        }

        [HttpPost]
        public ResponseModel RegenerateBatchFile(RegenerateBatchFileRequestModel model)
        {
            return _submissionService.RegenerateBatchFile(model, GetUserId());
        }
        #endregion

        [HttpGet]
        [AllowAnonymous]
        public ResponseModel Read()
        {
            return _submissionService.Read();
        }

        [HttpPost]
        public ResponseModel SearchERA(ERASearchRequestModel model)
        {
            return _submissionService.SearchERA(model);
        }

        [HttpPost]
        public ResponseModel EraSummary(EraSummaryRequest model)
        {
            return _submissionService.EraSummary(model);
        }
        [HttpGet]
        public ResponseModel ViewClaimERA(long claimno , string check_No)
        {
           ViewERASummaryRequest model = new ViewERASummaryRequest();
            ResponseModel rm = new ResponseModel();
           int eraid = 0;
            using (var ctx = new NPMDBEntities())
            {
                //eraid = ctx.ERACLAIMINFOes.Where(p => p.PATIENTACCOUNTNUMBER == claimno.ToString()).Select(p => p.ERAID).Distinct().ToList();
                eraid = ctx.ERACHECKDETAILs
    .Where(p => p.CHECKNUMBER == check_No && (p.is_duplicate == null || p.is_duplicate == false))
    .Select(p => p.ERAID)
    .Distinct().FirstOrDefault();
            }
            model.eraId = eraid;
            model.CHECK_NO = check_No;
            model.claim_no = claimno;
            rm = _submissionService.ViewEraSummary(model);
            return rm;
        }

        public class claimsummaryrequest
        {
            public long? claimNo { get; set; }
            public long? eraId { get; set; }
        }

        public class ApplyERARequestModel
        {
            public string[] claims { get; set; }
            public long eraId { get; set; }
            public DateTime depositDate { get; set; }
        }

        public class ERAAutoPostRequestModel
        {
            public int id { get; set; }
            public DateTime depositDate { get; set; }
        }
        [HttpPost]
        public ResponseModel ERAClaimSummary(claimsummaryrequest req)
        {
            return _submissionService.ERAClaimSummary(req);
        }

        [HttpPost]
        public ResponseModel ApplyERA(ApplyERARequestModel req)
        {
            return _submissionService.ApplyERA(req, GetUserId());
        }

        [HttpPost]
        public ResponseModel AutoPost(ERAAutoPostRequestModel request)
        {
            return _submissionService.AutoPost(request, GetUserId());
        }

        [HttpPost]
        public ResponseModel ERAClaimsOverPayment(claimsummaryrequest req)
        {
            return _submissionService.ERAClaimsOverPayment(req);
        }
    }
}