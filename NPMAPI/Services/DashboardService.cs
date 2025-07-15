using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using EdiFabric.Core.Model.Edi.X12;
using iTextSharp.text;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;

namespace NPMAPI.Services
{

    public class DashboardService : IDashboardRepository
    {
        private readonly IReportRepository _reportService;
        public DashboardService(IReportRepository reportService)
        {
            _reportService = reportService;
        }

        public ResponseModel GetDashboardData(long practiceCode, string fromDate, string toDate, long userId)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                var req = new ReportRequestModel();
                req.PracticeCode = practiceCode;
               // var recentAgingSummary = _reportService.AgingSummaryRecent(req, userId);
                var CPAAnalysis = _reportService.GetCPAAnalysis(practiceCode, fromDate, toDate, userId);
                var insuranceAging = _reportService.GetInsuranceAging(practiceCode,userId);
                var claimsAndERAs = _reportService.GetClaimsAndERAs(practiceCode, fromDate, toDate,userId);
                var patientAging = _reportService.GetPatientAging(practiceCode,userId);
                var pcRatio = _reportService.GetPCRatio(practiceCode, fromDate, toDate, userId);
                var ChargesANDPaymentsTrend = _reportService.GetChargesANDPaymentsTrend(practiceCode, fromDate, toDate,userId);
               // var recentChargesPayment = _reportService.ChargesPaymentsRecent(req, userId);
               // var agingDashboard = _reportService.GetAgingDashboard(req, userId);
                res.Response = new ExpandoObject();
                // res.Response.recentAgingSummary = recentAgingSummary.Response.Count == 0 ? null : recentAgingSummary.Response;
                // res.Response.CPAAnalysis = CPAAnalysis.Response.Count==0 ? null : CPAAnalysis.Response;
                // res.Response.insuranceAging = insuranceAging.Response.Count == 0 ? null : insuranceAging.Response;
                // res.Response.claimsAndERAs = claimsAndERAs.Response.Count == 0 ? null : claimsAndERAs.Response;
                // res.Response.recentAgingSummary = recentAgingSummary.Response.Count == 0 ? null : recentAgingSummary.Response;
                // res.Response.patientAging = patientAging.Response.Count == 0 ? null : patientAging.Response;
                // res.Response.pcRatio = pcRatio.Response.Count == 0 ? null : pcRatio.Response;
                //res.Response.agingDashboard = agingDashboard.Response.Count == 0 ? null : agingDashboard.Response;
               // res.Response.recentAgingSummary = recentAgingSummary.Response;
                res.Response.CPAAnalysis = CPAAnalysis.Response;
                res.Response.insuranceAging = insuranceAging.Response;
                res.Response.claimsAndERAs = claimsAndERAs.Response;
                res.Response.patientAging = patientAging.Response;
                res.Response.pcRatio = pcRatio.Response;
                res.Response.ChargesANDPaymentsTrend = ChargesANDPaymentsTrend.Response;
              //  res.Response.agingDashboard = agingDashboard.Response;

                res.Status = "success";
                return res;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ResponseModel GetExternalPractices()
        {
            List<int?> practices = new List<int?>(); 
            
            ResponseModel res = new ResponseModel();
            try { 
            using (var ctx = new NPMDBEntities())
            {
                    //  practices = ctx.Practice_Reporting.Where(pr => (pr.Deleted ?? false) == false).Select(pr => pr.Practice_Code).ToList();
                    //res.Response = ctx.Practice_Reporting.Where(pr => (pr.Deleted ?? false) == false).Select(pr => pr.Practice_Code).ToList();
                    //..Above change is commented to resolve the COSS Practice issue occured due to Practice_Reporting table change
                    res.Response = ctx.ExternalPractices_Reporting.Where(pr => (pr.Deleted ?? false) == false).Select(pr => pr.Practice_Code).ToList();
                }
                res.Status = "success";
            }
            catch(Exception e) {
                res.Status="error";
                throw e;
            }

            return res;
        }

    }
}