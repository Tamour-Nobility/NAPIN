using EdiFabric.Core.Model.Edi.X12;
using NPMAPI.Models;
using NPMAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;


namespace NPMAPI.Controllers
{
    public class RealTimeClaimStatusController : BaseController
    {
        private readonly IClaimService _claimService;
        private readonly IClaimBatch276 _IClaimBatch276;
        public RealTimeClaimStatusController(IClaimService claimService, IClaimBatch276 claimBatch276)
            {

                _claimService = claimService;
            _IClaimBatch276 = claimBatch276;
            }
        public async Task<IHttpActionResult> GenerateBatch276(long practice_id, long claim_id,long Insurance_Id, string unique_name)
            {
                try
                {
                //var response = await Task.Run(() => _claimService.SendRequest(practice_id, claim_id);
                var response = await _claimService.SendRequest(practice_id, claim_id, Insurance_Id, unique_name);
                return Ok(response);
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            }

        [HttpPost]
        public async Task<ResponseModel>  GetCSIStatus(long practiceCode, long ClaimNo)
            {
                return await _claimService.GetCSIReport(practiceCode, ClaimNo);
            }

        public async Task<IHttpActionResult> GenerateBatch(long practice_id, long claim_id, long Insurance_Id, string unique_name)
           {
             try
             {
                //var response = await Task.Run(() => _claimService.SendRequest(practice_id, claim_id);
                var response = await _IClaimBatch276.SendRequest(practice_id, claim_id);
                return Ok(response);
             }
             catch (Exception ex)
             {
                return InternalServerError(ex);
             }
          }
        public async Task<IHttpActionResult> UploadBatches276(BatchUploadRequest model)
        {
            if (model == null)
            {
                model = new BatchUploadRequest();
            }

            if (model.BatcheIds == null)
            {
                model.BatcheIds = new long[0];
            }

            if (model.BatcheIds.Length == 0)
            {
                //model.BatcheIds = new long[] {3552021,35517,35599,35511452};  
                model.BatcheIds = new long[] {35517};
            }

            try
            {
                // Assuming UploadBatches returns a Task<ResponseModel>
                var response = await _IClaimBatch276.UploadBatches(model, GetUserId());
                return Ok(response); // Return the ResponseModel object wrapped in Ok()
            }
            catch (Exception ex)
            {
                return InternalServerError(ex); // Return the error as InternalServerError
            }
        }

        public async Task<IHttpActionResult> SingleClaimUploadBatches276(CSIClaimBatchUploadRequest model, [FromUri] bool? getUpdatedCSI = null)
        {
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var existingRecord = ctx.CSI_Batch
                        .FirstOrDefault(b => b.Claim_Number == model.ClaimNo && model.BatcheIds.Contains(b.Batch_Id));

                    if (existingRecord != null && existingRecord.Status_277 == "Pending")
                    {
                        return Ok(new
                        {
                            Status = "Success",
                            Response = new
                            {
                                Type = 4,
                                BatchStatus = "Pending"
                            }
                        });
                    }
                    var response = await _IClaimBatch276.SingleCSIClaimBatchUpload(model, GetUserId());

                    if (response.Response == "Batches has been uploaded successfully." && getUpdatedCSI == true)
                    {
                        var recordToUpdate = ctx.CSI_Batch
                            .FirstOrDefault(b => b.Claim_Number == model.ClaimNo && model.BatcheIds.Contains(b.Batch_Id));

                        if (recordToUpdate != null)
                        {
                            recordToUpdate.Status_277 = "Pending";
                            ctx.SaveChanges();
                        }
                    }
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }



        //public async Task<IHttpActionResult> SingleClaimUploadBatches276(CSIClaimBatchUploadRequest model, [FromUri] bool? getUpdatedCSI = null)
        //{
        //    try
        //    {
        //        using (var ctx = new NPMDBEntities())
        //        {
        //            var response = await _IClaimBatch276.SingleCSIClaimBatchUpload(model, GetUserId());
        //            if (response.Response.Type == 1)
        //            {
        //                if (getUpdatedCSI == true)
        //                {
        //                var existingRecord = ctx.CSI_Batch
        //                .FirstOrDefault(b => b.Claim_Number == model.ClaimNo && model.BatcheIds.Contains(b.Batch_Id));
        //                    if (existingRecord != null)
        //                    {
        //                    //existingRecord.Response_String_277 = "";
        //                    //existingRecord.Batch_Status999 = "Pending";
        //                    existingRecord.Status_277 = "Pending";
        //                    ctx.SaveChanges();
        //                    }
        //                }
        //            }
        //            return Ok(response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return InternalServerError(ex);
        //    }
        //}

        //public async Task<IHttpActionResult> UploadBatches276(BatchUploadRequest model)
        //{
        //    if (model == null)
        //    {
        //        model = new BatchUploadRequest();
        //    }

        //    if (model.BatcheIds == null)
        //    {
        //        model.BatcheIds = new long[0];
        //    }

        //    if (model.BatcheIds.Length == 0)
        //    {
        //        //model.BatcheIds = new long[] {3552021,35517};  
        //        model.BatcheIds = new long[] { 35517 };
        //    }
        //    try
        //    {
        //        var response = await _IClaimBatch276.UploadBatches(model, GetUserId());
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return InternalServerError(ex);
        //    }
        //}


        [HttpPost]
        public async Task<RegenerateBatchCSIFileModel> RegenerateBatchFile(RegenerateBatchCSIFileModel model)
        {
            return await _IClaimBatch276.RegenerateBatchCSI(model, GetUserId());
        }


    }
}