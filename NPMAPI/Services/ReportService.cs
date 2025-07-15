using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI;
using EdiFabric.Templates.Hipaa5010;
using Microsoft.AspNet.SignalR.Hubs;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;
using NPOI.SS.Formula.Functions;
using NPOI.Util;
using Org.BouncyCastle.Ocsp;

namespace NPMAPI.Services
{
    public class ReportService : IReportRepository
    {
        #region Reports
        public string sp_name = "";
        public bool hasMatch = false;

        [HttpGet]
        public ResponseModel AgingSummaryAnalysisReport(long PracticeCode)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Aging_Summary_Analysis_Reporting_Result> objAgingAnalysisReport = null;
                using (var ctx = new NPMDBEntities())
                {
                    objAgingAnalysisReport = ctx.Aging_Summary_Analysis_Reporting(PracticeCode).ToList();
                }

                if (objAgingAnalysisReport != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objAgingAnalysisReport;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return objResponse;
        }

        public async Task<ResponseModel> DormantClaimsReport(long PracticeCode, int page, int size)
        {
            ResponseModel objResponse = new ResponseModel();
            var pagingResponse = new PagingResponse();
            try
            {
                List<SP_PATIENTSTATEMENTCOUNT_Result> objDormantClaimsReport = null;
                using (var ctx = new NPMDBEntities())
                {

                    objDormantClaimsReport = ctx.SP_PATIENTSTATEMENTCOUNT(PracticeCode)
                        .OrderByDescending(s => s.CLAIM_NO)
                        .Skip((page - 1) * size).Take(size).ToList();
                    pagingResponse.TotalRecords = ctx.SP_PATIENTSTATEMENTCOUNT(PracticeCode).Count();
                    pagingResponse.FilteredRecords = objDormantClaimsReport.Count(); // Count after pagination
                    pagingResponse.CurrentPage = page;
                    pagingResponse.data = objDormantClaimsReport;
                    
                }

                objResponse.Status = "success";
                objResponse.Response = pagingResponse;
            }
            catch (Exception)
            {
                throw;
            }
            return objResponse;
        }


        [HttpGet]
        public ResponseModel DormantClaimsReports(long Claim_no)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<SP_PATIENTSTATEMENTCOUNT_BYCLAIM_Result> objDormantClaimsReport = null;
                using (var ctx = new NPMDBEntities())
                {
                    objDormantClaimsReport = ctx.SP_PATIENTSTATEMENTCOUNT_BYCLAIM(Claim_no).ToList();
                }

                if (objDormantClaimsReport != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objDormantClaimsReport;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return objResponse;
        }

        [HttpGet]
        public ResponseModel DormantClaimsReportsPagination(long PracticeCode)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<SP_PATIENTSTATEMENTCOUNT_Result> objDormantClaimsReport = null;
                using (var ctx = new NPMDBEntities())
                {
                    objDormantClaimsReport = ctx.SP_PATIENTSTATEMENTCOUNT(PracticeCode).ToList();
                }

                if (objDormantClaimsReport != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objDormantClaimsReport;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return objResponse;
        }
        [HttpGet]
        public ResponseModel AgingSummaryPatientAnalysisReport(long PracticeCode)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Aging_Summary_Analysis_Report_Patient_Result> objAgingAnalysisReport = null;
                using (var ctx = new NPMDBEntities())
                {
                    objAgingAnalysisReport = ctx.Aging_Summary_Analysis_Report_Patient(PracticeCode).ToList();
                }

                if (objAgingAnalysisReport != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objAgingAnalysisReport;
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;

            }
            catch (Exception)
            {
                throw;
            }
            return objResponse;
        }

        [HttpGet]
        public ResponseModel FinancialAnalysisByProviderandProcCodesReport(long PracticeCode, DateTime DateFrom, DateTime DateTo)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Financial_Analysis_by_Provider_and_Procedure_Codes_Result> objFinancialAnalysisReport = null;
                using (var ctx = new NPMDBEntities())
                {
                    objFinancialAnalysisReport = ctx.Financial_Analysis_by_Provider_and_Procedure_Codes(PracticeCode, DateFrom, DateTo).ToList();
                }

