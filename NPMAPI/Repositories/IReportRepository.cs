using System;
using System.Threading.Tasks;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;

namespace NPMAPI.Repositories
{
    public interface IReportRepository
    {
        ResponseModel AgingSummaryAnalysisReport(long PracticeCode);
        Task<ResponseModel> DormantClaimsReport(long PracticeCode, int page, int size);
        ResponseModel DormantClaimsReports(long Claim_no);
        ResponseModel DormantClaimsReportsPagination(long PracticeCode);
        ResponseModel AgingSummaryPatientAnalysisReport(long PracticeCode);
        ResponseModel FinancialAnalysisCPTLevelReport(long PracticeCode, DateTime DateFrom, DateTime DateTo);
        ResponseModel FinancialAnalysisByProviderandProcCodesReport(long PracticeCode, DateTime DateFrom, DateTime DateTo);
        ResponseModel RecallVisits(ReportRequestModel req, long v);
        ResponseModel PeriodAnalysisAndClosing(ReportRequestModel req, long v);
        ResponseModel PracticeAnalysis(ReportRequestModel req, long v);
        ResponseModel PatientBirthDays(ReportRequestModel req, long v);
        ResponseModel OverAllChargesDos(ReportRequestModel req, long v);
        ResponseModel ByCPTDos(ReportRequestModel req, long v);
        ResponseModel ByHospitalDos(ReportRequestModel req, long v);
        ResponseModel ByPrimaryDXDos(ReportRequestModel req, long v);
        ResponseModel ByCarrierDos(ReportRequestModel req, long v);
        ResponseModel PaymentMonthWise(ReportRequestModel req, long v);
        ResponseModel PaymentDailyRefresh(ReportRequestModel req, long v);
        ResponseModel PaymentByCarrier(ReportRequestModel req, long v);
        ResponseModel PaymentPrimaryDX(ReportRequestModel req, long v);
        ResponseModel PaymentByPrimaryICD10(ReportRequestModel req, long v);
        ResponseModel PaymentDetail(long? PracticeCode, DateTime? DateFrom, DateTime? DateTo, long? PatientAccount);
        //added by Sami Ullah
        ResponseModel PaymentDetailPagination(long? PracticeCode, DateTime? DateFrom, DateTime? DateTo, long? PatientAccount,int page,int size);
        ResponseModel AgingSummaryRecent(ReportRequestModel req, long v);
        //..below code is added by pir-ubaid - dashboard revamp
        ResponseModel GetCPAAnalysis(long? practiceCode, string fromDate, string toDate, long v);
        ResponseModel GetInsuranceAging(long? practiceCode, long v);
        ResponseModel GetClaimsAndERAs(long? practiceCode, string fromDate, string toDate, long v);
        ResponseModel GetPatientAging(long? practiceCode, long v);
        ResponseModel GetPCRatio(long? practiceCode, string fromDate, string toDate, long v);
        ResponseModel GetChargesANDPaymentsTrend(long? practiceCode, string fromDate, string toDate, long v);

        //..
        ResponseModel ChargesPaymentsRecent(ReportRequestModel req, long v);
        ResponseModel CPTWisePaymentDetail(long? practiceCode, DateTime? dateFrom, DateTime? dateTo);
        ResponseModel CPTWisePaymentDetailPagination(long? practiceCode, DateTime? dateFrom, DateTime? dateTo,int Page,int Count);
        ResponseModel GetAgingDashboard(ReportRequestModel req, long v);
        ResponseModel GetInsuranceDetailReport(long? PracCode, long v);
        //added by Samiullah
        ResponseModel GetInsuranceDetailReportPagination(long? PracCode, long v,int page, int size);
        ResponseModel GetUserReport(string PracCode, string userid, string dateFrom, string dateTo);
        //added by Samiullah
        ResponseModel GetUserReportPagination(string PracCode, string userid, string dateFrom, string dateTo,int page,int size);
        ResponseModel CPA(ReportRequestModel req, long v);
        ResponseModel AppointmentDetailReport(long practiceCode, string dateFrom, string dateTo);
        ResponseModel MissingAppointmentDetailReport(long practiceCode, string dateFrom, string dateTo);
        ResponseModel holdReport(long? PracCode);
        ResponseModel ClaimPaymentsDetailReport(ClaimPaymentsDetailRequest request);

        ResponseModel ClaimAssignmentReport(long PracticeCode, string DateFrom, string DateTo);
        ResponseModel AccounAssignmentReport(long practiceCode, string dateFrom, string dateTo);
        ResponseModel ERAUnpostedAdjReport(long practiceCode, string dateFrom, string dateTo);
        //added by SamiUllah
        ResponseModel AccounAssignmentReportPagination(long practiceCode, string dateFrom, string dateTo,int page,int size);
        ResponseModel GetRollingSummaryReport(string PracCode, string duration);

        Task<ResponseModel> CollectionAnalysisReport(long PracticeCode, DateTime? DateFrom, DateTime? DateTo, bool isExport = false, int page = 1, int size = 10);
        Task<ResponseModel> PatientAgingReport(long PracticeCode, bool isExport = false, int page = 1, int size = 10);
        Task<ResponseModel> PatientAgingAnalysisReport(long PracticeCode, bool isExport = false, int page = 1, int size = 10);
        //added By Samiullah
        Task<ResponseModel> VisitClaimActivityReport(PatelReportsRequestModel request);
        //added By Samiullah
        Task<ResponseModel> GetProvidersByPractice(int PracticeId);

        Task<ResponseModel> GetChargesBreakDownReport(PatelReportsRequestModel request);
        Task<ResponseModel> GetNegativeBalanceReport(long practiceCode, string responsibleParty, string dateCriteria, DateTime dateFrom, DateTime dateTo, Boolean isExport, int pageSize, int PageNo);
     
             

        //Task<ResponseModel> DenialReportDetailPagination(long? practiceCode,string Criteria, DateTime? dateFrom, DateTime? dateTo, int Page, int Count);
        ResponseModel DenialReportDetailPagination(long? practiceCode,string criteria, DateTime? dateFrom, DateTime? dateTo, int Page, int Count);

        ResponseModel DenialReportDetail(long? practiceCode, string criteria, DateTime? dateFrom, DateTime? dateTo);

        ResponseModel GetEshaCBRDetails(long? practiceCode, int Page, int Count, bool perPageRecord);

        ResponseModel GetPatientAgingSummaryDetails(long? practiceCode, int Page, int Count, bool perPageRecord);

        ResponseModel GetPlanAgingReportDetails(long? PracticeCode, int Page, int Count, bool perPageRecord);



    }
}
