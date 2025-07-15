using NPMAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Repositories
{
    public interface IClaimStatusService
    {
        ResponseModel GenerateBatch_276(long practice_id, long claim_id,long Insurance_Id, string unique_name);
        ResponseModelForSerialnumber GenerateSerialNumber(int length = 8);
        ResponseModelSerialNumber SequanceNumber();
        ResponseModelForSerialnumber GenerateSerialNumberISA(int length = 9);
        ResponseModelForSerialnumber GenerateSerialNumberBHTGS(int length = 9);
        //ResponseModel GenerateBatch_276277(long practice_id, long claim_id, long Insurance_Id, string unique_name);

    }
}