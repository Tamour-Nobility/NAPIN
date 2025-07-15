using NPMAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace NPMAPI.Repositories
{
    public interface IClaimBatch276
    {
        Task<ResponseModel> UploadBatches(BatchUploadRequest model, long v);
        Task<ResponseModel> SingleCSIClaimBatchUpload(CSIClaimBatchUploadRequest model, long v);
        Task<Output277> SendRequest(long practice_id, long claim_id);
        Task<RegenerateBatchCSIFileModel> RegenerateBatchCSI(RegenerateBatchCSIFileModel model, long userId);
    }
}