using Microsoft.AspNetCore.Mvc;
using NPMAPI.Models;
using NPMAPI.Repositories;
using NPMAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace NPMAPI.Controllers
{
    public class UBClaimController : BaseController

    {
        private readonly IUBClaim _uBClaim;
        public UBClaimController(IUBClaim uBClaim)
        {
            _uBClaim = uBClaim;
        }



        [HttpGet]
        public async Task<ResponseModel> GetAllCodesData()
        {
            return await _uBClaim.GetAllCodesData();
        }

        [HttpGet]
        public async Task<ResponseModel> GetallDropdowns()
        {
            return await _uBClaim.GetallDropdowns();
        }

    



        [HttpPost]
        public async Task<ResponseModel> ManageOccurenceSpan(List<Claims_Occurrence_Code> OSClist)
        {

            return await _uBClaim.ManageOccurenceSpan(OSClist, GetUserId()); ;

        }
    }
}