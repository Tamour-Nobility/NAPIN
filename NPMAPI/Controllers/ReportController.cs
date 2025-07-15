using System;
using System.Threading.Tasks;
using System.Web.Http;
using iTextSharp.text.pdf.parser.clipper;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace NPMAPI.Controllers
{
    public class ReportController : BaseController
    {

        private readonly IReportRepository _reportService;

        public ReportController(IReportRepository reportService)
        {
            _reportService = reportService;
        }

        #region Reports

        [HttpGet]
        public ResponseModel AgingSummaryAnalysisReport(long PracticeCode)
        {
            return _reportService.AgingSummaryAnalysisReport(PracticeCode);
        }
        [HttpGet]
        public Task<ResponseModel> DormantClaimsReport(long PracticeCode, int page, int size)
        {
            return _reportService.DormantClaimsReport(PracticeCode, page, size);
        }

        [HttpGet]
        public ResponseModel DormantClaimsReports(long Claim_no)
        {
            return _reportService.DormantClaimsReports(Claim_no);
        }

        [HttpGet]
        public ResponseModel DormantClaimsReportsPagination(long PracticeCode)
        {
            return _reportService.DormantClaimsReportsPagination(PracticeCode);
        }

        [HttpGet]
        public ResponseModel AgingSummaryPatientAnalysisReport(long PracticeCode)
        {
            return _reportService.AgingSummaryPatientAnalysisReport(PracticeCode);
        }

        [HttpGet]
        public ResponseModel FinancialAnalysisByProviderandProcCodesReport(long PracticeCode, DateTime DateFrom, DateTime DateTo)
        {
            return _reportService.FinancialAnalysisByProviderandProcCodesReport(PracticeCode, DateFrom, DateTo);
        }

        [HttpGet]
        public ResponseModel FinancialAnalysisCPTLevelReport(long PracticeCode, DateTime DateFrom, DateTime DateTo)
        {
            return _reportService.FinancialAnalysisCPTLevelReport(PracticeCode, DateFrom, DateTo);
        }

        [HttpGet]
        public ResponseModel PaymentDetail(long? PracticeCode, DateTime? DateFrom, DateTime? DateTo, long? PatientAccount)
        {
            return _reportService.PaymentDetail(PracticeCode, DateFrom, DateTo, PatientAccount);
        }
        //added by Sami ullah
        [HttpGet]
        public ResponseModel PaymentDetailPagination(long? PracticeCode, DateTime? DateFrom, DateTime? DateTo, long? PatientAccount, int page, int size)
        {
            return _reportService.PaymentDetailPagination(PracticeCode, DateFrom, DateTo, PatientAccount,page,size);
        }

        [HttpGet]
        public ResponseModel CPTWisePaymentDetail(long? PracticeCode, DateTime? DateFrom, DateTime? DateTo)
        {
            return _reportService.CPTWisePaymentDetail(PracticeCode, DateFrom, DateTo);
        }
        [HttpGet]
        public ResponseModel CPTWisePaymentDetailPagination(long? PracticeCode, DateTime? DateFrom, DateTime? DateTo,  int page, int count)
        {
            return _reportService.CPTWisePaymentDetailPagination(PracticeCode, DateFrom, DateTo, page,count);
        }

        [HttpGet]
        public ResponseModel AppointmentDetailReport(long PracticeCode, string DateFrom, string DateTo)
        {
            return _reportService.AppointmentDetailReport(PracticeCode, DateFrom, DateTo);
        }

        [HttpGet]
        public ResponseModel holdReport(long PracticeCode)
        {
            return _reportService.holdReport(PracticeCode);
        }


        [HttpPost]
        public ResponseModel ClaimPaymentsDetailReport([FromBody] ClaimPaymentsDetailRequest request)
        {
            if (ModelState.IsValid)
                return _reportService.ClaimPaymentsDetailReport(request);
            else
                return new ResponseModel
                {
                    Status = "Error",
                    Response = "Validation"
                };
        }

        [HttpGet]
        public ResponseModel MissingAppointmentDetailReport(long PracticeCode, string DateFrom, string DateTo)
        {
            return _reportService.MissingAppointmentDetailReport(PracticeCode, DateFrom, DateTo);
        }

        [HttpPost]
        public ResponseModel RecallVisits(ReportRequestModel req)
        {
            return _reportService.RecallVisits(req, GetUserId());
        }

        [HttpPost]
        public ResponseModel PeriodAnalysisAndClosing(ReportRequestModel req)
        {
            return _reportService.PeriodAnalysisAndClosing(req, GetUserId());
        }

        [HttpPost]
        public ResponseModel PracticeAnalysis(ReportRequestModel req)
        {
            return _reportService.PracticeAnalysis(req, GetUserId());
        }

        [HttpPost]
        public ResponseModel PatientBirthDays(ReportRequestModel req)
        {
            return _reportService.PatientBirthDays(req, GetUserId());
        }

        [HttpPost]
        public ResponseModel AgingSummaryRecent(ReportRequestModel req)
        {
            return _reportService.AgingSummaryRecent(req, GetUserId());
        }

        [HttpPost]
        public ResponseModel ChargesPaymentsRecent(ReportRequestModel req)
        {
            return _reportService.ChargesPaymentsRecent(req, GetUserId());
        }

        [HttpPost]
        public ResponseModel GetAgingDashboard(ReportRequestModel req)
        {
            return _reportService.GetAgingDashboard(req, GetUserId());
        }
        [HttpGet]
        public ResponseModel GetInsuranceDetailReport(long? PracCode)
        {
            return _reportService.GetInsuranceDetailReport(PracCode, GetUserId());
        }
        //added By Samiullah
        [HttpGet]
        public ResponseModel GetInsuranceDetailReportPagination(long? PracCode,int page,int size)
        {
            return _reportService.GetInsuranceDetailReportPagination(PracCode, GetUserId(),page,size);
        }

        [HttpGet]
        public ResponseModel GetUserReport(string PracCode, string userid,string dateFrom,string dateTo)
        {
            return _reportService.GetUserReport(PracCode, userid, dateFrom, dateTo);
        }
        //added by Sami Ullah
        [HttpGet]
        public ResponseModel GetUserReportPagination(string PracCode, string userid, string dateFrom, string dateTo,int page,int size )
        {
            return _reportService.GetUserReportPagination(PracCode, userid, dateFrom, dateTo,page,size);
        }
        [HttpGet]
        public ResponseModel GetRollingSummaryReport(string PracCode, string duration)
        {
            return _reportService.GetRollingSummaryReport(PracCode, duration);
        }
        //added By Sami Ullah
        [HttpGet]
         public async Task<ResponseModel> GetProvidersBYPractice(int PracticeId)
        {
            return await _reportService.GetProvidersByPractice(PracticeId);
        }

        //[HttpGet]
        //public ResponseModel CollectionAnalysisReport(long? PracticeCode, DateTime? DateFrom, DateTime? DateTo)
        //{
        //    return _reportService.CollectionAnalysisReport(PracticeCode, DateFrom, DateTo);
        //}

        [HttpGet]
        public Task<ResponseModel> CollectionAnalysisReport(long PracticeCode, DateTime? DateFrom, DateTime? DateTo, bool isExport = false, int page = 1, int size = 10)
        {
            return _reportService.CollectionAnalysisReport(PracticeCode, DateFrom, DateTo, isExport, page, size);
        }

        [HttpGet]

        public Task<ResponseModel> PatientAgingReport(long PracticeCode, bool isExport = false, int page=1, int size = 10)
        {
            return _reportService.PatientAgingReport(PracticeCode, isExport, page, size);
        }
        [HttpGet]
        public Task<ResponseModel> PatientAgingAnalysisReport(long PracticeCode, bool isExport = false, int page = 1, int size = 10)
        {
            return _reportService.PatientAgingAnalysisReport(PracticeCode, isExport, page, size);
        }

        [HttpGet]
        public ResponseModel DenialReportDetailPagination(long? PracticeCode,string Criteria, DateTime? DateFrom, DateTime? DateTo, int page, int count)
        {
            return _reportService.DenialReportDetailPagination(PracticeCode, Criteria, DateFrom, DateTo, page, count);
        }

        [HttpGet]
        public ResponseModel DenialReportDetail(long? PracticeCode, string Criteria, DateTime? DateFrom, DateTime? DateTo)
        {
            return _reportService.DenialReportDetail(PracticeCode, Criteria, DateFrom, DateTo);
        }
        [HttpGet]
        public Task<ResponseModel> GetNegativeBalanceReport(long practiceCode, string responsibleParty, string dateCriteria, DateTime dateFrom, DateTime dateTo, Boolean isExport, int pageSize, int PageNo)
        {
            return _reportService.GetNegativeBalanceReport(practiceCode, responsibleParty, dateCriteria, dateFrom, dateTo, isExport, pageSize, PageNo);
        }
        [HttpGet]
        public ResponseModel GetEshaCBRDetails(long? practiceCode, int Page, int Count, bool perPageRecord)
        {
            return _reportService.GetEshaCBRDetails(practiceCode, Page, Count, perPageRecord);
        }
        public ResponseModel GetPatientAgingSummaryDetails(long? practiceCode, int Page, int Count, bool perPageRecord)
        {
            return _reportService.GetPatientAgingSummaryDetails(practiceCode, Page, Count, perPageRecord);
        }
        public ResponseModel GetPlanAgingReportDetails(long? PracticeCode, int Page, int Count, bool perPageRecord)
        {
            return _reportService.GetPlanAgingReportDetails(PracticeCode, Page, Count, perPageRecord);

        }


        #endregion

        #region Charges

        [HttpPost]
        public ResponseModel OverAllChargesDos(ReportRequestModel req)
        {
            return _reportService.OverAllChargesDos(req, GetUserId());
        }

        [HttpPost]
        public ResponseModel ByCPTDos(ReportRequestModel req)
        {
            return _reportService.ByCPTDos(req, GetUserId());
        }

        [HttpPost]
        public ResponseModel ByHospitalDos(ReportRequestModel req)
        {
            return _reportService.ByHospitalDos(req, GetUserId());
        }

        [HttpPost]
        public ResponseModel ByPrimaryDXDos(ReportRequestModel req)
        {
            return _reportService.ByPrimaryDXDos(req, GetUserId());
        }

        [HttpPost]
        public ResponseModel ByCarrierDOS(ReportRequestModel req)
        {
            return _reportService.ByCarrierDos(req, GetUserId());
        }

        [HttpPost]
        public ResponseModel CPA(ReportRequestModel req)
        {
            return _reportService.CPA(req, GetUserId());
        }

        //CLAIMS AND ACCOUNTS ASSIGNMENT

        [HttpGet]
        public ResponseModel ClaimAssignmentReport(long PracticeCode, string DateFrom, string DateTo)
        {
            return _reportService.ClaimAssignmentReport(PracticeCode, DateFrom, DateTo);
        }

        [HttpGet]
        public ResponseModel ERAUnpostedAdjReport(long PracticeCode, string DateFrom, string DateTo)
        {
            return _reportService.ERAUnpostedAdjReport(PracticeCode, DateFrom, DateTo);
        }

        [HttpGet]
        public ResponseModel AccounAssignmentReport(long PracticeCode, string DateFrom, string DateTo)
        {
            return _reportService.AccounAssignmentReport(PracticeCode, DateFrom, DateTo);
        }
        //added by Samiullah
        [HttpGet]
        public ResponseModel AccounAssignmentReportPagination(long PracticeCode, string DateFrom, string DateTo,int page, int size)
        {
            return _reportService.AccounAssignmentReportPagination(PracticeCode, DateFrom, DateTo,page,size);
        }
        //added By Sami Ullah
        [HttpPost]
        public async Task<ResponseModel> GetVisitClaimActivityReport(PatelReportsRequestModel req)
        {
            return await _reportService.VisitClaimActivityReport(req);
        }
        [HttpPost]
        public async Task<ResponseModel> GetChargesBreakDownReport(PatelReportsRequestModel req)
        {
            return await _reportService.GetChargesBreakDownReport(req);
        }
        #endregion

        #region Payments

        [HttpPost]
        public ResponseModel PaymentDailyRefresh(ReportRequestModel req)
        {
            return _reportService.PaymentDailyRefresh(req, GetUserId());
        }

        [HttpPost]
        public ResponseModel PaymentMonthWise(ReportRequestModel req)
        {
            return _reportService.PaymentMonthWise(req, GetUserId());
        }

        [HttpPost]
        public ResponseModel PaymentByCarrier(ReportRequestModel req)
        {
            return _reportService.PaymentByCarrier(req, GetUserId());
        }

        [HttpPost]
        public ResponseModel PaymentPrimaryDX(ReportRequestModel req)
        {
            return _reportService.PaymentPrimaryDX(req, GetUserId());
        }

        [HttpPost]
        public ResponseModel PaymentByPrimaryICD10(ReportRequestModel req)
        {
            return _reportService.PaymentByPrimaryICD10(req, GetUserId());
        }
        #endregion 
    }
}