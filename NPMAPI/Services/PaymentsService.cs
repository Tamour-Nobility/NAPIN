using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using EdiFabric.Core.Model.Edi.X12;
using Microsoft.AspNet.SignalR;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;
using Org.BouncyCastle.Asn1.Ocsp;

namespace NPMAPI.Services
{
    public class PaymentsService : IPaymentsRepository
    {


        public ResponseModel AddInsurancePayment(InsurancePaymentViewModel request)
        {
            ResponseModel objResponse = new ResponseModel();
            BATCHPAYMENT objModel = null;
            try
            {

                using (var ctx = new NPMDBEntities())
                {
                    objModel = new BATCHPAYMENT();
                    long BatchNo = Convert.ToInt64(ctx.SP_TableIdGenerator("BatchNo").FirstOrDefault().ToString());
                    objModel.BatchNo = BatchNo;
                    objModel.InsuranceID = request.InsuranceID;
                    objModel.PaymentTypeID = request.PaymentTypeID;
                    objModel.FacilityID = request.FacilityID;
                    objModel.DepositDate = request.DepositDate;
                    objModel.Amount = request.Amount;
                    objModel.CheckDate = request.CheckDate;
                    objModel.CheckNo = request.CheckNo;
                    objModel.EOBDate = request.EOBDate;
                    objModel.ReceivedDate = request.ReceivedDate;
                    objModel.NOtes = request.NOtes;
                    objModel.practice_code = request.prac_code;
                    objModel.PostedAmount = 0;


                    ctx.BATCHPAYMENTS.Add(objModel);
                    ctx.SaveChanges();
                    objResponse.Response = request.BatchNo;
                    objResponse.Status = "Success";


                }
            }
            catch (Exception ex)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel AddPatientPayment(PatientPayment request)
        {
            ResponseModel objResponse = new ResponseModel();
            BATCHPAYMENT objModel = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {


                    


                    objModel = new BATCHPAYMENT();
                    long BatchNo = Convert.ToInt64(ctx.SP_TableIdGenerator("BatchNo").FirstOrDefault().ToString());
                    objModel.BatchNo = BatchNo;
                    objModel.PatientName = request.PatientName;
                    objModel.PaymentTypeID = request.PaymentTypeID;
                    objModel.FacilityID = request.FacilityID;
                    objModel.DepositDate = request.DepositDate;
                    objModel.Amount = request.Amount;
                    objModel.PatientAccount = request.PatientAccount;
                    objModel.CheckDate = request.CheckDate;
                    objModel.CheckNo = request.CheckNo;
                    objModel.practice_code = request.prac_code;
                    objModel.PostedAmount = 0;
                    objModel.NOtes = null;
                    objModel.EOBDate = null;
                    objModel.ReceivedDate = null;
                    objModel.InsuranceID = null;

                    ctx.BATCHPAYMENTS.Add(objModel);
                    ctx.SaveChanges();
                    objResponse.Response = request.BatchNo;
                    objResponse.Status = "Success";


                }
            }
            catch (Exception ex)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public List<SelectListViewModel> GetPaymentList()
        {
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    return ctx.PaymentTypes.Select(paymenttype =>
                         new SelectListViewModel()
                         {
                             Id = paymenttype.PaymentTypeID,
                             Name = paymenttype.PaymentType1,
                             Meta = paymenttype.PaymentSource

                         }
                     ).ToList();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ResponseModel SearchPayment(PaymentsSearchRequestModel SearchModel)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<uspGetAllPaymentbySearch_Result> objpaymnetList = null;
                using (var ctx = new NPMDBEntities())
                {
                    objpaymnetList = ctx.uspGetAllPaymentbySearch((SearchModel.paymentFrom == null ? "" : SearchModel.paymentFrom), (SearchModel.CheckNo == null ? "" : SearchModel.CheckNo), (SearchModel.FacilityId == null ? "" : SearchModel.FacilityId), (SearchModel.postedBy == null ? "" : SearchModel.postedBy), (SearchModel.BatchNo == null ? "" : SearchModel.BatchNo), (SearchModel.paymentId == null ? "" : SearchModel.paymentId), (SearchModel.PatientName == null ? "" : SearchModel.PatientName), (SearchModel.InsuranceId == null ? "" : SearchModel.InsuranceId), (SearchModel.PaymentType == null ? "" : SearchModel.PaymentType), (SearchModel.PaymentStatus == null ? "" : SearchModel.PaymentStatus), (SearchModel.PaymentDateFrom == null ? "" : SearchModel.PaymentDateFrom), (SearchModel.PaymentDateTo == null ? "" : SearchModel.PaymentDateTo) , (SearchModel.practice_code == null ? "" : SearchModel.practice_code)).ToList();
                }

                if (objpaymnetList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objpaymnetList;
                }
                else
                {
                    objResponse.Status = "No Data Found";
                }
                return objResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ResponseModel GetClaimsSummary(string ClaimId, string practiceCode)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<SP_GetClaimById_Result> objclaimId = null;
               
                using (var ctx = new NPMDBEntities())
                {

                   var response = GetClaimsbyid(ClaimId, practiceCode);

                    if (response.Status == "success")
                    {
                        if (response.Response.Count > 0)
                        {
                            var statementsList = response.Response;
                            objclaimId = statementsList;
                        }

                    }
                    objResponse.Status = "Sucess";
                    objResponse.Response = objclaimId;


                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel GetClaimBypatientdetials(patientBasedClaimModel request)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<SP_Claim_patientbysearch_Result> objPatientClaim = null;

                using (var ctx = new NPMDBEntities())
                {

                    objPatientClaim = ctx.SP_Claim_patientbysearch((request.practiceCode == null ? "" : request.practiceCode), (request.PatientAccount == null ? "" : request.PatientAccount), (request.FacilityCode == null ? "" : request.FacilityCode), (request.Balance == 0 ? "" : request.Balance.ToString())).ToList();
                    objResponse.Status = "Sucess";
                    objResponse.Response = objPatientClaim;


                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel GetClaimByinsdetials(insBasedClaimModel request)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<SP_Claim_Insurancebysearch_Result> objinsClaim = null;

                using (var ctx = new NPMDBEntities())
                {

                    objinsClaim = ctx.SP_Claim_Insurancebysearch((request.practiceCode == null ? "" : request.practiceCode), (request.insId == null ? "" : request.insId), (request.FacilityCode == null ? "" : request.FacilityCode), (request.Balance == 0 ? "" : request.Balance.ToString()), (request.dateFrom == null ? "" : request.dateFrom), (request.dateTo == null ? "" : request.dateTo)).ToList();
                    objResponse.Status = "Sucess";
                    objResponse.Response = objinsClaim;
                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel PostClaims(postClaim request)
        {

            ResponseModel objResponse = new ResponseModel();
            string[] claims= null;

            claims=request.claimNo.Split(',');

            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    for (int i = 0; i < claims.Length; i++)
                    {
                        long BatchPatientClaimId = Convert.ToInt64(ctx.SP_TableIdGenerator("BatchPatientClaimId").FirstOrDefault().ToString());
                        ctx.BatchPatientClaims.Add(new BatchPatientClaim
                        {
                            BatchPatientClaimId = BatchPatientClaimId,
                            BatchNo = request.batchNo,
                            ClaimNo = Convert.ToInt64(claims[i]) 
                        });

                    }
                    ctx.SaveChanges();

                    var result = ctx.BATCHPAYMENTS.FirstOrDefault(p => p.BatchNo == request.batchNo);
                    if(result != null)
                    {
                        result.PostedAmount = request.postAmount;
                        ctx.SaveChanges();
                    }

                    objResponse.Status = "Sucess";
                }


            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;

        }

        public ResponseModel getClaimsDetails(long claimNo, long patientaccount)
        {


            ClaimsPaymentDetailModel claimsPaymentDetailModel = new ClaimsPaymentDetailModel();
            ResponseModel objResponse = new ResponseModel();
            try
            {

                using (var ctx = new NPMDBEntities())
                {
                    #region Claim Model
                    claimsPaymentDetailModel.ClaimModel = ctx.Claims.SingleOrDefault(c => c.Claim_No == claimNo && (c.Deleted == null || c.Deleted == false));
                    claimsPaymentDetailModel.ClaimModel.Facility_Name = (from c in ctx.Claims
                                                                         join f in ctx.Facilities on c.Facility_Code equals f.Facility_Code
                                                                         where c.Claim_No == claimNo
                                                                         select f.Facility_Name).FirstOrDefault();

                    #endregion


                    claimsPaymentDetailModel.claimPayments = (from ci in ctx.Claim_Payments
                                                              join ins in ctx.Insurances on ci.Insurance_Id equals ins.Insurance_Id into ps
                                                              from pp in ps.DefaultIfEmpty()
                                                              join inp in ctx.Insurance_Payers on pp.InsPayer_Id equals inp.Inspayer_Id into uc
                                                              from c in uc.DefaultIfEmpty()
                                                              where (ci.Deleted ?? false) == false && (ci.Claim_No == claimNo)
                                                              select new ClaimPaymentViewModel()
                                                              {
                                                                  claimPayments = ctx.Claim_Payments.Where(ici => ici.Claim_No == claimNo && ici.claim_payments_id == ci.claim_payments_id).FirstOrDefault(),
                                                                  InsurancePayerName = c.Inspayer_Description
                                                              }).OrderBy(p => p.claimPayments.Date_Entry).ToList();


                    claimsPaymentDetailModel.claimInusrance = (from ci in ctx.Claim_Insurance
                                                               join ins in ctx.Insurances on ci.Insurance_Id equals ins.Insurance_Id
                                                               join inp in ctx.Insurance_Payers on ins.InsPayer_Id equals inp.Inspayer_Id
                                                               join g in ctx.Guarantors on ci.Subscriber equals g.Guarantor_Code into uc
                                                               from c in uc.DefaultIfEmpty()
                                                               where (ci.Deleted ?? false) == false && (ci.Claim_No == claimNo)
                                                               select new ClaimInsuranceViewModel()
                                                               {
                                                                   claimInsurance = ctx.Claim_Insurance.Where(ici => ici.Claim_No == claimNo && ici.Claim_Insurance_Id == ci.Claim_Insurance_Id).FirstOrDefault(),
                                                                   InsurancePayerName = inp.Inspayer_Description,
                                                                   SubscriberName = c.Guarant_Fname + " " + c.Guarant_Lname
                                                               }).OrderBy(i => i.claimInsurance.Pri_Sec_Oth_Type.ToLower() == "o").ThenBy(i => i.claimInsurance.Pri_Sec_Oth_Type.ToLower() == "s").ThenBy(i => i.claimInsurance.Pri_Sec_Oth_Type.ToLower() == "p").ToList();


                    claimsPaymentDetailModel.claimCharges = ctx.Claim_Charges.Where(ci => ci.Claim_No == claimNo && (ci.Deleted == null || ci.Deleted == false)).Select(svm => new ClaimChargesViewModel { amt = "", Description = ctx.Procedures.FirstOrDefault(p => p.ProcedureCode == svm.Procedure_Code).ProcedureDescription, claimCharges = ctx.Claim_Charges.Where(ci => ci.Claim_No == claimNo && ci.claim_charges_id == svm.claim_charges_id).FirstOrDefault() }).OrderBy(c => c.claimCharges.Sequence_No).ToList();
                    for (int i = 0; i < claimsPaymentDetailModel.claimCharges.Count(); i++)
                    {
                        var dCode1 = claimsPaymentDetailModel.claimCharges[i].claimCharges.Drug_Code;
                        try
                        {
                            if (claimsPaymentDetailModel.claimCharges[i].claimCharges.Drug_Code != null && claimsPaymentDetailModel.claimCharges[i].claimCharges.Drug_Code.Length > 5)
                            {
                                var dCode = (claimsPaymentDetailModel.claimCharges[i].claimCharges.Drug_Code)?.Insert(5, "-");
                                if (dCode.Length >= 10)
                                    dCode1 = dCode?.Insert(10, "-");
                            }
                        }
                        catch (Exception ex)
                        {
                            NPMLogger.GetInstance().Error(ex.ToString());
                        }
                        claimsPaymentDetailModel.claimCharges[i].Drug_Code = dCode1;
                        claimsPaymentDetailModel.claimCharges[i].claimCharges.Drug_Code = claimsPaymentDetailModel.claimCharges[i].claimCharges.Drug_Code;
                    }

                    if (claimsPaymentDetailModel != null && claimsPaymentDetailModel.claimCharges != null)
                    {
                        foreach (var item in claimsPaymentDetailModel.claimCharges)
                        {
                            var maxDate = Convert.ToDateTime("10/01/2015");
                            var ndcModel = ctx.NDC_CrossWalk.Where(scf => scf.HCPCS_Code == item.claimCharges.Procedure_Code && scf.Effective_Date_To >= maxDate).ToList();
                            item.claimCharges.NDCCodeList = new List<SelectListViewModel>();
                            foreach (var nItem in ndcModel)
                            {
                                SelectListViewModel objmodel = new SelectListViewModel();
                                objmodel.Id = nItem.NDC_ID;
                                objmodel.Name = nItem.NDC2;
                                objmodel.Meta = new ExpandoObject();
                                objmodel.Meta.Qualifier = nItem.Qualifier;
                                item.claimCharges.NDCCodeList.Add(objmodel);
                            }
                        }
                    }


                }
                objResponse.Response = claimsPaymentDetailModel;
                objResponse.Status = "Sucess";

            }

            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;

        }

        public ResponseModel SaveClaimsDetails(ClaimsPaymentDetailModel[] cpd)
        {

            ResponseModel objResponse = new ResponseModel();
            bool isInsuranceChanged = false;
            double postAmount =0.00;
            long batch_no=0;
            foreach (var c in cpd)
            {
                ClaimsPaymentDetailModel UpdateClaim = c;
                batch_no = c.batch_no;
                try
                {
                    using (var db = new NPMDBEntities())
                    {
                        using (db.Database.BeginTransaction())
                        {




                            var originalclaim = db.Claims.Where(x => x.Claim_No == UpdateClaim.claims_no).FirstOrDefault();


                            if (originalclaim != null)
                            {
                                if (UpdateClaim.claimPayments != null)
                                {
                                    originalclaim.Pri_Ins_Payment = 0;
                                    originalclaim.Sec_Ins_Payment = 0;
                                    originalclaim.Oth_Ins_Payment = 0;
                                    originalclaim.Patient_Payment = 0;
                                    originalclaim.Amt_Paid = 0;
                                    originalclaim.Adjustment = 0;
                                    for (int i = 0; i < UpdateClaim.claimPayments.Count(); i++)
                                    {
                                        if (!(UpdateClaim.claimPayments[i].claimPayments.Deleted ?? false))
                                        {
                                            if (UpdateClaim.claimPayments[i].claimPayments.Payment_Source == "1")
                                                originalclaim.Pri_Ins_Payment += UpdateClaim.claimPayments[i].claimPayments.Amount_Paid != null ? UpdateClaim.claimPayments[i].claimPayments.Amount_Paid : 0;
                                            if (UpdateClaim.claimPayments[i].claimPayments.Payment_Source == "2")
                                                originalclaim.Sec_Ins_Payment += UpdateClaim.claimPayments[i].claimPayments.Amount_Paid != null ? UpdateClaim.claimPayments[i].claimPayments.Amount_Paid : 0;
                                            if (UpdateClaim.claimPayments[i].claimPayments.Payment_Source == "3")
                                                originalclaim.Oth_Ins_Payment += UpdateClaim.claimPayments[i].claimPayments.Amount_Paid != null ? UpdateClaim.claimPayments[i].claimPayments.Amount_Paid : 0;
                                            if (UpdateClaim.claimPayments[i].claimPayments.Payment_Source == "P" || UpdateClaim.claimPayments[i].claimPayments.Payment_Source == "C" || UpdateClaim.claimPayments[i].claimPayments.Payment_Source == "I")
                                                originalclaim.Patient_Payment += UpdateClaim.claimPayments[i].claimPayments.Amount_Paid != null ? UpdateClaim.claimPayments[i].claimPayments.Amount_Paid : 0;
                                            originalclaim.Amt_Paid += UpdateClaim.claimPayments[i].claimPayments.Amount_Paid != null ? UpdateClaim.claimPayments[i].claimPayments.Amount_Paid : 0;
                                            if (UpdateClaim.claimPayments[i].claimPayments.Amount_Adjusted != null)
                                                originalclaim.Adjustment += UpdateClaim.claimPayments[i].claimPayments.Amount_Adjusted;// != null ? decimal.Parse(UpdateClaim.claimPayments[i].AmountAdjusted) : 0;
                                        }
                                    }
                                }

                                originalclaim.Amt_Due = Convert.ToDecimal(originalclaim.Claim_Total) - (Convert.ToDecimal(originalclaim.Amt_Paid) + Convert.ToDecimal(originalclaim.Adjustment));
                                if (originalclaim.Amt_Due <= 0 && originalclaim.PTL_Status != true)
                                {
                                    originalclaim.Pri_Status = "P";
                                    originalclaim.Sec_Status = "P";
                                    originalclaim.Oth_Status = "P";
                                    originalclaim.Pat_Status = "P";
                                }
                                else
                                {
                                    originalclaim.Pri_Status = "";
                                    originalclaim.Sec_Status = "";
                                    originalclaim.Oth_Status = "";
                                    originalclaim.Pat_Status = "";
                                }

                            }


                            #region claimPayments
                            if (UpdateClaim.claimPayments != null)
                            {
                                foreach (var cPaymentItem in UpdateClaim.claimPayments)
                                {
                                    var item = cPaymentItem.claimPayments;
                                    if (item.claim_payments_id == 0 && (item.Deleted != null && !(bool)item.Deleted))
                                    {
                                        Claim_Payments cp = new Claim_Payments();
                                        long iClaimPaymentsId = Convert.ToInt64(db.SP_TableIdGenerator("Claim_Payments_Id").FirstOrDefault().ToString());
                                        cp.claim_payments_id = iClaimPaymentsId;
                                        cp.Created_By = UpdateClaim.userID;
                                        cp.Created_Date = DateTime.Now;
                                        if (item.Insurance_Id != null)
                                            cp.Insurance_Id = item.Insurance_Id;
                                        cp.Payment_Source = item.Payment_Source;
                                        if (item.ENTERED_FROM != null)
                                            cp.ENTERED_FROM = item.ENTERED_FROM;
                                        else
                                            cp.ENTERED_FROM = "";
                                        cp.Date_Adj_Payment = DateTime.Now;
                                        if (item.Date_Entry != null)
                                            cp.Date_Entry = item.Date_Entry;
                                        if (item.Date_Filing != null)
                                            cp.Date_Filing = item.Date_Filing;
                                        if (item.Sequence_No != null)
                                            cp.Sequence_No = item.Sequence_No;
                                        cp.Charged_Proc_Code = item.Charged_Proc_Code;
                                        cp.Paid_Proc_Code = item.Paid_Proc_Code;
                                        if (item.Units != null)
                                            cp.Units = item.Units;
                                        cp.MODI_CODE1 = item.MODI_CODE1;
                                        cp.MODI_CODE2 = item.MODI_CODE2;
                                        if (item.Amount_Approved != null)
                                            cp.Amount_Approved = item.Amount_Approved;
                                        if (item.Amount_Adjusted != null)
                                        {
                                            cp.Amount_Adjusted = item.Amount_Adjusted;
                                        }
                                        else
                                        {
                                            cp.Amount_Adjusted = 0;
                                        }
                                        if (item.Payment_Type != null)
                                            cp.Payment_Type = item.Payment_Type;
                                        else
                                            cp.Payment_Type = "";
                                        if (item.Amount_Paid != null)
                                            cp.Amount_Paid = item.Amount_Paid;
                                        cp.Claim_No = UpdateClaim.claims_no;
                                        cp.ERA_ADJUSTMENT_CODE = item.ERA_ADJUSTMENT_CODE;
                                        cp.ERA_Rejection_CATEGORY_CODE = item.ERA_Rejection_CATEGORY_CODE;
                                        cp.Payment_No = 123;
                                        cp.BATCH_NO = item.BATCH_NO;
                                        if (item.DEPOSITSLIP_ID != null)
                                            cp.DEPOSITSLIP_ID = item.DEPOSITSLIP_ID;
                                        if (item.BATCH_DATE != null)
                                            cp.BATCH_DATE = item.BATCH_DATE;
                                        cp.Details = item.Details;
                                        cp.Check_No = item.Check_No;
                                        if (item.Reject_Amount != null)
                                            cp.Reject_Amount = item.Reject_Amount;
                                        cp.Reject_Type = item.Reject_Type;
                                        if (item.DepositDate != null)
                                            cp.DepositDate = item.DepositDate;


                                      
                                        db.Claim_Payments.Add(cp);
                                        if (!string.IsNullOrEmpty(cp.Insurance_Id.ToString()))
                                        {
                                            //////////var result = db.Database.SqlQuery<string>("select top 1 call_status from calls where call_claim_no='" + UpdateClaim.claimNo + "'   and Call_Insurance_Id='" + cp.Insurance_Id + "'   and Call_Status in('IP','WT')   and isnull(Call_Deleted,0)=0 ").FirstOrDefault();
                                            //////////if (!string.IsNullOrEmpty(result))
                                            //////////{
                                            //////////    if (result.Trim() == "IP" || result.Trim() == "WT")
                                            //////////    {
                                            //////////        db.Database.ExecuteSqlCommand("update calls set call_status='DP' where call_claim_no='" + UpdateClaim.claimNo + "'  and Call_Status in('IP','WT') ");
                                            //////////        res.callmsg = true;
                                            //////////    }
                                            //////////}
                                        }
                                        //string count = "select count(*) from calls where Call_Claim_No='" + UpdateClaim.claimNo + "' and Call_Status in('IP','WT')  and isnull(Call_Deleted,0)=0 ";
                                        //var k = db.Database.SqlQuery<int>(count).Single();
                                        //if (k > 0)
                                        //{
                                        //    string dropCall = "update calls set Call_Status='DP' where call_claim_no='" + UpdateClaim.claimNo + "' and call_status in ('IP','WT') and isnull(call_deleted,0)=0";
                                        //    db.Database.ExecuteSqlCommand(dropCall);
                                        //}
                                    }
                                    else
                                    {
                                        var cp = db.Claim_Payments.Find(item.claim_payments_id);
                                        if (cp != null)
                                        {
                                            if (item.Deleted != null && (bool)item.Deleted)
                                            {
                                                cp.Deleted = true;
                                            }
                                            if (item.IsRectify != null && (bool)item.IsRectify)
                                            {
                                                cp.IsRectify = true;
                                            }
                                            cp.Payment_Source = item.Payment_Source;
                                            if (item.Insurance_Id != null)
                                                cp.Insurance_Id = item.Insurance_Id;
                                            else
                                                cp.Insurance_Id = null;
                                            if (item.ENTERED_FROM != null)
                                                cp.ENTERED_FROM = item.ENTERED_FROM;
                                            else
                                                item.ENTERED_FROM = "";
                                            if (item.Date_Entry != null)
                                                cp.Date_Entry = item.Date_Entry;
                                            if (item.Date_Filing != null)
                                                cp.Date_Filing = item.Date_Filing;
                                            if (item.Sequence_No != null)
                                                cp.Sequence_No = item.Sequence_No;
                                            cp.Charged_Proc_Code = item.Charged_Proc_Code;
                                            cp.Paid_Proc_Code = item.Paid_Proc_Code;
                                            if (item.Payment_Type != null)
                                                cp.Payment_Type = item.Payment_Type;
                                            else
                                                item.Payment_Type = "";
                                            if (item.Units != null)
                                                cp.Units = item.Units;
                                            cp.MODI_CODE1 = item.MODI_CODE1;
                                            cp.MODI_CODE2 = item.MODI_CODE2;
                                            if (item.Amount_Approved != null)
                                            {
                                                cp.Amount_Approved = item.Amount_Approved;
                                            }
                                            else
                                            {
                                                cp.Amount_Approved = 0;
                                            }
                                            if (item.Amount_Adjusted != null)
                                            {
                                                cp.Amount_Adjusted = item.Amount_Adjusted;
                                            }
                                            else
                                            {
                                                cp.Amount_Adjusted = 0;
                                            }

                                            if (item.Amount_Paid != null)
                                            {
                                                cp.Amount_Paid = item.Amount_Paid;
                                            }
                                            else
                                            {
                                                cp.Amount_Paid = 0;
                                            }
                                            cp.ERA_ADJUSTMENT_CODE = item.ERA_ADJUSTMENT_CODE;
                                            cp.ERA_Rejection_CATEGORY_CODE = item.ERA_Rejection_CATEGORY_CODE;
                                            cp.Payment_No = 123;
                                            cp.Modified_By = UpdateClaim.userID;
                                            cp.Modified_Date = DateTime.Now;
                                            cp.BATCH_NO = item.BATCH_NO;
                                            if (item.DEPOSITSLIP_ID != null)
                                                cp.DEPOSITSLIP_ID = item.DEPOSITSLIP_ID;
                                            if (item.BATCH_DATE != null)
                                                cp.BATCH_DATE = item.BATCH_DATE;
                                            cp.Details = item.Details;
                                            cp.Check_No = item.Check_No;
                                            cp.Reject_Type = item.Reject_Type;
                                            if (item.Reject_Amount != null)
                                                cp.Reject_Amount = item.Reject_Amount;
                                            if (item.DepositDate != null)
                                                cp.DepositDate = item.DepositDate;
                                        }
                                    }
                                    
                                    db.SaveChanges();
                                }
                            }
                            #endregion claimPayments

                            postAmount = c.PostedAmount;
                            long BatchPatientClaimId = Convert.ToInt64(db.SP_TableIdGenerator("BatchPatientClaimId").FirstOrDefault().ToString());
                            db.BatchPatientClaims.Add(new BatchPatientClaim
                            {
                                BatchPatientClaimId = BatchPatientClaimId,
                                BatchNo = c.batch_no,
                                ClaimNo = Convert.ToInt64(c.claims_no)
                            });
                            db.SaveChanges();


                            

                            db.Database.CurrentTransaction.Commit();
                            objResponse.Status = "Sucess";


                        }

                    }
                }
                catch (Exception)
                {
                    objResponse.Status = "Error";
                }

            }
            try
            {

                using (var db = new NPMDBEntities())
                {
                    var result = db.BATCHPAYMENTS.FirstOrDefault(p => p.BatchNo == batch_no);
                    if (result != null)
                    {
                        result.PostedAmount = Convert.ToDecimal( postAmount);
                        db.SaveChanges();
                    }
                }
                   
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }



            return objResponse;

        }


        public ResponseModel checkPostedClaims(long batchno , long practiceCode)
        {
            List<SP_GetClaimById_Result> objclaimId = null;
            ResponseModel objResponse = new ResponseModel();
            List< BatchPatientClaim>  myClaims = new List< BatchPatientClaim >();

            string[] claims = { };
            
            try
            {
                using ( var db = new NPMDBEntities())
                {
                    claims = db.BatchPatientClaims.Where(b => b.BatchNo == batchno).Select(b=> b.ClaimNo.ToString()).ToArray();

                    if(claims.Length > 0)
                    {
                        string claimsNO = string.Join(",", claims);
                       


                        var response = GetClaimsbyidforChecking(claimsNO, practiceCode.ToString());

                        if (response.Status == "success")
                        {
                            if (response.Response.Count > 0)
                            {
                                var statementsList = response.Response;
                                objclaimId = statementsList;
                                foreach( var c in objclaimId) { 
                                c.isPosted = true;
                                
                                }
                            }

                        }
                        objResponse.Status = "Sucess";
                        objResponse.Response = objclaimId;
                    }



                }


            }catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;

        }

        private string GetClaimPOS(List<Claim_Charges> claimCharges)
        {
            try
            {
                var charges = claimCharges?.Where(cc => (cc.Deleted ?? false) == false).OrderBy(c => c.Created_Date).FirstOrDefault();
                if (charges == null)
                {
                    return "";
                }
                else
                {
                    return (charges.POS != null && charges.POS != 0) ? charges.POS.ToString() : "";

                }
            }
            catch (Exception)
            {
                throw;
            }
        
        }

        private ResponseModel GetClaimsbyid(string claimsid, string prac_code)
        {
            List<SP_GetClaimById_Result> claimsList = new List<SP_GetClaimById_Result>();
            ResponseModel res = new ResponseModel();
            try
            {
                using (var db = new NPMDBEntities())
                {


                    claimsList.AddRange(
                            db.Database.SqlQuery<SP_GetClaimById_Result>("SP_GetclaimbymultiIDS @ClaimId,@practiceCode", parameters: new[] {
                                new SqlParameter("@ClaimId", claimsid),
                                new SqlParameter("@practiceCode", prac_code),

                            }).ToList());
                    res.Status = "success";
                    res.Response = claimsList;

                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }


        private ResponseModel GetClaimsbyidforChecking(string claimsid, string prac_code)
        {
            List<SP_GetClaimById_Result> claimsList = new List<SP_GetClaimById_Result>();
            ResponseModel res = new ResponseModel();
            try
            {
                using (var db = new NPMDBEntities())
                {


                    claimsList.AddRange(
                            db.Database.SqlQuery<SP_GetClaimById_Result>("SP_GetclaimbymultiIDSforchecking @ClaimId,@practiceCode", parameters: new[] {
                                new SqlParameter("@ClaimId", claimsid),
                                new SqlParameter("@practiceCode", prac_code),

                            }).ToList());
                    res.Status = "success";
                    res.Response = claimsList;

                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }









    }



}
