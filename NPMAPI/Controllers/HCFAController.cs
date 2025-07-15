using EdiFabric.Core.Model.Edi.X12;
using NPMAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Web.Http;

namespace NPMAPI.Controllers
{
    public class HCFAController : BaseController
    {
        [HttpGet]
        public HttpResponseMessage GenerateHcfa(string claimNo, string insuranceType, bool isPrintable = false)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["HCFAAPIBaseAddress"]);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var urlParameters = $"/api/hcfa?claimNo={claimNo}&isPrintable={isPrintable}&insuranceType={insuranceType}";
                return client.GetAsync(urlParameters).Result;
            }
        }


        [HttpGet]
        public HttpResponseMessage GenerateUB(string claimNo, bool isPrintable = false)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["UB04APIBaseAddress"]);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var urlParameters = $"/api/Ub04form?claimNo={claimNo}&isPrintable={isPrintable}";
                return client.GetAsync(urlParameters).Result;
            }
        }
        [HttpGet]
        public HttpResponseMessage GenerateBatchHcfa(string batchIds, string insuranceType, bool isPrintable = false, string status=null)
        {
            string claimIdsString = "";
            bool hasData = false;
            dynamic response2 = "";
            List<long?> claimIds = new List<long?>();
            using (var ctx = new NPMDBEntities())
            {
                //var batchIdArray = batchIds.Split(',');
                //var batchesTypes = (from cb in ctx.claim_batch

                //                    where batchIdArray.Contains(cb.batch_id.ToString())
                //                    select new
                //                    {
                //                        Submission_Type = cb.Submission_Type
                //                    }).ToList();
                //bool hasElectronicSubmission = batchesTypes.Any((b => b.Submission_Type.ToLower() == "electronic"));
                //if (hasElectronicSubmission)
                //{
                //    var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                //    {
                //        Content = new StringContent("The batch contains electronic submissions and cannot proceed.")
                //    };
                //    return responseMessage;
                //}
                List<long> batchIdList = batchIds.Split(',')
                                 .Select(b => Convert.ToInt64(b.Trim()))
                                 .ToList();

                claimIds = ctx.claim_batch_detail
                 .Where(cbd => batchIdList.Contains((long)cbd.batch_id))
                 .Select(cbd => cbd.claim_id)
                 .ToList();

                
                claimIdsString = String.Join(",", claimIds);
            }
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["HCFAAPIBaseAddress"]);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var urlParameters = $"/api/hcfa?claimNo={claimIdsString}&isPrintable={isPrintable}&insuranceType={insuranceType}";
                var response = client.GetAsync(urlParameters).Result;
                var contentType = response.Content.Headers.ContentType?.MediaType;
                var content = response.Content.ReadAsStringAsync().Result;
                hasData = contentType == "application/json" &&
                       !string.IsNullOrWhiteSpace(content) &&
                       content.Trim() != "{}" &&
                       content.Trim() != "[]";
                response2 = client.GetAsync(urlParameters).Result;
                //return client.GetAsync(urlParameters).Result;
            }
            using (var ctx = new NPMDBEntities())
            {
                if (status != null && status == "Yes")
                {
                    if (hasData == false)
                    {
                        var claims = ctx.Claims.Where(c => claimIds.Contains(c.Claim_No)).ToList();

                        foreach (var c in claims)
                        {
                            if (!string.IsNullOrEmpty(c.Pri_Status) && (c.Pri_Status.ToLower() == "n" || c.Pri_Status.ToLower() == "r"))
                            {
                                c.Pri_Status = "B";
                            }
                            else if (!string.IsNullOrEmpty(c.Sec_Status) && (c.Sec_Status.ToLower() == "n" || c.Sec_Status.ToLower() == "r"))
                            {
                                c.Sec_Status = "B";
                            }
                            else if (!string.IsNullOrEmpty(c.Oth_Status) && (c.Oth_Status.ToLower() == "n" || c.Oth_Status.ToLower() == "r"))
                            {
                                c.Oth_Status = "B";
                            }
                        }
                        var batchid = ctx.claim_batch.Where(b => b.batch_id.ToString() == batchIds).FirstOrDefault();

                        batchid.batch_status = "Printed";
                        batchid.batch_lock = true;
                        batchid.date_uploaded = DateTime.Now;

                        ctx.SaveChanges();
                    }
                }
            }
            return response2;
        }
        [HttpGet]
        public bool canPrint(string batchIds)
        {
            string claimIdsString = "";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            using (var ctx = new NPMDBEntities())
            {
                var batchIdArray = batchIds.Split(',');
                var batchesTypes = (from cb in ctx.claim_batch

                                    where batchIdArray.Contains(cb.batch_id.ToString())
                                    select new
                                    {
                                        Submission_Type = cb.Submission_Type
                                    }).ToList();
                bool hasElectronicSubmission = batchesTypes.Any(b =>
            b.Submission_Type != null &&
            b.Submission_Type.Equals("electronic", StringComparison.OrdinalIgnoreCase));
                if (hasElectronicSubmission)
                {
                    return false;
                }
                else
                    return true;
            };
        }
    }

}
