using NPMAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace NPMAPI.Repositories
{
    public interface IClaimService
    {
        Task<Output277> SendRequest(long practice_id, long claim_id,long Insurance_Id, string unique_name);
        Task<ResponseModel> GetCSIReport(long practiceCode, long ClaimNo);
    }
}