                if (objFinancialAnalysisReport != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objFinancialAnalysisReport;
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public ResponseModel FinancialAnalysisCPTLevelReport(long PracticeCode, DateTime DateFrom, DateTime DateTo)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Financial_Analysis_At_CPT_Level_Result> objFinancialAnalysisReport = null;
                using (var ctx = new NPMDBEntities())
                {
                    objFinancialAnalysisReport = ctx.Financial_Analysis_At_CPT_Level(PracticeCode, DateFrom, DateTo).ToList();
                }

                if (objFinancialAnalysisReport != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objFinancialAnalysisReport;
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public ResponseModel PaymentDetail(long? PracticeCode, DateTime? DateFrom, DateTime? DateTo, long? PatientAccount)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var paymentDetailList = ctx.Payment_Detail(PracticeCode, DateFrom, DateTo, PatientAccount).OrderByDescending(x => x.patient_account).ToList();
                    if (paymentDetailList.Count > 0)
                    {
                        foreach (var pay in paymentDetailList)
                        {
                            pay.amount_adjusted = pay.amount_adjusted == null ? 0 : pay.amount_adjusted;
                            pay.Amount_Paid = pay.Amount_Paid == null ? 0 : pay.Amount_Paid;
                            pay.reject_amount = pay.reject_amount == null ? 0 : pay.reject_amount;
                        }
                        var responseList = paymentDetailList.Select(c => new PaymentDetailResponse
                        {
                                practice_code=c.practice_code,
                                Practice_Name=c.prac_name,
                                claim_no=c.claim_no,
                                dos=c.dos,
                                Patient_Name=c.Patient_Name,
                                patient_account=c.patient_account,
                                Billing_Provider=c.Billing_Provider,
                                date_entry=c.date_entry,
                                Amount_Paid=c.Amount_Paid,
                                amount_adjusted=c.amount_adjusted,
                                Amount_rejected=c.reject_amount,
                                payment_type=c.payment_type,
                                Payment_Source=c.Payment_Source,
                                Cheque_Date=c.Cheque_Date,
                                Cheque_No=c.check_no
                        }).ToList();
                        res.Status = "Success";
                        res.Response = responseList;
                    }
                    else
                    {
                        res.Status = "Failed";
                        res.Response = "No Records";
                    }
                    return res;
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        //added by Sami ullah
        public ResponseModel PaymentDetailPagination(long? PracticeCode, DateTime? DateFrom, DateTime? DateTo, long? PatientAccount, int page, int size)
        {
            ResponseModel res = new ResponseModel();
            var pagingResponse = new PagingResponse();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var paymentDetailList = ctx.Payment_Detail(PracticeCode, DateFrom, DateTo, PatientAccount)
                        .OrderByDescending(x=>x.patient_account)
                         .Skip((page - 1) * size).Take(size).ToList();
                    pagingResponse.TotalRecords = ctx.Payment_Detail(PracticeCode, DateFrom, DateTo, PatientAccount).Count();
                    pagingResponse.FilteredRecords = paymentDetailList.Count(); // Count after pagination
                    pagingResponse.CurrentPage = page;
                    if (paymentDetailList.Count > 0)
                    {
                        foreach (var pay in paymentDetailList)
                        {
                            pay.amount_adjusted = pay.amount_adjusted == null ? 0 : pay.amount_adjusted;
                            pay.Amount_Paid = pay.Amount_Paid == null ? 0 : pay.Amount_Paid;
                            pay.reject_amount = pay.reject_amount == null ? 0 : pay.reject_amount;
                        }
                        var responseList = paymentDetailList.Select(c => new PaymentDetailResponse
                        {
                            practice_code = c.practice_code,
                            Practice_Name = c.prac_name,
                            claim_no = c.claim_no,
                            dos = c.dos,
                            Patient_Name = c.Patient_Name,
                            patient_account = c.patient_account,
                            Billing_Provider = c.Billing_Provider,
                            date_entry = c.date_entry,
                            Amount_Paid = c.Amount_Paid,
                            amount_adjusted = c.amount_adjusted,
                            Amount_rejected = c.reject_amount,
                            payment_type = c.payment_type,
                            Payment_Source = c.Payment_Source,
                            Cheque_Date = c.Cheque_Date,
                            Cheque_No = c.check_no
                        }).ToList();
                        pagingResponse.data = responseList;
                        res.Status = "Success";
                        res.Response = pagingResponse;
                    }
                    else
                    {
                        res.Status = "Failed";
                        res.Response = "No Records";
                    }
                    return res;
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        public ResponseModel CPTWisePaymentDetail(long? PracticeCode, DateTime? DateFrom, DateTime? DateTo)
        {
            ResponseModel res = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    List<Payment_detail_Proc_Result> cptPaymentDetailList = ctx.Payment_detail_Proc(PracticeCode, DateFrom, DateTo).ToList();
                    if (cptPaymentDetailList.Count > 0)
                    {
                        foreach (var pay in cptPaymentDetailList)
                        {
                            pay.AMOUNT_ADJUSTED = pay.AMOUNT_ADJUSTED == null ? 0 : pay.AMOUNT_ADJUSTED;
                            pay.AMOUNT_PAID = pay.AMOUNT_PAID == null ? 0 : pay.AMOUNT_PAID;
                            pay.REJECT_AMOUNT = pay.REJECT_AMOUNT == null ? 0 : pay.REJECT_AMOUNT;
                        }
                        var responseList = cptPaymentDetailList.Select(c => new Payment_detail_Proc_Response
                        {
                         PRACTICE_CODE=c.Practice_Code,
                         PRACTICE_NAME=c.Prac_Name,
                         CLAIM_NO=c.CLAIM_NO,
                         LOCATION_NAME=c.LOCATION_NAME,
                         FACILITY=c.FACILITY_NAME,
                         DOS=c.DOS,
                         PATIENT_NAME=c.Patient_name,
                         PATIENT_ACCOUNT = c.PATIENT_ACCOUNT,
                         ATTENDING_PHYSICIAN =c.Attending_Physician,
                         BILLING_PROVIDER=c.Billing_PROVIDER,
                         DATE_ENTRY=c.DATE_ENTRY,
                         AMOUNT_PAID=c.AMOUNT_PAID,
                         AMOUNT_ADJUSTED=c.AMOUNT_ADJUSTED,
                         AMOUNT_REJECTED=c.REJECT_AMOUNT,
                         DOS_FROM=c.Dos_From,
                         DOS_TO=c.Dos_To,
                         PAYMENT_TYPE=c.Payment_Type,
                         PAYMENT_SOURCE=c.Payment_Source,
                         INSURANCE_NAME=c.Insurance_Name,
                         CHEQUE_DATE=c.Cheque_Date,
                         CHEQUE_NO=c.Check_No,
                         CPT=c.Procedure_Code,
                        }).ToList();
                        res.Status = "Success";
                        res.Response = responseList;
                    }
                    else
                    {
                        res.Status = "Failed";
                        res.Response = "No Records";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return res;
        }
        public ResponseModel CPTWisePaymentDetailPagination(long? PracticeCode, DateTime? DateFrom, DateTime? DateTo,  int Page, int Count)
        {
            ResponseModel res = new ResponseModel();
            var pagingResponse = new PagingResponse();
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    List<Payment_detail_Proc_Result> cptPaymentDetailList = ctx.Payment_detail_Proc(PracticeCode, DateFrom, DateTo)  
                          
                           .Skip((Page - 1) * Count).Take(Count).ToList();
                    pagingResponse.TotalRecords = ctx.Payment_detail_Proc(PracticeCode, DateFrom, DateTo).Count();
                    pagingResponse.FilteredRecords = cptPaymentDetailList.Count(); // Count after pagination
                    pagingResponse.CurrentPage = Page;
                   

 
                if (cptPaymentDetailList.Count > 0)
                    {
                        foreach (var pay in cptPaymentDetailList)
                        {
                            pay.AMOUNT_ADJUSTED = pay.AMOUNT_ADJUSTED == null ? 0 : pay.AMOUNT_ADJUSTED;
                            pay.AMOUNT_PAID = pay.AMOUNT_PAID == null ? 0 : pay.AMOUNT_PAID;
                            pay.REJECT_AMOUNT = pay.REJECT_AMOUNT == null ? 0 : pay.REJECT_AMOUNT;
                        }
                        var responseList = cptPaymentDetailList.Select(c => new Payment_detail_Proc_Response
                        {
                            PRACTICE_CODE = c.Practice_Code,
                            PRACTICE_NAME = c.Prac_Name,
                            CLAIM_NO = c.CLAIM_NO,
                            LOCATION_NAME = c.LOCATION_NAME,
                            FACILITY = c.FACILITY_NAME,
                            DOS = c.DOS,
                            PATIENT_NAME = c.Patient_name,
                            PATIENT_ACCOUNT=c.PATIENT_ACCOUNT,
                            ATTENDING_PHYSICIAN = c.Attending_Physician,
                            BILLING_PROVIDER = c.Billing_PROVIDER,
                            DATE_ENTRY = c.DATE_ENTRY,
                            AMOUNT_PAID = c.AMOUNT_PAID,
                            AMOUNT_ADJUSTED = c.AMOUNT_ADJUSTED,
                            AMOUNT_REJECTED = c.REJECT_AMOUNT,
                            DOS_FROM = c.Dos_From,
                            DOS_TO = c.Dos_To,
                            PAYMENT_TYPE = c.Payment_Type,
                            PAYMENT_SOURCE = c.Payment_Source,
                            INSURANCE_NAME = c.Insurance_Name,
                            CHEQUE_DATE = c.Cheque_Date,
                            CHEQUE_NO = c.Check_No,
                            CPT = c.Procedure_Code,
                        }).ToList();
                        pagingResponse.data = responseList;
                        res.Status = "Success";
                        res.Response = pagingResponse;
                    }
                    else
                    {
                        res.Status = "Failed";
                        res.Response = "No Records";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return res;
        }
        public ResponseModel DenialReportDetailPagination(long? PracticeCode,string Criteria, DateTime? DateFrom, DateTime? DateTo, int Page, int Count)
        {
            ResponseModel res = new ResponseModel();
            var pagingResponse = new PagingResponse();
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    List<USP_Denial_Report_Result> denialDetailList = ctx.USP_Denial_Report(PracticeCode, Criteria, DateFrom, DateTo)

                           .Skip((Page - 1) * Count).Take(Count).ToList();
                    pagingResponse.TotalRecords = ctx.USP_Denial_Report(PracticeCode, Criteria, DateFrom, DateTo).Count();
                    pagingResponse.FilteredRecords = denialDetailList.Count(); // Count after pagination
                    pagingResponse.CurrentPage = Page;



                    if (denialDetailList.Count > 0)
                    {
                        var responseList = denialDetailList.Select(c => new Denial_Report_Response
                        {

                            PRACTICE_CODE = c.PRACTICE_CODE,
                            PRACTICE_NAME = c.PRAC_NAME,
                            CLAIM_NUMBER = c.CLAIM_NO,
                            DOS = c.DOS,
                            CLAIM_DOE = c.ClAIM_DOE,
                            PATIENT_NAME = c.PATIENT_NAME,
                            PATIENT_ACCOUNT = c.PATIENT_ACCOUNT,
                            BILLING_PROVIDER = c.BILLING_PROVIDER,
                            RESOURCE_PROVIDER = c.RESOURCE_PROVIDER,
                            DENIAL_DATE = c.DENIAL_DATE,
                            PROCEDURE_CODE = c.Procedurecode,
                            AMOUNT_PAID = c.AMOUNT_PAID,
                            AMOUNT_ADJUSTED = c.AMOUNT_ADJUSTED,
                            REJECT_AMOUNT = c.REJECT_AMOUNT,
                            PAYMENT_TYPE = c.PAYMENT_TYPE,
                            PAYMENT_SOURCE = c.PAYMENT_SOURCE_DESCRIPTION,
                            CHEQUE_DATE = c.CHECK_DATE,
                            CHEQUE_NO = c.CHECK_NO,
                            DENIAL_CODE = c.DENIAL_CODE,
                            DENIAL_CODE_DESCRIPTION = c.DENIAL_CODE_DESCRIPTION,
                        }).ToList();
                        pagingResponse.data = responseList;
                        res.Status = "Success";
                        res.Response = pagingResponse;
                    }
                    else
                    {
                        res.Status = "Failed";
                        res.Response = "No Records";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return res;
        }
        public ResponseModel DenialReportDetail(long? PracticeCode, string Criteria, DateTime? DateFrom, DateTime? DateTo)
        {
            ResponseModel res = new ResponseModel();
            var pagingResponse = new PagingResponse();
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    List<USP_Denial_Report_Result> denialDetailList = ctx.USP_Denial_Report(PracticeCode, Criteria, DateFrom, DateTo).ToList();
                 
                    if (denialDetailList.Count > 0)
                    {
                        var responseList = denialDetailList.Select(c => new Denial_Report_Response
                        {

                            PRACTICE_CODE = c.PRACTICE_CODE,
                            PRACTICE_NAME = c.PRAC_NAME,
                            CLAIM_NUMBER = c.CLAIM_NO,
                            DOS = c.DOS,
                            CLAIM_DOE = c.ClAIM_DOE,
                            PATIENT_NAME = c.PATIENT_NAME,
                            PATIENT_ACCOUNT = c.PATIENT_ACCOUNT,
                            BILLING_PROVIDER = c.BILLING_PROVIDER,
                            RESOURCE_PROVIDER = c.RESOURCE_PROVIDER,
                            DENIAL_DATE = c.DENIAL_DATE,
                            PROCEDURE_CODE = c.Procedurecode,
                            AMOUNT_PAID = c.AMOUNT_PAID,
                            AMOUNT_ADJUSTED = c.AMOUNT_ADJUSTED,
                            REJECT_AMOUNT = c.REJECT_AMOUNT,
                            PAYMENT_TYPE = c.PAYMENT_TYPE,
                            PAYMENT_SOURCE = c.PAYMENT_SOURCE_DESCRIPTION,
                            CHEQUE_DATE = c.CHECK_DATE,
                            CHEQUE_NO = c.CHECK_NO,
                            DENIAL_CODE = c.DENIAL_CODE,
                            DENIAL_CODE_DESCRIPTION = c.DENIAL_CODE_DESCRIPTION,
                        }).ToList();
                        pagingResponse.data = responseList;
                        res.Status = "Success";
                        res.Response = pagingResponse;
                    }
                    else
                    {
                        res.Status = "Failed";
                        res.Response = "No Records";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return res;
        }

        public ResponseModel GetEshaCBRDetails(long? practicecode, int page, int count, bool perpagerecord)
        {
            ResponseModel res = new ResponseModel();
            var pagingresponse = new PagingResponse();
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    List<USP_NPM_CREDIT_BALANCE_Result> detaillist = null;
                    if (perpagerecord)
                    {
                        detaillist = ctx.USP_NPM_CREDIT_BALANCE(practicecode).Skip((page - 1) * count).Take(count).ToList();
                        pagingresponse.TotalRecords = ctx.USP_NPM_CREDIT_BALANCE(practicecode).Count();
                        pagingresponse.FilteredRecords = detaillist.Count(); // count after pagination
                        pagingresponse.CurrentPage = page;
                    }
                    else
                    {
                        detaillist = ctx.USP_NPM_CREDIT_BALANCE(practicecode).ToList();

                    }



                    if (detaillist.Count > 0)
                    {
                        var responselist = detaillist.Select(c => new Credit_BR_Response
                        {

                            PRACTICE_CODE = c.PRACTICE_CODE,
                            PRAC_NAME = c.PRAC_NAME,
                            PATIENT_NAME = c.PATIENT_NAME,
                            PATIENT_ACCOUNT = c.PATIENT_ACCOUNT,
                            BILL_TO = c.BILL_TO,
                            LAST_CHARGE_DATE = c.LAST_CHARGE_DATE,
                            LAST_PAYMENT_DATE = c.LAST_PAYMENT_DATE,
                            LAST_STATEMENT_DATE = c.LAST_STATEMENT_DATE,
                            LAST_CLAIM_DATE = c.LAST_CLAIM_DATE,
                            BALANCE = c.BALANCE,

                        }).ToList();
                        pagingresponse.data = responselist;
                        res.Status = "Success";
                        res.Response = pagingresponse;
                    }
                    else
                    {
                        res.Status = "Failed";
                        res.Response = "No Records";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return res;
        }

        public ResponseModel GetPatientAgingSummaryDetails(long? PracticeCode, int Page, int Count, bool perPageRecord)
        {
            ResponseModel res = new ResponseModel();
            var pagingResponse = new PagingResponse();
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    List<USP_NPM_PATIENT_AGING_Result> detailList = null;
                    if (perPageRecord)
                    {
                        detailList = ctx.USP_NPM_PATIENT_AGING(PracticeCode).Skip((Page - 1) * Count).Take(Count).ToList();
                        pagingResponse.TotalRecords = ctx.USP_NPM_PATIENT_AGING(PracticeCode).Count();
                        pagingResponse.FilteredRecords = detailList.Count(); // Count after pagination
                        pagingResponse.CurrentPage = Page;
                    }
                    else
                    {
                        detailList = ctx.USP_NPM_PATIENT_AGING(PracticeCode).ToList();

                    }



                    if (detailList.Count > 0)
                    {
                        var responseList = detailList.Select(c => new Patient_ASR_Response
                        {

                            PRACTICE_CODE = c.PRACTICE_CODE,
                            PRAC_NAME = c.PRAC_NAME,
                            PATIENT_NAME = c.PATIENT_NAME,
                            PATIENT_ACCOUNT = c.PATIENT_ACCOUNT,
                            POLICY_NUMBER = c.POLICY_NUMBER,
                            BALANCE = c.BALANCE,
                            Current = c.Current,
                            C30_Days = c.C30_Days,
                            C60_Days = c.C60_Days,
                            C90_Days = c.C90_Days,
                            C120_Days = c.C120_Days,

                        }).ToList();
                        pagingResponse.data = responseList;
                        res.Status = "Success";
                        res.Response = pagingResponse;
                    }
                    else
                    {
                        res.Status = "Failed";
                        res.Response = "No Records";
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return res;
        }



        public ResponseModel GetPlanAgingReportDetails(long? PracticeCode, int Page, int Count, bool perPageRecord)
        {
            ResponseModel res = new ResponseModel();
            var pagingResponse = new PagingResponse();
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    List<USP_NPM_PLAN_AGING_Result> detailList = null;
                    if (perPageRecord)
                    {
                        detailList = ctx.USP_NPM_PLAN_AGING(PracticeCode).Skip((Page - 1) * Count).Take(Count).ToList();
                        pagingResponse.TotalRecords = ctx.USP_NPM_PLAN_AGING(PracticeCode).Count();
                        pagingResponse.FilteredRecords = detailList.Count(); // Count after pagination
                        pagingResponse.CurrentPage = Page;
                    }
                    else
                    {
                        detailList = ctx.USP_NPM_PLAN_AGING(PracticeCode).ToList();

                    }



                    if (detailList.Count > 0)
                    {
                        var responseList = detailList.Select(c => new Plan_Aging_Response
                        {

                            PRACTICE_CODE = c.PRACTICE_CODE,
                            PRAC_NAME = c.PRAC_NAME,
                            GROUP_NAME = c.GROUP_NAME,
                            AGING_PAYER = c.AGING_PAYER,
                            BALANCE = c.BALANCE,
                            Current = c.Current,
                            C30_Days = c.C30_Days,
                            C60_Days = c.C60_Days,
                            C90_Days = c.C90_Days,
                            C120_Days = c.C120_Days,

                        }).ToList();
                        pagingResponse.data = responseList;
                        res.Status = "Success";
                        res.Response = pagingResponse;
                    }
                    else
                    {
                        res.Status = "Failed";
                        res.Response = "No Records";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return res;
        }


        public ResponseModel AppointmentDetailReport(long PracticeCode, string DateFrom, string DateTo)
        {
            ResponseModel res = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    List<SP_Appointment_Detail_Report_Result> appDtlList = ctx.SP_Appointment_Detail_Report(PracticeCode, DateFrom, DateTo).ToList();
                    if (appDtlList.Count > 0)
                    {
                        res.Status = "Success";
                        res.Response = appDtlList;
                    }
                    else
                    {
                        res.Status = "Failed";
                        res.Response = "No Records";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return res;
        }

        public ResponseModel ClaimPaymentsDetailReport(ClaimPaymentsDetailRequest request)
        {
            ResponseModel res = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    //add sp here
                    res.Response = new
                    {
                        paymentId = 012,
                        postedBy = "",
                        paymentFrom = "",
                        paymentType = "",
                        checkNo = 1231,
                        checkDate = DateTime.Now.Day,
                        depositDate = DateTime.Now.Day,
                        totalAmount = 230,
                        postedAmount = 500
                    };
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return res;
        }

        public ResponseModel MissingAppointmentDetailReport(long PracticeCode, string DateFrom, string DateTo)
        {
            ResponseModel res = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    List<SP_Missing_Appointment_Report_Result> appDtlList = ctx.SP_Missing_Appointment_Report(PracticeCode, DateFrom, DateTo).ToList();
                    if (appDtlList.Count > 0)
                    {
                        res.Status = "Success";
                        res.Response = appDtlList;
                    }
                    else
                    {
                        res.Status = "Failed";
                        res.Response = "No Records";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return res;
        }

        bool AddProperty(ExpandoObject obj, string key, object value)
        {
            var dynamicDict = obj as IDictionary<string, object>;
            if (dynamicDict.ContainsKey(key))
                return false;
            else
                dynamicDict.Add(key, value);
            return true;
        }

        public ResponseModel OverAllChargesDos(ReportRequestModel req, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                CultureInfo culture = new CultureInfo("en-US");
              
                DateTime dDateFrom = Convert.ToDateTime(req.DateFrom, culture);
                DateTime dDateTo = Convert.ToDateTime(req.DateTo, culture);
                //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                findPractice(req.PracticeCode);
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    //Updated by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                    sp_name = hasMatch ? "OVER_ALL_CHARGES_DOS" : "USP_NPM_OVER_ALL_CHARGES";
                    cmd.CommandText = "exec " + sp_name + " @PracticeCode, @DATEFROM, @DATETO, @locationcode, @DateType";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", req.PracticeCode));
                    cmd.Parameters.Add(new SqlParameter("DATEFROM", dDateFrom));
                    cmd.Parameters.Add(new SqlParameter("DATETO", dDateTo));
                    cmd.Parameters.Add(new SqlParameter("locationcode", string.Join(",", req.LocationCode)));
                    cmd.Parameters.Add(new SqlParameter("DateType", req.DateType));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Status = "success";
                    res.Response = list;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel ByCPTDos(ReportRequestModel req, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                DateTime dDateTo;
                DateTime dDateFrom;
                DateTime.TryParse(req.DateFrom, out dDateFrom);
                DateTime.TryParse(req.DateTo, out dDateTo);
                //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                findPractice(req.PracticeCode);
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    //Updated by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                    sp_name = hasMatch ? "By_Cpt_DOS" : "USP_NPM_By_Cpt_DOS";
                    cmd.CommandText = "exec " + sp_name + " @PracticeCode, @DATEFROM, @DATETO, @locationcode, @DateType";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", req.PracticeCode));
                    cmd.Parameters.Add(new SqlParameter("DATEFROM", dDateFrom));
                    cmd.Parameters.Add(new SqlParameter("DATETO", dDateTo));
                    cmd.Parameters.Add(new SqlParameter("locationcode", string.Join(",", req.LocationCode)));
                    cmd.Parameters.Add(new SqlParameter("DateType", req.DateType));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Status = "success";
                    res.Response = list;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel ByHospitalDos(ReportRequestModel req, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                DateTime dDateTo;
                DateTime dDateFrom;
                DateTime.TryParse(req.DateFrom, out dDateFrom);
                DateTime.TryParse(req.DateTo, out dDateTo);
                //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                findPractice(req.PracticeCode);
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                    sp_name = hasMatch ? "By_Hospital_DOS" : "USP_NPM_By_Hospital";
                    cmd.CommandText = "exec " + sp_name + " @PracticeCode, @DATEFROM, @DATETO, @DateType";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", req.PracticeCode));
                    cmd.Parameters.Add(new SqlParameter("DATEFROM", dDateFrom));
                    cmd.Parameters.Add(new SqlParameter("DATETO", dDateTo));
                    cmd.Parameters.Add(new SqlParameter("DateType", req.DateType));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Status = "success";
                    res.Response = list;
                    db.Database.Connection.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel ByPrimaryDXDos(ReportRequestModel req, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                DateTime dDateTo;
                DateTime dDateFrom;
                DateTime.TryParse(req.DateFrom, out dDateFrom);
                DateTime.TryParse(req.DateTo, out dDateTo);
                //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                findPractice(req.PracticeCode);
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                    sp_name = hasMatch ? "By_Primary_Dx_DOS" : "USP_NPM_By_Primary_Dx_DOS";
                    cmd.CommandText = "exec " + sp_name + " @PracticeCode, @DATEFROM, @DATETO, @locationcode, @DateType";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", req.PracticeCode));
                    cmd.Parameters.Add(new SqlParameter("DATEFROM", dDateFrom));
                    cmd.Parameters.Add(new SqlParameter("DATETO", dDateTo));
                    cmd.Parameters.Add(new SqlParameter("locationcode", string.Join(",", req.LocationCode)));
                    cmd.Parameters.Add(new SqlParameter("DateType", req.DateType));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Status = "success";
                    res.Response = list;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel ByCarrierDos(ReportRequestModel req, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                DateTime dDateTo;
                DateTime dDateFrom;
                DateTime.TryParse(req.DateFrom, out dDateFrom);
                DateTime.TryParse(req.DateTo, out dDateTo);
                //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                findPractice(req.PracticeCode);
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                    sp_name = hasMatch ? "By_Carrier_DOS" : "USP_NPM_By_Carrier_DOS";
                    cmd.CommandText = "exec " + sp_name + " @PracticeCode, @DATEFROM, @DATETO, @locationcode, @DateType";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", req.PracticeCode));
                    cmd.Parameters.Add(new SqlParameter("DATEFROM", dDateFrom));
                    cmd.Parameters.Add(new SqlParameter("DATETO", dDateTo));
                    cmd.Parameters.Add(new SqlParameter("locationcode", string.Join(",", req.LocationCode)));
                    cmd.Parameters.Add(new SqlParameter("DateType", req.DateType));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Status = "success";
                    res.Response = list;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel PaymentMonthWise(ReportRequestModel req, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                DateTime dDateTo;
                DateTime dDateFrom;
                DateTime.TryParse(req.DateFrom, out dDateFrom);
                DateTime.TryParse(req.DateTo, out dDateTo);
                //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                findPractice(req.PracticeCode);
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    //Updated by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                    //cmd.CommandText = "exec Payment_Month_Wise @PracticeCode, @DATEFROM, @DATETO, @locationcode, @DateType";
                    sp_name = hasMatch ? "Payment_Month_Wise" : "USP_NPM_Payment_Month_Wise";
                    cmd.CommandText = "exec " + sp_name + " @PracticeCode, @DATEFROM, @DATETO, @locationcode, @DateType";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", req.PracticeCode));
                    cmd.Parameters.Add(new SqlParameter("DATEFROM", dDateFrom));
                    cmd.Parameters.Add(new SqlParameter("DATETO", dDateTo));
                    cmd.Parameters.Add(new SqlParameter("locationcode", string.Join(",", req.LocationCode)));
                    cmd.Parameters.Add(new SqlParameter("DateType", req.DateType));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Status = "success";
                    res.Response = list;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel PaymentDailyRefresh(ReportRequestModel req, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                DateTime dDateTo;
                DateTime dDateFrom;
                DateTime.TryParse(req.DateFrom, out dDateFrom);
                DateTime.TryParse(req.DateTo, out dDateTo);
                //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                findPractice(req.PracticeCode);
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                    //cmd.CommandText = "exec Payment_Daily_Refresh @PracticeCode, @DATEFROM, @DATETO, @locationcode, @DateType";
                    sp_name = hasMatch ? "Payment_Daily_Refresh" : "USP_NPM_Payment_Daily_Refresh";
                    cmd.CommandText = "exec " + sp_name + " @PracticeCode, @DATEFROM, @DATETO, @locationcode, @DateType";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", req.PracticeCode));
                    cmd.Parameters.Add(new SqlParameter("DATEFROM", dDateFrom));
                    cmd.Parameters.Add(new SqlParameter("DATETO", dDateTo));
                    cmd.Parameters.Add(new SqlParameter("locationcode", string.Join(",", req.LocationCode)));
                    cmd.Parameters.Add(new SqlParameter("DateType", req.DateType));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Status = "success";
                    res.Response = list;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel PaymentByCarrier(ReportRequestModel req, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                DateTime dDateTo;
                DateTime dDateFrom;
                DateTime.TryParse(req.DateFrom, out dDateFrom);
                DateTime.TryParse(req.DateTo, out dDateTo);
                //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                findPractice(req.PracticeCode);
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                    //cmd.CommandText = "exec Payment_By_Carrier @PracticeCode, @DATEFROM, @DATETO, @locationcode, @DateType";
                    sp_name = hasMatch ? "Payment_By_Carrier" : "USP_NPM_Payment_By_Carrier";
                    cmd.CommandText = "exec " + sp_name + " @PracticeCode, @DATEFROM, @DATETO, @locationcode, @DateType";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", req.PracticeCode));
                    cmd.Parameters.Add(new SqlParameter("DATEFROM", dDateFrom));
                    cmd.Parameters.Add(new SqlParameter("DATETO", dDateTo));
                    cmd.Parameters.Add(new SqlParameter("locationcode", string.Join(",", req.LocationCode)));
                    cmd.Parameters.Add(new SqlParameter("DateType", req.DateType));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Status = "success";
                    res.Response = list;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel PaymentPrimaryDX(ReportRequestModel req, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                DateTime dDateTo;
                DateTime dDateFrom;
                DateTime.TryParse(req.DateFrom, out dDateFrom);
                DateTime.TryParse(req.DateTo, out dDateTo);
                //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                findPractice(req.PracticeCode);
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    //Updated by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                    //cmd.CommandText = "exec By_Primary_DX_Payment @PracticeCode, @DATEFROM, @DATETO, @locationcode, @DateType";
                    sp_name = hasMatch ? "By_Primary_DX_Payment" : "USP_NPM_By_Primary_DX_Payment";
                    cmd.CommandText = "exec " + sp_name + " @PracticeCode, @DATEFROM, @DATETO, @locationcode, @DateType";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", req.PracticeCode));
                    cmd.Parameters.Add(new SqlParameter("DATEFROM", dDateFrom));
                    cmd.Parameters.Add(new SqlParameter("DATETO", dDateTo));
                    cmd.Parameters.Add(new SqlParameter("locationcode", string.Join(",", req.LocationCode)));
                    cmd.Parameters.Add(new SqlParameter("DateType", req.DateType));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Status = "success";
                    res.Response = list;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel PaymentByPrimaryICD10(ReportRequestModel req, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                DateTime dDateTo;
                DateTime dDateFrom;
                DateTime.TryParse(req.DateFrom, out dDateFrom);
                DateTime.TryParse(req.DateTo, out dDateTo);
                //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                findPractice(req.PracticeCode);
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                    //cmd.CommandText = "exec Payment_By_Primary_ICD10 @PracticeCode, @DATEFROM, @DATETO, @locationcode, @DateType";
                    sp_name = hasMatch ? "Payment_By_Primary_ICD10" : "USP_NPM_Payment_By_Primary_ICD10";
                    cmd.CommandText = "exec " + sp_name + " @PracticeCode, @DATEFROM, @DATETO, @locationcode, @DateType";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", req.PracticeCode));
                    cmd.Parameters.Add(new SqlParameter("DATEFROM", dDateFrom));
                    cmd.Parameters.Add(new SqlParameter("DATETO", dDateTo));
                    cmd.Parameters.Add(new SqlParameter("locationcode", string.Join(",", req.LocationCode)));
                    cmd.Parameters.Add(new SqlParameter("DateType", req.DateType));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Status = "success";
                    res.Response = list;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel RecallVisits(ReportRequestModel req, long userId)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.SP_GETRECALLVISITS_EGD(req.PracticeCode, req.DateFrom, req.DateTo).ToList();
                    res.Response = results;
                    res.Status = "success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel PeriodAnalysisAndClosing(ReportRequestModel req, long userId)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.SP_GETPERIODANALYSISANDCLOSING(req.PracticeCode, req.DateFrom, req.DateTo).ToList();
                    res.Response = results;
                    res.Status = "success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel PracticeAnalysis(ReportRequestModel req, long userId)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.SP_GETPRACTICEANALYSIS(req.PracticeCode, req.DateFrom, req.DateTo).ToList();
                    res.Response = results;
                    res.Status = "success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel PatientBirthDays(ReportRequestModel req, long userId)
        {
            ResponseModel res = new ResponseModel();
            PagingResponse pagingResponse = new PagingResponse();   
            try
            {
                if(req.PagedRequest.isExport==true)
                {
                    using (var ctx = new NPMDBEntities())
                    {
                        if (!string.IsNullOrEmpty(req.Month) && req.Month.Length == 7)
                        {
                            req.Month = req.Month + "-01";
                        }

                        var results = ctx.SP_GETPATIENTBDAYS(req.PracticeCode, req.Month).ToList();
                        var responseList = results.Select(c => new PatientBirthDaysResponse
                        {
                            PATIENT_ACCOUNT=c.PATIENTID,
                            PATIENT_NAME=c.PATIENTNAMEFULLLASTFIRST,
                            PATIENT_DOB=c.PATIENTDOBFULL,
                            PATIENT_AGE=c.PATIENTAGE,
                            HOME_PHONE=c.HOME_PHONE,
                            INSURANCE_NAME=c.POLICYINSURANCENAME,
                            RECENT_DOS=c.PATIENTMOSTRECENTDOS,
                            PROVIDER_NAME=c.RENDERRINGPROVIDERNAMEFULL,


                        }).ToList();
                        res.Response = responseList;
                        res.Status = "success";
                    }
                }
                else
                {
                    using (var ctx = new NPMDBEntities())
                    {
                        if (!string.IsNullOrEmpty(req.Month) && req.Month.Length == 7)
                        {
                            req.Month = req.Month + "-01";
                        }

                        var results = ctx.SP_GETPATIENTBDAYS(req.PracticeCode, req.Month)
                                .Skip((req.PagedRequest.page - 1) * req.PagedRequest.size).Take(req.PagedRequest.size).ToList();
                        pagingResponse.TotalRecords = ctx.SP_GETPATIENTBDAYS(req.PracticeCode, req.Month).Count();
                        pagingResponse.FilteredRecords = results.Count(); // Count after pagination
                        pagingResponse.CurrentPage = req.PagedRequest.page;
                        var responseList = results.Select(c => new PatientBirthDaysResponse
                        {
                            PATIENT_ACCOUNT = c.PATIENTID,
                            PATIENT_NAME = c.PATIENTNAMEFULLLASTFIRST,
                            PATIENT_DOB = c.PATIENTDOBFULL,
                            PATIENT_AGE = c.PATIENTAGE,
                            HOME_PHONE = c.HOME_PHONE,
                            INSURANCE_NAME = c.POLICYINSURANCENAME,
                            RECENT_DOS = c.PATIENTMOSTRECENTDOS,
                            PROVIDER_NAME = c.RENDERRINGPROVIDERNAMEFULL,


                        }).ToList();
                     
                        pagingResponse.data = responseList; ;
                        res.Response = pagingResponse;
                        res.Status = "success";
                    }
                }
              
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel AgingSummaryRecent(ReportRequestModel req, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                  var results = ctx.Aging_Summary_report_Top_5_Payers(req.PracticeCode).ToList();
                    res.Response = results;
                    res.Status = "success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }
       // bellow code is :Added BY Pir Ubaid-Dashboard Revamp 
        public ResponseModel GetCPAAnalysis(long? practiceCode, string fromDate, string toDate, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = "exec SP_GetCPAAnalysis_PirUbaid @PracticeCode, @FromDate, @ToDate";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", practiceCode));
                    cmd.Parameters.Add(new SqlParameter("FromDate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("ToDate", toDate));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Status = "success";
                    res.Response = list;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }
        public ResponseModel GetInsuranceAging(long? practiceCode, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = "exec sp_GetInsuranceAging_Ubaid @PracticeCode";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", practiceCode));
                    //cmd.Parameters.Add(new SqlParameter("FromDate", fromDate));
                    //cmd.Parameters.Add(new SqlParameter("ToDate", toDate));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Status = "success";
                    res.Response = list;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        //public ResponseModel GetClaimsAndERAs(long? practiceCode, DateTime? fromDate, DateTime? toDate, long v)
        //{
        //    ResponseModel res = new ResponseModel();
        //    try
        //    {
        //        using (var db = new NPMDBEntities())
        //        {
        //            db.Database.Connection.Open();
        //            var cmd = db.Database.Connection.CreateCommand();
        //            cmd.CommandText = "exec uspGetClaimsAndERAs_PirUbaid @PracticeCode, @FromDate, @ToDate";
        //            cmd.Parameters.Add(new SqlParameter("PracticeCode", practiceCode));
        //            cmd.Parameters.Add(new SqlParameter("FromDate", fromDate));
        //            cmd.Parameters.Add(new SqlParameter("ToDate", toDate));
        //            var reader = cmd.ExecuteReader();
        //            var list = new List<dynamic>();
        //            if (reader.HasRows)
        //            {
        //                while (reader.Read())
        //                {
        //                    dynamic obj = new ExpandoObject();
        //                    for (int i = 0; i < reader.FieldCount; i++)
        //                    {
        //                        AddProperty(obj, reader.GetName(i), reader[i]);
        //                    }
        //                    list.Add(obj);
        //                }
        //            }
        //            res.Status = "success";
        //            res.Response = list;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return res;
        //}

        public ResponseModel GetClaimsAndERAs(long? practiceCode, string fromDate, string toDate, long v)
        {
            ResponseModel res = new ResponseModel();
            res.Response = new ResponseDataModel();

            try
            {
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = "exec uspGetClaimsAndERAs_PirUbaid @PracticeCode, @FromDate, @ToDate";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", practiceCode));
                    cmd.Parameters.Add(new SqlParameter("FromDate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("ToDate", toDate));
                    var reader = cmd.ExecuteReader();

                    var claimsAndERAs = new ClaimsAndERAs();

                    if (reader.Read())
                    {
                        claimsAndERAs.claims_submitted = (int)reader["claims_submitted"];
                    }

                    if (reader.NextResult())
                    {
                        if (reader.Read())
                        {
                            claimsAndERAs.pending_claims = (int)reader["pending_claims"];
                        }
                    }
                    if (reader.NextResult())
                    {
                        if (reader.Read())
                        {
                            claimsAndERAs.total_posted_eras = (int)reader["total_posted_eras"];
                        }
                    }

                    if (reader.NextResult())
                    {
                        if (reader.Read())
                        {
                            claimsAndERAs.total_unposted_eras = (int)reader["total_unposted_eras"];
                        }
                    }

                    if (reader.NextResult())
                    {
                        
                        if (reader.Read())
                        {
                            claimsAndERAs.total_patient_accounts = (int)reader["total_patient_accounts"];
                        }
                    }

                    if (reader.NextResult())
                    {
                        if (reader.Read())
                        {
                            claimsAndERAs.total_claims = (int)reader["total_claims"];
                        }
                    }

                    if (reader.NextResult())
                    {
                            if (reader.Read())
                        {
                            claimsAndERAs.total_statements_sent = (int)reader["total_statements_sent"];
                        }
                    }
                    res.Status = "success";
                    res.Response.claimsAndERAs = claimsAndERAs;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }



        public ResponseModel GetPatientAging(long? practiceCode, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = "exec sp_GetPatientAging_Ubaid @PracticeCode";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", practiceCode));
                    //cmd.Parameters.Add(new SqlParameter("FromDate", fromDate));
                    //cmd.Parameters.Add(new SqlParameter("ToDate", toDate));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Status = "success";
                    res.Response = list;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel GetPCRatio(long? practiceCode, string fromDate, string toDate, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = "exec SP_PCRatio @PracticeCode, @FromDate, @ToDate";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", practiceCode));
                    cmd.Parameters.Add(new SqlParameter("FromDate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("ToDate", toDate));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Status = "success";
                    res.Response = list;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel GetChargesANDPaymentsTrend(long? practiceCode, string fromDate, string toDate, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                DateTime? _todate = Convert.ToDateTime(toDate);
                DateTime? _fromdate = Convert.ToDateTime(fromDate);
                // Ensure toDate has a value
                if (!_todate.HasValue)
                {
                    throw new ArgumentException("toDate must have a value.");
                }

                // Check if fromDate and toDate are less than 32 days apart
                if (_fromdate.HasValue && (_todate.Value - _fromdate.Value).TotalDays < 32)
                {
                    _fromdate = _todate.Value.AddDays(-90);

                    //..this is converted to string
                    fromDate = _fromdate.ToString();
                }

                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = "exec SP_ChargesANDPaymentsTrend @PracticeCode, @FromDate, @ToDate";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", practiceCode));
                    cmd.Parameters.Add(new SqlParameter("FromDate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("ToDate", toDate));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Status = "success";
                    res.Response = list;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel ChargesPaymentsRecent(ReportRequestModel req, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = "exec SP_GETChargesPaymentsLastMonthsTets @PRAC";
                    cmd.Parameters.Add(new SqlParameter("PRAC", req.PracticeCode));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Status = "success";
                    res.Response = list;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel GetAgingDashboard(ReportRequestModel req, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.SP_GETAGINGDASHBOARD_Tets(req.PracticeCode).ToList();
                    res.Response = results;
                    res.Status = "success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel GetInsuranceDetailReport(long? PracCode, long v)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.sp_insuranceardetailreport(PracCode).ToList();


                    if (results.Count > 0)

                    {
                       var responseList=results.Select(c => new InsuranceDetailReportResponse
                       {
                         PRACTICE_CODE=c.PRACTICE_CODE,
                         Practice_name=c.PRAC_NAME,
                         PATIENT_NAME=c.PATIENT_NAME,   
                         PATIENT_ACCOUNT=c.PATIENT_ACCOUNT,
                         DATE_OF_BIRTH=c.DATE_OF_BIRTH,
                         CLAIM_ENTRY_DATE=c.CLAIM_ENTRYDATE,
                         CLAIM_NO=c.CLAIM_NO,
                         DOS=c.DOS,
                         CLAIM_TOTAL=c.CLAIM_TOTAL,
                         Amount_Paid=c.AMT_PAID,
                         Amount_Adjusted=c.ADJUSTMENT,
                         Amount_Due=c.AMT_DUE,
                         Primary_Status=c.PRI_STATUS,
                         PRIMARY_PAYER=c.PRIMARYPAYER,
                         PRIMARY_POLICY_NUMBER=c.PRI_POLICY_NUMBER,
                         Secondary_Status=c.SEC_STATUS,
                         SECONDARY_PAYER=c.SECPAYER,
                         SECONDARY_POLICY_NUMBER=c.SEC_POLICY_NUMBER,
                         Other_Status=c.OTH_STATUS,
                         AGING_DAYS=c.AGINGDAYS,
                         Patient_Status=c.Pat_status,
                       } ).ToList();    
                        res.Response = responseList;
                        res.Status = "success";
                    }
                    else
                    {
                        res.Status = "Failed";
                        res.Response = "No Records";

                    }

                }
            }
            catch (Exception)
            {
                throw;
            }

            return res;

        }
        //added by Samiullah
        public ResponseModel GetInsuranceDetailReportPagination(long? PracCode, long v, int page, int size)
        {
            ResponseModel res = new ResponseModel();
            PagingResponse pagingResponse = new PagingResponse();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.sp_insuranceardetailreport(PracCode)

                            .Skip((page - 1) * size).Take(size).ToList();
                    pagingResponse.TotalRecords = ctx.sp_insuranceardetailreport(PracCode).Count();
                    pagingResponse.FilteredRecords = results.Count(); // Count after pagination
                    pagingResponse.CurrentPage = page;
                 

                    if (results.Count > 0)
                    {
                        var responseList = results.Select(c => new InsuranceDetailReportResponse
                        {
                            PRACTICE_CODE = c.PRACTICE_CODE,
                            Practice_name = c.PRAC_NAME,
                            PATIENT_NAME = c.PATIENT_NAME,
                            PATIENT_ACCOUNT = c.PATIENT_ACCOUNT,
                            DATE_OF_BIRTH = c.DATE_OF_BIRTH,
                            CLAIM_ENTRY_DATE = c.CLAIM_ENTRYDATE,
                            CLAIM_NO = c.CLAIM_NO,
                            DOS = c.DOS,
                            CLAIM_TOTAL = c.CLAIM_TOTAL,
                            Amount_Paid = c.AMT_PAID,
                            Amount_Adjusted = c.ADJUSTMENT,
                            Amount_Due = c.AMT_DUE,
                            Primary_Status = c.PRI_STATUS,
                            PRIMARY_PAYER = c.PRIMARYPAYER,
                            PRIMARY_POLICY_NUMBER = c.PRI_POLICY_NUMBER,
                            Secondary_Status = c.SEC_STATUS,
                            SECONDARY_PAYER = c.SECPAYER,
                            SECONDARY_POLICY_NUMBER = c.SEC_POLICY_NUMBER,
                            Other_Status = c.OTH_STATUS,
                            AGING_DAYS = c.AGINGDAYS,
                            Patient_Status = c.Pat_status,
                        }).ToList();
                        pagingResponse.data = responseList;
                        res.Response = pagingResponse;
                        res.Status = "success";
                    }
                    else
                    {
                        res.Status = "Failed";
                        res.Response = "No Records";

                    }

                }
            }
            catch (Exception)
            {
                throw;
            }

            return res;

        }
        public ResponseModel GetUserReport(string PracCode, string v, string dateFrom, string dateTo)
        {
            if (v == "0")
            {
                v = null;
            }
            if (dateFrom == "null")
            {
                dateFrom = null;
            }
            if (dateTo == "null")
            {
                dateTo = null;
            }
            ResponseModel res = new ResponseModel();
            try
            {
                List<SP_getuserDailyReport_Result> list = null;
                using (var ctx = new NPMDBEntities())
                {
                     list = ctx.SP_getuserDailyReport(PracCode, v, dateFrom, dateTo).ToList();

                    if (list.Count > 0)
                    {
                        var response=list.Select(c => new userDailyReportResponse
                        {
                            Practice_Code=c.Practice_Code,
                            Practice_Name=c.Prac_Name,
                            Patient_Account=c.Patient_Account,
                            Patient_Name=c.Patient_Name,
                            CLAIM_NO=c.CLAIM_NO,
                            Location_Name=c.Location_Name,
                            Facility=c.Facility_name,
                            Attending_Physician=c.Attending_Physician,
                            RENDERING_PROVIDER=c.RENDERRING_PROVIDER,
                            RESOURCE_PROVIDER=c.RESOURCE_PROVIDER,  
                            PROCEDURE_CODE=c.PROCEDURE_CODE,
                            Procedure_Description=c.ProcedureDescription,
                            Primary_Carrier=c.Primary_Carrier,
                            Primary_Policy_Number=c.Primary_Policy_Number_,
                            Secondary_Carrier=c.Secondary_Carrier,
                            Secondary_Policy_Number=c.Secondary_Policy_Number_,
                            created_by=c.created_byN,
                            DOS=c.DOS,
                            Entry_Date=c.Entry_Date,
                            BILLED_CHARGE=c.BILLED_CHARGE,
                            UNITS=c.UNITS,
                            claim_charges_id=c.claim_charges_id,
                            Modifier_1=c.Modi_Code1,
                            Modifier_2=c.Modi_Code2,
                            Modifier_3=c.modi_code3,
                            Modifier_4=c.modi_code4,
                            Diagnosis_1=c.DX_Code1,
                            Diagnosis_2=c.DX_Code2,
                            Diagnosis_3=c.DX_Code3,
                            Diagnosis_4=c.DX_Code4,
                            Diagnosis_5=c.DX_Code5,
                            Diagnosis_6=c.DX_Code6,
                            Diagnosis_7=c.DX_Code7,
                            Diagnosis_8=c.DX_Code8,
                            Diagnosis_9=c.DX_Code9,
                            Diagnosis_10=c.DX_Code10,
                            Diagnosis_11=c.DX_Code11,
                            Diagnosis_12=c.DX_Code12

                        }).ToList();    
                        res.Response = response;
                        res.Status = "success";
                    }
                    else
                    {
                        res.Status = "Failed";
                        res.Response = "No Records";

                    }

                }
            }
            catch (Exception)
            {
                throw;
            }

            return res;

        }
        public ResponseModel GetUserReportPagination(string PracCode, string v, string dateFrom, string dateTo,int page,int size)
        {
            if (v == "0")
            {
                v = null;
            }
            if (dateFrom == "null")
            {
                dateFrom = null;
            }
            if (dateTo == "null")
            {
                dateTo = null;
            }
            ResponseModel res = new ResponseModel();
            PagingResponse pagingResponse = new PagingResponse();
            try
            {
                List<SP_getuserDailyReport_Result> list = null;
                using (var ctx = new NPMDBEntities())
                {
                    list = ctx.SP_getuserDailyReport(PracCode, v, dateFrom, dateTo)
                           .OrderByDescending(s => s.CLAIM_NO)
                           .Skip((page - 1) * size).Take(size).ToList();
                    pagingResponse.TotalRecords = ctx.SP_getuserDailyReport(PracCode, v, dateFrom, dateTo).Count();
                    pagingResponse.FilteredRecords = list.Count(); // Count after pagination
                    pagingResponse.CurrentPage = page;
                   
                  

                    if (list.Count > 0)
                    {
                        var response = list.Select(c => new userDailyReportResponse
                        {
                            Practice_Code = c.Practice_Code,
                            Practice_Name = c.Prac_Name,
                            Patient_Account = c.Patient_Account,
                            Patient_Name = c.Patient_Name,
                            CLAIM_NO = c.CLAIM_NO,
                            Location_Name = c.Location_Name,
                                     Facility=c.Facility_name,
                            Attending_Physician = c.Attending_Physician,
                            RENDERING_PROVIDER = c.RENDERRING_PROVIDER,
                            RESOURCE_PROVIDER = c.RESOURCE_PROVIDER,
                            PROCEDURE_CODE = c.PROCEDURE_CODE,
                            Procedure_Description = c.ProcedureDescription,
                            Primary_Carrier = c.Primary_Carrier,
                            Primary_Policy_Number = c.Primary_Policy_Number_,
                            Secondary_Carrier = c.Secondary_Carrier,
                            Secondary_Policy_Number = c.Secondary_Policy_Number_,
                            created_by = c.created_byN,
                            DOS = c.DOS,
                            Entry_Date = c.Entry_Date,
                            BILLED_CHARGE = c.BILLED_CHARGE,
                            UNITS = c.UNITS,
                            claim_charges_id = c.claim_charges_id,
                            Modifier_1 = c.Modi_Code1,
                            Modifier_2 = c.Modi_Code2,
                            Modifier_3 = c.modi_code3,
                            Modifier_4 = c.modi_code4,
                            Diagnosis_1 = c.DX_Code1,
                            Diagnosis_2 = c.DX_Code2,
                            Diagnosis_3 = c.DX_Code3,
                            Diagnosis_4 = c.DX_Code4,
                            Diagnosis_5 = c.DX_Code5,
                            Diagnosis_6 = c.DX_Code6,
                            Diagnosis_7 = c.DX_Code7,
                            Diagnosis_8 = c.DX_Code8,
                            Diagnosis_9 = c.DX_Code9,
                            Diagnosis_10 = c.DX_Code10,
                            Diagnosis_11 = c.DX_Code11,
                            Diagnosis_12 = c.DX_Code12

                        }).ToList();
                        pagingResponse.data = response;
                        res.Response = pagingResponse;
                        res.Status = "success";
                    }
                    else
                    {
                        res.Status = "Failed";
                        res.Response = "No Records";

                    }

                }
            }
            catch (Exception)
            {
                throw;
            }

            return res;

        }
        //public ResponseModel GetUserReport(string PracCode, string v)
        //{
        //    if (v == "0")
        //            {
        //              v = null;
        //          }
        //        ResponseModel res = new ResponseModel();
        //    try
        //    {
        //        using (var db = new NPMDBEntities())
        //        {
        //            db.Database.Connection.Open();
        //            var cmd = db.Database.Connection.CreateCommand();
        //            cmd.CommandText = "exec [SP_userReportCharges] @practice_code, @userid";
        //            cmd.Parameters.Add(new SqlParameter("practice_code", PracCode));
        //            cmd.Parameters.Add(new SqlParameter("userid", v));
        //            var reader = cmd.ExecuteReader();
        //            var list = new List<dynamic>();
        //            if (reader.HasRows)
        //            {
        //                while (reader.Read())
        //                {
        //                    dynamic obj = new ExpandoObject();
        //                    for (int i = 0; i < reader.FieldCount; i++)
        //                    {
        //                        AddProperty(obj, reader.GetName(i), reader[i]);
        //                    }
        //                    list.Add(obj);
        //                }
        //            }
        //            res.Status = "success";
        //            res.Response = list;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return res;
        //}
        public ResponseModel holdReport(long? PracCode)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.DelayClaimReports(PracCode).ToList();

                    if (results.Count > 0)
                    {
                       res.Response = results;
                      res.Status = "success";
                    }
                    else
                   {
                      res.Status = "Failed";
                        res.Response = "No Records";

                    }

                }
            }
            catch (Exception)
            {
                throw;
            }

            return res;

        }

        public ResponseModel GetRollingSummaryReport(string PracCode, string duration)
        {
            long pr = Convert.ToInt64(PracCode);
            int du=Convert.ToInt32('-'+duration);
            ResponseModel res = new ResponseModel();
            try
            {
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = "exec SP_rollingReport @PracticeCode,@duration";
                    cmd.Parameters.Add(new SqlParameter("PracticeCode", pr));
                    cmd.Parameters.Add(new SqlParameter("duration", du));
            
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Response = list;
                    res.Status = "success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;

        }


        public async Task<ResponseModel> CollectionAnalysisReport(long PracticeCode, DateTime? DateFrom, DateTime? DateTo, bool isExport = false, int page = 1, int size = 10)
        {
            ResponseModel objResponse = new ResponseModel();
            var pagingResponse = new PagingResponse();
            try
            {
                List<USP_COLLECTION_ANALYSIS_REPORT_NPM_Result> collectionAnalysisList = null;
                if (isExport == true)
                {
                    using (var ctx = new NPMDBEntities())
                    {
                        collectionAnalysisList = ctx.USP_COLLECTION_ANALYSIS_REPORT_NPM(PracticeCode,DateFrom,DateTo).ToList();
                    }

                    if (collectionAnalysisList != null)
                    {
                        objResponse.Status = "success";
                        objResponse.Response = collectionAnalysisList;
                    }
                    else
                    {
                        objResponse.Status = "Error";
                    }
                }
                else
                {
                    using (var ctx = new NPMDBEntities())
                    {
                        collectionAnalysisList = ctx.USP_COLLECTION_ANALYSIS_REPORT_NPM(PracticeCode,DateFrom,DateTo)
                            .OrderByDescending(s => s.CLAIM_NO)
                            .Skip((page - 1) * size).Take(size).ToList();
                        pagingResponse.TotalRecords = ctx.USP_COLLECTION_ANALYSIS_REPORT_NPM(PracticeCode,DateFrom,DateTo).Count();
                        pagingResponse.FilteredRecords = collectionAnalysisList.Count(); // Count after pagination
                        pagingResponse.CurrentPage = page;
                        pagingResponse.data = collectionAnalysisList;
                    }
                    objResponse.Status = "success";
                    objResponse.Response = pagingResponse;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return objResponse;

        }

        public async Task<ResponseModel> PatientAgingReport(long PracticeCode, bool isExport = false, int page=1, int size=10)
        {
            ResponseModel objResponse = new ResponseModel();
            var pagingResponse = new PagingResponse();
            try
            {
                List<USP_PATIENT_AGING_REPORT_NPM_Result> patientAgingList = null;
                if (isExport == true)
                {
                    using (var ctx = new NPMDBEntities())
                    {
                        patientAgingList =  ctx.USP_PATIENT_AGING_REPORT_NPM(PracticeCode).ToList();
                    }

                    if (patientAgingList != null)
                    {
                        objResponse.Status = "success";
                        objResponse.Response = patientAgingList;
                    }
                    else
                    {
                        objResponse.Status = "Error";
                    }
                }
                else
                {
                    using (var ctx = new NPMDBEntities())
                    {

                        patientAgingList = ctx.USP_PATIENT_AGING_REPORT_NPM(PracticeCode)
                            .OrderByDescending(s => s.CLAIM_NO)
                            .Skip((page - 1) * size).Take(size).ToList();
                        pagingResponse.TotalRecords = ctx.USP_PATIENT_AGING_REPORT_NPM(PracticeCode).Count();
                        pagingResponse.FilteredRecords = patientAgingList.Count(); // Count after pagination
                        pagingResponse.CurrentPage = page;
                        pagingResponse.data = patientAgingList;

                    }

                    objResponse.Status = "success";
                    objResponse.Response = pagingResponse;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return objResponse;
        }

        public async Task<ResponseModel> PatientAgingAnalysisReport(long PracticeCode, bool isExport = false, int page = 1, int size = 10)
        {
            ResponseModel objResponse = new ResponseModel();
            var pagingResponse = new PagingResponse();
            try
            {
                List<USP_Patient_Wise_Aging_Summary_Report_NPM_Result> patientAgingList = null;
                if (isExport == true)
                {
                    using (var ctx = new NPMDBEntities())
                    {
                        patientAgingList = ctx.USP_Patient_Wise_Aging_Summary_Report_NPM(PracticeCode).ToList();
                    }

                    if (patientAgingList != null)
                    {
                        objResponse.Status = "success";
                        objResponse.Response = patientAgingList;
                    }
                    else
                    {
                        objResponse.Status = "Error";
                    }
                }
                else
                {
                    using (var ctx = new NPMDBEntities())
                    {

                      patientAgingList =  ctx.USP_Patient_Wise_Aging_Summary_Report_NPM(PracticeCode)
                           .OrderBy(s => s.PATIENT_ACCOUNT)
                           .Skip((page - 1) * size).Take(size).ToList();
                        pagingResponse.TotalRecords = ctx.USP_Patient_Wise_Aging_Summary_Report_NPM(PracticeCode).Count();
                        pagingResponse.FilteredRecords = patientAgingList.Count(); // Count after pagination
                        pagingResponse.CurrentPage = page;
                        pagingResponse.data = patientAgingList;

                    }

                    objResponse.Status = "success";
                    objResponse.Response = pagingResponse;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return objResponse;
        }

        //CLAIMS AND ACCOUNT ASSIGNMENT REPORTS

        public ResponseModel ClaimAssignmentReport(long PracticeCode, string DateFrom, string DateTo)
        {
            ResponseModel objResponse = new ResponseModel();
            List<ClaimAssignee_CL> assignedclaims = null;

            {
                using (var ctx = new NPMDBEntities())
                    try
                    {
                        DateTime dDateTo;
                        DateTime dDateFrom;
                        DateTime.TryParse(DateFrom, out dDateFrom);
                        DateTime.TryParse(DateTo, out dDateTo);
                        dDateTo = dDateTo.AddHours(23).AddMinutes(59);

                        assignedclaims = ctx.ClaimAssignee_CL.Where(c => c.PracticeCode == PracticeCode && c.Created_Date >= dDateFrom && c.Created_Date <= dDateTo).ToList();

                        if (assignedclaims != null)
                        {
                            objResponse.Status = "Sucess";
                            objResponse.Response = assignedclaims;


                        }
                    }

                    catch (Exception)
                    {
                        throw;
                    }
            }
            return objResponse;
        }



        //public ResponseModel AccounAssignmentReport(long PracticeCode, string DateFrom, string DateTo)
        //{
        //    ResponseModel objResponse = new ResponseModel();
        //    List<AccountAssignee_AL> assignedaccounts = null;

        //    {
        //        using (var ctx = new NPMDBEntities())
        //            try
        //            {
        //                DateTime dDateTo;
        //                DateTime dDateFrom;
        //                DateTime.TryParse(DateFrom, out dDateFrom);
        //                DateTime.TryParse(DateTo, out dDateTo);
        //                dDateTo = dDateTo.AddHours(23).AddMinutes(59);


        //public ResponseModel AccounAssignmentReport(long PracticeCode, string DateFrom, string DateTo)
        //{
        //    ResponseModel objResponse = new ResponseModel();
        //    List<AccountAssignee_AL> assignedaccounts = null;

        //    {
        //        using (var ctx = new NPMDBEntities())
        //            try
        //            {
        //                DateTime dDateTo;
        //                DateTime dDateFrom;
        //                DateTime.TryParse(DateFrom, out dDateFrom);
        //                DateTime.TryParse(DateTo, out dDateTo);
        //                dDateTo = dDateTo.AddHours(23).AddMinutes(59);

        //                assignedaccounts = ctx.AccountAssignee_AL.Where(c => c.PracticeCode == PracticeCode && c.Created_Date >= dDateFrom && c.Created_Date <= dDateTo).ToList();

        //                if (assignedaccounts != null)
        //                {
        //                    objResponse.Status = "Sucess";
        //                    objResponse.Response = assignedaccounts;


        //                }
        //            }

        //            catch (Exception)
        //            {
        //                throw;
        //            }
        //    }
        //    return objResponse;
        //}
        public ResponseModel ERAUnpostedAdjReport(long PracticeCode, string DateFrom, string DateTo)
        {
            ResponseModel objResponse = new ResponseModel();
            List<ERA_UNPOSTED_PAYMENTS_EDI_Result> eRA_UNPOSTED_PAYMENTS_EDI_Results = new List<ERA_UNPOSTED_PAYMENTS_EDI_Result>();

            using (var ctx = new NPMDBEntities())
            {
                eRA_UNPOSTED_PAYMENTS_EDI_Results = ctx.ERA_UNPOSTED_PAYMENTS_EDI(PracticeCode, Convert.ToDateTime(DateFrom), Convert.ToDateTime(DateTo)).ToList();
            }
            objResponse.Response = eRA_UNPOSTED_PAYMENTS_EDI_Results;
            objResponse.Status = "Success";

            return objResponse;
        }
        public ResponseModel AccounAssignmentReport(long PracticeCode, string DateFrom, string DateTo)
        {
            ResponseModel objResponse = new ResponseModel();
            List<AccountAssignee_AL> assignedaccounts = null;

            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    DateTime dDateFrom;
                    DateTime dDateTo;

                    if (DateTime.TryParse(DateFrom, out dDateFrom) && DateTime.TryParse(DateTo, out dDateTo))
                    {
                        // Adjust DateTo to include the whole day
                        dDateTo = dDateTo.AddHours(23).AddMinutes(59);

                        assignedaccounts = ctx.AccountAssignee_AL
                            .Where(c => c.PracticeCode == PracticeCode &&
                                        c.Created_Date >= dDateFrom &&
                                        c.Created_Date <= dDateTo)
                            .ToList();

                        if (assignedaccounts != null && assignedaccounts.Count > 0)
                        {
                            var responseList = assignedaccounts.Select(c => new AccountAssigneeResponse
                            {
                                Account_AssigneeID = c.Account_AssigneeID,
                                Task_Status = c.Status,
                                Priority = c.Priority,
                                Start_Date = c.Start_Date,
                                Due_Date = c.Due_Date,
                                AssignedToUserId = c.Assignedto_UserId,
                                AssignedToUserName = c.Assignedto_UserName,
                                Assigned_To = c.Assignedto_FullName,
                                AssignedByUserId = c.AssignedBy_UserId,
                                AssignedByUserName = c.AssignedBy_UserName,
                                Assigned_By = c.AssignedBy_FullName,
                                PracticeCode = c.PracticeCode,
                                Account_No = c.PatientAccount,
                                Patient_Name = c.PatientFullName,
                                Deleted = c.Deleted,
                                CreatedBy = c.Created_By,
                                Task_Created_Date = c.Created_Date,
                                ModifiedBy = c.Modified_By,
                                ModifiedDate = c.Modified_Date,
                                ModificationAllowed = c.modification_allowed
                            }).ToList();

                            objResponse.Status = "Success";
                            objResponse.Response = responseList;
                        }
                        else
                        {
                            objResponse.Status = "No Data";
                            objResponse.Response = "No accounts found for the given criteria.";
                        }
                    }
                    else
                    {
                        objResponse.Status = "Error";
                        objResponse.Response = "Invalid date format.";
                    }
                }
                catch (Exception ex)
                {
                    objResponse.Status = "Error";
                    objResponse.Response = ex.Message;
                }
            }

            return objResponse;
        }
        public ResponseModel AccounAssignmentReportPagination(long PracticeCode, string DateFrom, string DateTo ,int page,int size)
        {
            ResponseModel objResponse = new ResponseModel();
            PagingResponse pagingResponse = new PagingResponse();
            List<AccountAssignee_AL> assignedaccounts = null;

            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    DateTime dDateFrom;
                    DateTime dDateTo;

                    if (DateTime.TryParse(DateFrom, out dDateFrom) && DateTime.TryParse(DateTo, out dDateTo))
                    {
                        // Adjust DateTo to include the whole day
                        dDateTo = dDateTo.AddHours(23).AddMinutes(59);

                       
                        assignedaccounts = ctx.AccountAssignee_AL
                                             .Where(c => c.PracticeCode == PracticeCode &&
                                             c.Created_Date >= dDateFrom &&
                                             c.Created_Date <= dDateTo)
                                            .OrderByDescending(c => c.Created_Date)
                                            .Skip((page - 1) * size).Take(size).ToList();
                        pagingResponse.TotalRecords = ctx.AccountAssignee_AL
                                                        .Where(c => c.PracticeCode == PracticeCode &&
                                                        c.Created_Date >= dDateFrom &&
                                                        c.Created_Date <= dDateTo).Count();
                        pagingResponse.FilteredRecords = assignedaccounts.Count(); // Count after pagination
                        pagingResponse.CurrentPage = page;
                      
                        if (assignedaccounts != null && assignedaccounts.Count > 0)
                        {
                            var responseList = assignedaccounts.Select(c => new AccountAssigneeResponse
                            {
                                Account_AssigneeID = c.Account_AssigneeID,
                                Task_Status = c.Status,
                                Priority = c.Priority,
                                Start_Date = c.Start_Date,
                                Due_Date = c.Due_Date,
                                AssignedToUserId = c.Assignedto_UserId,
                                AssignedToUserName = c.Assignedto_UserName,
                                Assigned_To = c.Assignedto_FullName,
                                AssignedByUserId = c.AssignedBy_UserId,
                                AssignedByUserName = c.AssignedBy_UserName,
                                Assigned_By = c.AssignedBy_FullName,
                                PracticeCode = c.PracticeCode,
                                Account_No = c.PatientAccount,
                                Patient_Name = c.PatientFullName,
                                Deleted = c.Deleted,
                                CreatedBy = c.Created_By,
                                Task_Created_Date = c.Created_Date,
                                ModifiedBy = c.Modified_By,
                                ModifiedDate = c.Modified_Date,
                                ModificationAllowed = c.modification_allowed
                            }).ToList();
                            pagingResponse.data = responseList;
                            objResponse.Status = "Success";
                            objResponse.Response = pagingResponse;
                        }
                        else
                        {
                            objResponse.Status = "No Data";
                            objResponse.Response = "No accounts found for the given criteria.";
                        }
                    }
                    else
                    {
                        objResponse.Status = "Error";
                        objResponse.Response = "Invalid date format.";
                    }
                }
                catch (Exception ex)
                {
                    objResponse.Status = "Error";
                    objResponse.Response = ex.Message;
                }
            }

            return objResponse;
        }

        public ResponseModel CPA(ReportRequestModel req, long v)
        {
            //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
            findPractice(req.PracticeCode);
            ResponseModel res = new ResponseModel();
            try
            {
                using (var db = new NPMDBEntities())
                {
                    db.Database.Connection.Open();
                    var cmd = db.Database.Connection.CreateCommand();
                    //Updated by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                    if (hasMatch)
                    {
                        cmd.CommandText = "exec SP_COSSCPABYADDCRT @datecriteria,@datacriteria,@Fromdate,@Todate,@facname";
                        cmd.Parameters.Add(new SqlParameter("facname", string.Join(",", req.LocationCode)));
                    }
                    else
                    {
                        cmd.CommandText = "exec USP_NPM_COSSCPABYADDCRT @datecriteria,@datacriteria,@PracticeCode,@Fromdate,@Todate,@locationcode";
                        cmd.Parameters.Add(new SqlParameter("locationcode", string.Join(",", req.LocationCode)));
                        cmd.Parameters.Add(new SqlParameter("PracticeCode", req.PracticeCode));
                    }
                    cmd.Parameters.Add(new SqlParameter("datecriteria", req.DateType));
                    cmd.Parameters.Add(new SqlParameter("datacriteria", req.DataType));
                    cmd.Parameters.Add(new SqlParameter("Fromdate", req.DateFrom));
                    cmd.Parameters.Add(new SqlParameter("Todate", req.DateTo));
                    //cmd.Parameters.Add(new SqlParameter("facname", string.Join(",", req.LocationCode)));
                    var reader = cmd.ExecuteReader();
                    var list = new List<dynamic>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            dynamic obj = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                AddProperty(obj, reader.GetName(i), reader[i]);
                            }
                            list.Add(obj);
                        }
                    }
                    res.Response = list;
                    res.Status = "success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }


        //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
        public ResponseModel CheckPractice(long PracticeCode)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                var result = this.findPractice(PracticeCode);
                res.Response = result;
                res.Status = "success";
            }
            catch (Exception e)
            {
                throw e;
            }
            return res;
        }
        //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
        public bool findPractice(long PracticeCode)
        {
            using (var db = new NPMDBEntities())
            {
               // hasMatch = db.Practice_Reporting.Any(o => o.Practice_Code == PracticeCode && o.Deleted != true);
               //..Above change is commented to resolve the COSS Practice issue occured due to Practice_Reporting table change
                hasMatch = db.ExternalPractices_Reporting.Any(o => o.Practice_Code == PracticeCode && o.Deleted != true);
            }
            return hasMatch;
        }
        //added by SamiUllah
        public async Task<ResponseModel> VisitClaimActivityReport(PatelReportsRequestModel request)
        {
            ResponseModel res = new ResponseModel();
            PagingResponse pagingResponse = new PagingResponse();
            try
            {
                if (request.PagedRequest.isExport == true)
                {
                    using (var ctx = new NPMDBEntities())
                    {


                        var results = ctx.USP_Visit_Claim_Activity_Report(request.PracticeCode, request.ProviderCode,request.DateFrom,request.DateTo).ToList();
                        var responseList = results.Select(c => new VisitClaimActivityReportResponse
                        {
                            POSTING=c.POSTING,
                            PATIENT_NAME=c.PATIENT_NAME,
                            DOS=c.DOS,
                            RESP=c.RESP,
                            CPT_CODE=c.CODE,
                            Description=c.description,
                            Billed=c.Billed,
                            PROVIDER=c.PROVIDER,
                            LOCATION=c.LOCATION,
                            Amount=c.Amount,
                        }).ToList();
                        res.Response = responseList;
                        res.Status = "success";
                    }
                }
                else
                {
                    using (var ctx = new NPMDBEntities())
                    {
                   
                        var results = ctx.USP_Visit_Claim_Activity_Report(request.PracticeCode, request.ProviderCode, request.DateFrom, request.DateTo)
                                .Skip((request.PagedRequest.page - 1) * request.PagedRequest.size).Take(request.PagedRequest.size).ToList();
                        pagingResponse.TotalRecords = ctx.USP_Visit_Claim_Activity_Report(request.PracticeCode, request.ProviderCode, request.DateFrom, request.DateTo).Count();
                        pagingResponse.FilteredRecords = results.Count(); // Count after pagination
                        pagingResponse.CurrentPage = request.PagedRequest.page;
                        var responseList = results.Select(c => new VisitClaimActivityReportResponse
                        {
                            POSTING = c.POSTING,
                            PATIENT_NAME = c.PATIENT_NAME,
                            DOS = c.DOS,
                            RESP = c.RESP,
                            CPT_CODE = c.CODE,
                            Description = c.description,
                            Billed = c.Billed,
                            PROVIDER = c.PROVIDER,
                            LOCATION = c.LOCATION,
                            Amount = c.Amount,
                        }).ToList();

                        pagingResponse.data = responseList; ;
                        res.Response = pagingResponse;
                        res.Status = "success";
                    }
                }
            }
            catch(Exception)
            {
                throw;
            }
            return res;
        }

        public async Task<ResponseModel> GetProvidersByPractice(int PracticeId)
        {
           ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var result = ctx.Providers
                        .Where(p => p.Practice_Code == PracticeId
                        && (p.Deleted == false || p.Deleted == null) // Handle nullable bool
                        && p.Is_Active == true) // Ensure Is_Active is true
                    .ToList();
                    if(result.Count > 0)
                    {
                        var res = result.Select(p => new ProvidersByPracticeResponse
                        {
                            ProviderId = p.Provider_Code,
                            ProviderFullName = p.Provid_LName + ", " + p.Provid_FName

                        }) ;
                        responseModel.Status = "success";
                        responseModel.Response = res;   
                    }
                }
            }
            catch (Exception )
            {
                throw;
            }
            return responseModel;
        }

        public async Task<ResponseModel> GetChargesBreakDownReport(PatelReportsRequestModel request)
        {
            ResponseModel res = new ResponseModel();
            PagingResponse pagingResponse = new PagingResponse();
            try
            {
                if (request.PagedRequest.isExport == true)
                {
                    using (var ctx = new NPMDBEntities())
                    {


                        var results = ctx.USP_Charge_Breakdown(request.PracticeCode, request.ProviderCode, request.DateFrom, request.DateTo).ToList();
                        var responseList = results.Select(c => new ChargesBreakdownReportResponse
                        {
                            CODE=c.CODE,
                            CPT=c.CPT,
                            DESCRIPTION=c.DESCRIPTION,
                            Max_RVU=c.Max_RVU,
                            UNITS=c.UNITS,
                            Total_RVU_Value=c.Total_RVU_Value,
                            COUNT=c.COUNT,
                            CHARGES=c.CHARGES,
                            PERCENTAGE = c.PERCENTAGE,
                                AVERAGE=c.AVERAGE,
                            BILLED=c.BILLED,
                            PERCENTAGE_SEC = c.PERCENTAGE_SEC,
                            AVERAGE_SEC = c.AVERAGE_SEC,
                            CONTRACT_WO=c.CONTRACTUALS_WO,
                            PAYMENTS_IN_PERIOD=c.PAYMENT_IN_PERIOD,
                            ADJUSTMENTS_IN_PERIOD=c.ADJUSTMENT_IN_PERIOD,

                        }).ToList();
                        res.Response = responseList;
                        res.Status = "success";
                    }
                }
                else
                {
                    using (var ctx = new NPMDBEntities())
                    {

                        var results = ctx.USP_Charge_Breakdown(request.PracticeCode, request.ProviderCode, request.DateFrom, request.DateTo)
                                .Skip((request.PagedRequest.page - 1) * request.PagedRequest.size).Take(request.PagedRequest.size).ToList();
                        pagingResponse.TotalRecords = ctx.USP_Charge_Breakdown(request.PracticeCode, request.ProviderCode, request.DateFrom, request.DateTo).Count();
                        pagingResponse.FilteredRecords = results.Count(); // Count after pagination
                        pagingResponse.CurrentPage = request.PagedRequest.page;
                        var responseList = results.Select(c => new ChargesBreakdownReportResponse
                        {
                            CODE = c.CODE,
                            CPT = c.CPT,
                            DESCRIPTION = c.DESCRIPTION,
                            Max_RVU = c.Max_RVU,
                            UNITS = c.UNITS,
                            Total_RVU_Value = c.Total_RVU_Value,
                            COUNT = c.COUNT,
                            CHARGES = c.CHARGES,
                            PERCENTAGE = c.PERCENTAGE,
                            AVERAGE = c.AVERAGE,
                            BILLED = c.BILLED,
                            PERCENTAGE_SEC = c.PERCENTAGE_SEC,
                            AVERAGE_SEC = c.AVERAGE_SEC,
                            CONTRACT_WO = c.CONTRACTUALS_WO,
                            PAYMENTS_IN_PERIOD = c.PAYMENT_IN_PERIOD,
                            ADJUSTMENTS_IN_PERIOD = c.ADJUSTMENT_IN_PERIOD,
                        }).ToList();

                        pagingResponse.data = responseList; ;
                        res.Response = pagingResponse;
                        res.Status = "success";
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public async Task<ResponseModel> GetNegativeBalanceReport(long practiceCode, string responsibleParty, string dateCriteria, DateTime dateFrom, DateTime dateTo, Boolean isExport, int pageSize,int PageNo)
        {
            ResponseModel res = new ResponseModel();
            PagingResponse pagingResponse = new PagingResponse();
            
            try
            {
                if (isExport == true)
                {
                    using (var ctx = new NPMDBEntities())
                    {


                        var results = ctx.Database.SqlQuery<USP_NEGATIVE_BALANCE_REPORT_Result>(
                          "EXEC USP_NEGATIVE_BALANCE_REPORT @practiceCode, @responsibleParty, @dateCriteria, @dateFrom, @dateTo",
                          new SqlParameter("@practiceCode", practiceCode),
                          new SqlParameter("@responsibleParty", responsibleParty ?? (object)DBNull.Value),
                          new SqlParameter("@dateCriteria", dateCriteria ?? (object)DBNull.Value),
                          
                          new SqlParameter("@dateFrom", dateFrom.ToString("yyyy-MM-dd")),
                          new SqlParameter("@dateTo", dateTo.ToString("yyyy-MM-dd"))
                      ).ToList();

                        var responseList = results.Select(c => new NegativeBalanceReportResponse
                        {
                            Practice_Code = c.PRACTICE_CODE,
                            Practice_Name = c.Prac_Name,
                            Patient_Account = c.PATIENT_ACCOUNT,
                            Patient_Name = c.PATIENT_NAME,
                            Claim_No = c.CLAIM_NO,
                            Created_By = c.Created_by,
                            DOS = c.DOS,
                            Bill_Date = c.BILL_DATE,
                            Attending_Physician = c.ATTENDING_PHYSICIAN,
                            Billing_Physician = c.BILLING_PHYSICIAN,
                            Claim_Total = c.CLAIM_TOTAL,
                            Amount_Paid = c.AMT_PAID,
                            Adjustment = c.ADJUSTMENT,
                            Amount_Due = c.Total_Due_Amount,
                            Primary_Ins_Payment = c.Pri_Ins_Payment,
                            Secondary_Ins_Payment = c.Sec_Ins_Payment,
                            Other_Ins_Payment = c.Oth_Ins_Payment,
                            Patient_Payment = c.Patient_Payment,
                            Primary_Status = c.PRI_STATUS,
                            Primary_Payer = c.PRI_PAYER,
                            Secondary_Status = c.SEC_STATUS,
                            Secondary_Payer = c.SEC_PAYER,
                            Other_Status = c.OTH_STATUS,
                            Other_Payer = c.OTH_PAYER,
                            Patient_Status = c.PAT_STATUS,
                            Aging_Payer_Type = c.AGING_PAYER_TYPE,
                            Aging_Payer = c.AGING_PAYER,
                            Patient_Credit_Balance = c.PATIENT_CREDIT_BALANCE,
                            Insurance_Overpaid = c.INSURANCE_OVER_PAID,

                            Moved_Date = c.MOVED_DATE
                        }).ToList();
                        res.Response = responseList;
                        res.Status = "success";
                    }
                }
                else
                {
                    using (var ctx = new NPMDBEntities())
                    {

                        var results = ctx.Database.SqlQuery<USP_NEGATIVE_BALANCE_REPORT_Result>(
                          "EXEC USP_NEGATIVE_BALANCE_REPORT @practiceCode, @responsibleParty, @dateCriteria, @dateFrom, @dateTo",
                          new SqlParameter("@practiceCode", practiceCode),
                          new SqlParameter("@responsibleParty", responsibleParty ?? (object)DBNull.Value),
                          new SqlParameter("@dateCriteria", dateCriteria ?? (object)DBNull.Value),
                          new SqlParameter("@dateFrom", dateFrom.ToString("yyyy-MM-dd")),
                          new SqlParameter("@dateTo", dateTo.ToString("yyyy-MM-dd"))
                      )
                                 .Skip((PageNo- 1) * pageSize).Take(pageSize).ToList();
                        pagingResponse.TotalRecords =  ctx.Database.SqlQuery<USP_NEGATIVE_BALANCE_REPORT_Result>(
                        "EXEC USP_NEGATIVE_BALANCE_REPORT @practiceCode, @responsibleParty, @dateCriteria, @dateFrom, @dateTo",
                          new SqlParameter("@practiceCode", practiceCode),
                          new SqlParameter("@responsibleParty", responsibleParty ?? (object)DBNull.Value),
                          new SqlParameter("@dateCriteria", dateCriteria ?? (object)DBNull.Value),
                        new SqlParameter("@dateFrom", dateFrom.ToString("yyyy-MM-dd")),
                        new SqlParameter("@dateTo", dateTo.ToString("yyyy-MM-dd"))
                    ).Count();
                        pagingResponse.FilteredRecords = results.Count(); // Count after pagination
                        pagingResponse.CurrentPage = PageNo;
                        var responseList = results.Select(c => new NegativeBalanceReportResponse
                        {
                            Practice_Code = c.PRACTICE_CODE,
                            Practice_Name = c.Prac_Name,
                            Patient_Account = c.PATIENT_ACCOUNT,
                            Patient_Name = c.PATIENT_NAME,
                            Claim_No = c.CLAIM_NO,
                            Created_By = c.Created_by,
                            DOS = c.DOS,
                            Bill_Date = c.BILL_DATE,
                            Attending_Physician = c.ATTENDING_PHYSICIAN,
                            Billing_Physician = c.BILLING_PHYSICIAN,
                            Claim_Total = c.CLAIM_TOTAL,
                            Amount_Paid = c.AMT_PAID,
                            Adjustment = c.ADJUSTMENT,
                            Amount_Due = c.Total_Due_Amount,
                            Primary_Ins_Payment = c.Pri_Ins_Payment,
                            Secondary_Ins_Payment = c.Sec_Ins_Payment,
                            Other_Ins_Payment = c.Oth_Ins_Payment,
                            Patient_Payment = c.Patient_Payment,
                            Primary_Status = c.PRI_STATUS,
                            Primary_Payer = c.PRI_PAYER,
                            Secondary_Status = c.SEC_STATUS,
                            Secondary_Payer = c.SEC_PAYER,
                            Other_Status = c.OTH_STATUS,
                            Other_Payer = c.OTH_PAYER,
                            Patient_Status = c.PAT_STATUS,
                            Aging_Payer_Type = c.AGING_PAYER_TYPE,
                            Aging_Payer = c.AGING_PAYER,
                            Patient_Credit_Balance = c.PATIENT_CREDIT_BALANCE,
                            Insurance_Overpaid = c.INSURANCE_OVER_PAID,

                            Moved_Date = c.MOVED_DATE
                        }).ToList();

                        pagingResponse.data = responseList; 
                        res.Response = pagingResponse;
                        res.Status = "success";
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
           
        }

        



        #endregion Reports
    }
}