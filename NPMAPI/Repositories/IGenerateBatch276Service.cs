using NPMAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace NPMAPI.Repositories
{
    public interface IGenerateBatch276Service
    {
        ResponseModel GenerateBatch_276277(long practice_id, long claim_id);
        ResponseModelForSerialnumber GenerateSerialNumber(int length = 8);
        ResponseModelSerialNumber SequanceNumber();
        ResponseModelForSerialnumber GenerateSerialNumberISA(int length = 9);
        ResponseModelForSerialnumber GenerateSerialNumberBHTGS(int length = 9);
        Task<ResponseModel> UploadBatches(BatchUploadRequest model, long v);
        Task<ResponseModel> SingleCSIClaimBatchUpload(CSIClaimBatchUploadRequest model, long v);
        //ResponseModel UploadBatches(BatchUploadRequest model, long v);
        ResponseModel RegenerateBatchCSIFile(RegenerateBatchCSIFileModel model,long userId);

        //ResponseModel GenerateBatch_276277(long practice_id, long claim_id);
        //ResponseModelForSerialnumber GenerateBatchSerialNumber(int length = 8);
        //ResponseModelSerialNumber SequanceNumber();
        //ResponseModelForSerialnumber GenerateBatchSerialNumberISA(int length = 9);
        //ResponseModelForSerialnumber GenerateSerialNumberBHTGS(int length = 9);
        //ResponseModel UploadBatches(BatchUploadRequest model, long v);
    }
}