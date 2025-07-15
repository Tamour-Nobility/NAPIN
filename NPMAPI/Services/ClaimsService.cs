using Microsoft.Ajax.Utilities;
using NPMAPI.Models;
using NPMAPI.Models.InboxHealth;
using NPMAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data;
using EdiFabric.Core.Model.Edi.X12;

namespace NPMAPI.Services
{
    public partial class DemographicService : IDemographicRepository
    {
        private readonly  IUBClaim uBClaim;

        // Constructor

        
        public DemographicService(IUBClaim _uBClaim)
        {
            this.uBClaim = _uBClaim;
        }


        public ResponseModel SaveClaimModel([FromBody] ClaimsViewModel ClaimModel)
        {



             
            ResponseModel objResponse = new ResponseModel();
            Claim objClaim = null;
            using (var ctx = new NPMDBEntities())
            {
                objClaim = ctx.Claims.SingleOrDefault(p => p.Claim_No == ClaimModel.ClaimModel.Claim_No);
                if (objClaim == null)
                {
                    ctx.Claims.Add(ClaimModel.ClaimModel);
                    ctx.SaveChanges();
                }
                else
                {
                    objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    objClaim.Attending_Physician = ClaimModel.ClaimModel.Attending_Physician;
                    objClaim.Resource_Physician = ClaimModel.ClaimModel.Resource_Physician;
                    objClaim.Supervising_Physician = ClaimModel.ClaimModel.Supervising_Physician;
                    objClaim.Billing_Physician = ClaimModel.ClaimModel.Billing_Physician;
                    objClaim.Location_Code = ClaimModel.ClaimModel.Location_Code;
                    objClaim.Referring_Physician = ClaimModel.ClaimModel.Referring_Physician;
                    objClaim.Referral_Number = ClaimModel.ClaimModel.Referral_Number;
                    objClaim.PA_Number = ClaimModel.ClaimModel.PA_Number;
                    objClaim.Additional_Claim_Info = ClaimModel.ClaimModel.Additional_Claim_Info;
                    objClaim.Is_Self_Pay = ClaimModel.ClaimModel.Is_Self_Pay;
                    //  objClaim.Pri_Ins_Payment = ClaimModel.ClaimModel.Pri_Ins_Payment;
                    objClaim.Pri_Status = ClaimModel.ClaimModel.Pri_Status;
                    //  objClaim.Sec_Ins_Payment = ClaimModel.ClaimModel.Sec_Ins_Payment;
                    objClaim.Sec_Status = ClaimModel.ClaimModel.Sec_Status;
                    //    objClaim.Oth_Ins_Payment = ClaimModel.ClaimModel.Oth_Ins_Payment; Same missing for Patient Payment
                    objClaim.Oth_Status = ClaimModel.ClaimModel.Oth_Status;
                    objClaim.Pat_Status = ClaimModel.ClaimModel.Pat_Status;
                    objClaim.Facility_Code = ClaimModel.ClaimModel.Facility_Code;
                    objClaim.Hospital_From = ClaimModel.ClaimModel.Hospital_From;
                    objClaim.Hospital_To = ClaimModel.ClaimModel.Hospital_To;
                    objClaim.Hospital_From = ClaimModel.ClaimModel.Hospital_From;  // No Room Number column in table
                    objClaim.PTL_Status = ClaimModel.ClaimModel.PTL_Status;
                    if ((bool)ClaimModel.ClaimModel.PTL_Status)
                    {
                        objClaim.Delay_Reason_Code = ClaimModel.ClaimModel.Delay_Reason_Code;
                        // Claim Feedback
                        Claims_Ptl_Feedback objClaimFeedback = new Claims_Ptl_Feedback();
                        objClaimFeedback.Claim_No = ClaimModel.ClaimModel.Claim_No;
                        objClaimFeedback.User_Notes = ClaimModel.PTLReasonDetail;
                        if (string.IsNullOrEmpty(ClaimModel.PTLReasonDoctorFeedback))
                        {
                            objClaimFeedback.FeedBack = ClaimModel.PTLReasonDoctorFeedback;
                            objClaimFeedback.FeedBack_Date = System.DateTime.Now;
                        }
                        objClaimFeedback.Reasons = ClaimModel.ClaimModel.Delay_Reason_Code;
                    }
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    ctx.SaveChanges();
                }
            }
            if (objClaim != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objClaim.Claim_No;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }



        public async Task<ResponseModel> SaveClaim(ClaimsViewModel cr,  long userId )
         {





            ResponseModel objResponse = new ResponseModel();

            var syncedPractice = _deltaSyncRepository.GetSyncedPractice(cr.PracticeCode);
            if (!CanSaveClaim(cr))
            {
                objResponse.Status = "Claim Cannot be Created after Patient Death";
                return objResponse;
            }
            #region New Claim
            if (cr.ClaimModel.Claim_No == 0)
            {
                using (var db = new NPMDBEntities())
                {

                    // Log start of operation for debugging
                    // Get the actual Billing Physician's full name
                    var billingPhysicianName = (from p in db.Providers
                                                where p.Provider_Code == cr.ClaimModel.Billing_Physician
                                                select p.Provid_LName + ", " + p.Provid_FName).FirstOrDefault();

                    // Get the actual Location description
                    var locationDescription = (from loc in db.Practice_Locations
                                               where loc.Location_Code == cr.ClaimModel.Location_Code
                                               select loc.Location_Name).FirstOrDefault();

                    // Log the key details to debug
                    // Check if an appointment exists with the same DOS, Billing Provider, Location, and Patient Account
                    var existingAppointment = db.SP_PATIENTAPPSEARCH(cr.ClaimModel.Patient_Account)
                        .FirstOrDefault(app => app.DATE == cr.ClaimModel.DOS?.ToString("MM/dd/yyyy")
                            && app.BILLING_PHYSICIAN == billingPhysicianName
                            && app.FACILITY == locationDescription
                            && app.STATUS == "Pending");

                    if (existingAppointment != null)
                    {
                        // Update the status of the appointment to "Completed"
                        existingAppointment.STATUS = "Completed";
                        var appn = db.Appointments.Where(x => x.Appointment_Id == existingAppointment.APPOINTMENT_ID).FirstOrDefault();
                        if (appn != null)
                        {
                            appn.Appointment_Status_Id = 6;
                            db.Entry(appn).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        // Save changes to the database
                    }
                    else
                    {
                    }

                    // Check if an appointment exists with the same DOS, Billing Provider, Location, and Patient Account
                    //    var existingAppointment = db.SP_PATIENTAPPSEARCH(cr.PatientAccount)
                    //.FirstOrDefault(app => app.DATE == cr.ClaimModel.DOS?.ToString("yyyy-MM-dd")
                    //    && app.BILLING_PHYSICIAN == cr.ClaimModel.Billing_Physician?.ToString()
                    //    && app.FACILITY == cr.ClaimModel.Location_Code?.ToString()
                    //    && app.STATUS == "Pending");

                    string cptLog = string.Empty;
                    using (db.Database.BeginTransaction())
                    {
                        try
                        {
                            #region Claim
                            Claim c = new Claim();
                            c.Patient_Account = cr.PatientAccount;
                           
                            if (cr.ClaimModel.Claim_No == 0)
                            {
                                long iClaimNo = Convert.ToInt64(db.SP_TableIdGenerator("Claim_No").FirstOrDefault().ToString());
                                cr.ClaimModel.Claim_No = iClaimNo;
                                c.practice_code = cr.PracticeCode;
                            }
                            c.Claim_No = cr.ClaimModel.Claim_No;
                            c.Claim_Type = cr.ClaimModel.Claim_Type ;

                            c.Deleted = false;
                            if (!string.IsNullOrEmpty(cr.ClaimModel.BATCH_NO))
                                c.BATCH_NO = cr.ClaimModel.BATCH_NO;
                            if (!string.IsNullOrEmpty(cr.ClaimModel.Batch_Date.ToString()))
                                c.Batch_Date = DateTime.Parse(cr.ClaimModel.Batch_Date.ToString());
                            if (!string.IsNullOrEmpty(cr.ClaimModel.DOS.ToString()))
                                c.DOS = DateTime.Parse(cr.ClaimModel.DOS.ToString());
                            c.Bill_Date = DateTime.Now;
                            c.Location_Code = cr.ClaimModel.Location_Code;
                            c.Attending_Physician = cr.ClaimModel.Attending_Physician;
                            c.Resource_Physician = cr.ClaimModel.Resource_Physician;
                            c.Created_By = userId;
                            c.Created_Date = DateTime.Now;
                            c.Scan_Date = cr.ClaimModel.Scan_Date;
                            c.Billing_Physician = cr.ClaimModel.Billing_Physician;
                            c.Referring_Physician = cr.ClaimModel.Referring_Physician;
                            c.Supervising_Physician = cr.ClaimModel.Supervising_Physician;
                            c.Referral_Number = cr.ClaimModel.Referral_Number;
                            c.PA_Number = cr.ClaimModel.PA_Number;
                            c.Additional_Claim_Info = cr.ClaimModel.Additional_Claim_Info;
                            c.Is_Corrected = false;
                            c.RSCode = null;
                            c.ICN_Number = null;
                            c.Is_Draft = cr.ClaimModel.Is_Draft;
                            c.PTL_Status = c.Is_Draft == true ? true : false;
                            // Self Pay Change
                            if (c.PTL_Status != true)
                            {
                                c.Pri_Status = cr.ClaimModel.Pri_Status;
                                c.Sec_Status = cr.ClaimModel.Sec_Status;
                                c.Oth_Status = cr.ClaimModel.Oth_Status;
                                c.Pat_Status = cr.ClaimModel.Pat_Status;
                            }
                            else
                            {
                                c.Pri_Status = "";
                                c.Sec_Status = "";
                                c.Oth_Status = "";
                                c.Pat_Status = "";
                                Claims_Ptl_Feedback objClaimFeedback = new Claims_Ptl_Feedback();
                                objClaimFeedback.Claim_No = cr.ClaimModel.Claim_No;
                                objClaimFeedback.User_Notes = cr.PTLReasonDetail;
                                if (string.IsNullOrEmpty(cr.PTLReasonDoctorFeedback))
                                {
                                    objClaimFeedback.FeedBack = cr.PTLReasonDoctorFeedback;
                                    objClaimFeedback.FeedBack_Date = System.DateTime.Now;
                                }
                                c.Delay_Reason_Code = cr.ClaimModel.Delay_Reason_Code;
                            }
                            c.Is_Self_Pay = cr.ClaimModel.Is_Self_Pay;
                            c.Copay_Owed = cr.ClaimModel.Copay_Owed;
                            c.Copay_Waived = cr.ClaimModel.Copay_Waived;
                            c.Stop_Submission = cr.ClaimModel.Stop_Submission;
                            c.Facility_Code = cr.ClaimModel.Facility_Code;
                            c.Hospital_From = cr.ClaimModel.Hospital_From;
                            c.Hospital_To = cr.ClaimModel.Hospital_To;
                            c.Reference_Number = cr.ClaimModel.Reference_Number;
                            if (cr.ClaimModel.PTL_Status != true)
                            {
                                c.resolve = cr.ClaimModel.resolve;
                                c.PAGE_NO = cr.ClaimModel.PAGE_NO;
                                c.scan_no = cr.ClaimModel.scan_no;
                                if (cr.ClaimModel.Scan_Date != null)
                                {
                                    c.Scan_Date = cr.ClaimModel.Scan_Date;
                                }
                            }
                            if (cr.ClaimModel != null)
                            {
                                c.DX_Code1 = cr.ClaimModel.DX_Code1;
                                c.DX_Code2 = cr.ClaimModel.DX_Code2;
                                c.DX_Code3 = cr.ClaimModel.DX_Code3;
                                c.DX_Code4 = cr.ClaimModel.DX_Code4;
                                c.DX_Code5 = cr.ClaimModel.DX_Code5;
                                c.DX_Code6 = cr.ClaimModel.DX_Code6;
                                c.DX_Code7 = cr.ClaimModel.DX_Code7;
                                c.DX_Code8 = cr.ClaimModel.DX_Code8;
                                c.DX_Code9 = cr.ClaimModel.DX_Code9;
                                c.DX_Code10 = cr.ClaimModel.DX_Code10;
                                c.DX_Code11 = cr.ClaimModel.DX_Code11;
                                c.DX_Code12 = cr.ClaimModel.DX_Code12;
                            }
                            c.Claim_Status = cr.ClaimModel.Claim_Status;
                            if (cr.ClaimModel != null)
                            {
                                c.Injury_Date = cr.ClaimModel.Injury_Date;
                                c.Accident_Auto = cr.ClaimModel.Accident_Auto;
                                c.Accident_Other = cr.ClaimModel.Accident_Other;
                                c.Employment = cr.ClaimModel.Employment;
                                c.Accident_Emergency = cr.ClaimModel.Accident_Emergency;
                                if (cr.ClaimModel.Accident_Date != null)
                                    c.Accident_Date = cr.ClaimModel.Accident_Date;
                                c.Accident_State = cr.ClaimModel.Accident_State;
                                c.Spinal_Manipulation_Xray_Availability = cr.ClaimModel.Spinal_Manipulation_Xray_Availability;
                                c.Manifestation_Date = cr.ClaimModel.Manifestation_Date;
                                c.Spinal_Manipulation_Description = cr.ClaimModel.Spinal_Manipulation_Description;
                                c.Spinal_Manipulation_Condition_Code = cr.ClaimModel.Spinal_Manipulation_Condition_Code;
                                c.Phy_Exam_Code = cr.ClaimModel.Phy_Exam_Code;
                                c.Phy_Exam_Desc = cr.ClaimModel.Phy_Exam_Desc;
                                c.Institution_Condition_Code = cr.ClaimModel.Institution_Condition_Code;
                                c.ServiceAuthExceptionCode = cr.ClaimModel.ServiceAuthExceptionCode;
                                c.Special_Program_Code = cr.ClaimModel.Special_Program_Code;
                                c.Weight = cr.ClaimModel.Weight;
                                c.Transport_Distance = cr.ClaimModel.Transport_Distance;
                                c.Transportation_Reason_Code = cr.ClaimModel.Transportation_Reason_Code;
                                c.Transportation_Condition_Code = cr.ClaimModel.Transportation_Condition_Code;
                                c.Condition_Indicator = cr.ClaimModel.Condition_Indicator;
                                c.Transport_Code = cr.ClaimModel.Transport_Code;
                                c.Ordering_Physician = cr.ClaimModel.Ordering_Physician;
                                if (cr.ClaimModel.Attach_Type_Code != null)
                                    c.Attach_Type_Code = cr.ClaimModel.Attach_Type_Code;
                                if (!string.IsNullOrEmpty(cr.ClaimModel.Last_Seen_Physician.ToString()))
                                    c.Last_Seen_Physician = cr.ClaimModel.Last_Seen_Physician;
                                if (cr.ClaimModel.Last_Seen_Date != null)
                                    c.Last_Seen_Date = cr.ClaimModel.Last_Seen_Date;
                                if (cr.ClaimModel.Plan_Code != null)
                                    c.Plan_Code = cr.ClaimModel.Plan_Code;
                                c.Start_Care_Date = cr?.ClaimModel?.Start_Care_Date;
                                c.ASSUMED_CARE_DATE = cr.ClaimModel.ASSUMED_CARE_DATE;
                                c.LMP_Date = cr?.ClaimModel?.LMP_Date;
                                c.X_Ray_Date = cr?.ClaimModel?.X_Ray_Date;
                                c.Last_Work_Date = cr?.ClaimModel?.Last_Work_Date;
                                c.EDC_date = cr?.ClaimModel?.EDC_date;
                                c.RELINQISHED_CARE_DATE = cr?.ClaimModel?.RELINQISHED_CARE_DATE;
                                c.AA = 1;
                                c.Current_Illness_Date = cr?.ClaimModel?.Current_Illness_Date;
                                c.TCM_Cal_Dos = cr.ClaimModel.TCM_Cal_Dos;
                                c.ICN_Number = cr.ClaimModel.ICN_Number;
                                c.HCFA_Note = cr.ClaimModel.HCFA_Note;
                            }
                            c.Include_In_Sdf = cr.ClaimModel.Include_In_Sdf;
                            c.Pri_Ins_Payment = 0;
                            c.Sec_Ins_Payment = 0;
                            c.Oth_Ins_Payment = 0;
                            c.Patient_Payment = 0;
                            c.Claim_Total = 0;
                            c.Adjustment = 0;
                            c.Amt_Due = 0;
                            c.Amt_Paid = 0;
                            for (int i = 0; i < cr.claimPayments.Count(); i++)
                            {
                                if (cr.claimPayments[i].claimPayments.Payment_Source == "1")
                                    c.Pri_Ins_Payment += cr.claimPayments[i].claimPayments.Amount_Paid != null ? cr.claimPayments[i].claimPayments.Amount_Paid : 0;
                                if (cr.claimPayments[i].claimPayments.Payment_Source == "2")
                                    c.Sec_Ins_Payment += cr.claimPayments[i].claimPayments.Amount_Paid != null ? cr.claimPayments[i].claimPayments.Amount_Paid : 0;
                                if (cr.claimPayments[i].claimPayments.Payment_Source == "3")
                                    c.Oth_Ins_Payment += cr.claimPayments[i].claimPayments.Amount_Paid != null ? cr.claimPayments[i].claimPayments.Amount_Paid : 0;
                                if (cr.claimPayments[i].claimPayments.Payment_Source == "P" || cr.claimPayments[i].claimPayments.Payment_Source == "C")
                                    c.Patient_Payment += cr.claimPayments[i].claimPayments.Amount_Paid != null ? cr.claimPayments[i].claimPayments.Amount_Paid : 0;
                                c.Amt_Paid += cr.claimPayments[i].claimPayments.Amount_Paid != null && cr.claimPayments[i].claimPayments.Amount_Paid.Value.ToString() != "" ? cr.claimPayments[i].claimPayments.Amount_Paid : 0;
                                c.Adjustment += cr.claimPayments[i].claimPayments.Amount_Adjusted != null && cr.claimPayments[i].claimPayments.Amount_Adjusted.Value.ToString() != "" ? cr.claimPayments[i].claimPayments.Amount_Adjusted : 0;
                            }
                            c.Claim_Total = 0;
                            for (int i = 0; i < cr.claimCharges.Count(); i++)
                            {
                                if (cr.claimCharges[i].claimCharges.Deleted != null && cr.claimCharges[i].claimCharges.Deleted != true)
                                    c.Claim_Total += cr.claimCharges[i].claimCharges.Amount != null ? cr.claimCharges[i].claimCharges.Amount : 0;
                            }
                            c.Amt_Due = Convert.ToDecimal(c.Claim_Total) - (Convert.ToDecimal(c.Amt_Paid) + Convert.ToDecimal(c.Adjustment));
                            if (cr.claimCharges.Count() > 0 && cr.claimCharges[0].claimCharges != null && cr.claimCharges[0].claimCharges.POS != null)
                            {
                                c.Pos = cr.claimCharges[0].claimCharges.POS.Value.ToString();
                            }
                            c.Additional_Estatement = cr.ClaimModel.Additional_Estatement;
                            c.Created_From = cr.ClaimModel.Created_From;
                            db.Claims.Add(c);
                            #endregion start
                            bool CLIA = false;
                            #region ClaimCharges
                            cptLog += "<claimCharges>";
                            for (var x = 0; x < cr.claimCharges.Count; x++)
                            {
                                Claim_Charges cc = new Claim_Charges();
                                if (cc.Deleted != true)
                                {
                                    if (!CLIA)
                                    {
                                        bool? isCLIA = db.Database.SqlQuery<bool>("select CONVERT(BIT,ISNULL(Clia_Number,'False')) from procedures where ProcedureCode='" + cr.claimCharges[x].claimCharges.Procedure_Code + "'").FirstOrDefault();
                                        if (isCLIA != null && isCLIA == true)
                                        {
                                            c.Add_CLIA_Number = true;
                                            CLIA = true;
                                        }
                                    }
                                    long iClaimChargesId = Convert.ToInt64(db.SP_TableIdGenerator("Claim_Charges_Id").FirstOrDefault().ToString());
                                    cc.DOE = DateTime.Now;
                                    cc.Claim_No = c.Claim_No;
                                    cc.claim_charges_id = iClaimChargesId;
                                    cc.Procedure_Code = cr.claimCharges[x].claimCharges.Procedure_Code;
                                    cc.Alternate_Code = cr.claimCharges[x].claimCharges.Alternate_Code;
                                    cc.Sequence_No = cr.claimCharges[x].claimCharges.Sequence_No;
                                    if (cr.claimCharges[x].claimCharges != null && cr.claimCharges[x].claimCharges.POS != null)
                                        cc.POS = cr.claimCharges[x].claimCharges.POS;
                                    cc.Deleted = false;
                                    cc.Created_By = userId;
                                    cc.Created_Date = DateTime.Now;
                                    cc.Units = cr?.claimCharges[x]?.claimCharges.Units;
                                    cc.Amount = cr?.claimCharges[x]?.claimCharges.Amount;
                                    cc.Dos_From = cr.claimCharges[x].claimCharges.Dos_From;
                                    cc.Dos_To = cr.claimCharges[x].claimCharges.Dos_To;
                                    if (cr.claimCharges[x].claimCharges.Start_Time != null)
                                        cc.Start_Time = cr.claimCharges[x].claimCharges.Start_Time;

                                    if (cr.claimCharges[x].claimCharges.Stop_Time != null)
                                        cc.Stop_Time = cr.claimCharges[x].claimCharges.Stop_Time;
                                    cc.DX_Pointer1 = cr.claimCharges[x].claimCharges.DX_Pointer1;
                                    cc.DX_Pointer2 = cr.claimCharges[x].claimCharges.DX_Pointer2;
                                    cc.DX_Pointer3 = cr.claimCharges[x].claimCharges.DX_Pointer3;
                                    cc.DX_Pointer4 = cr.claimCharges[x].claimCharges.DX_Pointer4;
                                    cc.Modi_Code1 = cr.claimCharges[x].claimCharges.Modi_Code1;
                                    cc.Modi_Code2 = cr.claimCharges[x].claimCharges.Modi_Code2;
                                    cc.Modi_Code3 = cr.claimCharges[x].claimCharges.Modi_Code3;
                                    cc.Modi_Code4 = cr.claimCharges[x].claimCharges.Modi_Code4;
                                    cc.Include_In_Edi = cr.claimCharges[x].claimCharges.Include_In_Edi;
                                    cc.IsRectify = cr.claimCharges[x].claimCharges.IsRectify;
                                    cc.NDC_Quantity = cr?.claimCharges[x]?.claimCharges.NDC_Quantity;
                                    cc.Drug_Code = (cr.claimCharges[x].Drug_Code)?.Replace("-", "");
                                    cc.Cpt_Type = "CPT";
                                    cc.NDC_Qualifier = cr.claimCharges[x].claimCharges.NDC_Qualifier;
                                    cc.Contractual_Amt = cr.claimCharges[x].claimCharges.Contractual_Amt;
                                    cc.Include_In_Sdf = cr.claimCharges[x].claimCharges.Include_In_Sdf;
                                    if (!string.IsNullOrEmpty(cr.claimCharges[x].claimCharges.Accession_No))
                                        cc.Accession_No = cr.claimCharges[x].claimCharges.Accession_No;
                                    if (!string.IsNullOrEmpty(cr.claimCharges[x].claimCharges.NDC_Service_Description))
                                        cc.NDC_Service_Description = cr.claimCharges[x].claimCharges.NDC_Service_Description;
                                    cc.Revenue_Code = cr?.claimCharges[x]?.claimCharges.Revenue_Code;
                                    cc.Weight = cr?.claimCharges[x]?.claimCharges.Weight;
                                    cc.Transport_Distance = cr.claimCharges[x].claimCharges.Transport_Distance;
                                    cc.Transportation_Reason_Code = cr.claimCharges[x].claimCharges.Transportation_Reason_Code;
                                    cc.Transportation_Condition_Code = cr.claimCharges[x].claimCharges.Transportation_Condition_Code;
                                    cc.Transport_Code = cr.claimCharges[x].claimCharges.Transport_Code;
                                    cc.Condition_Indicator = cr?.claimCharges[x]?.claimCharges.Condition_Indicator;
                                    cc.Emergency_Related = cr?.claimCharges[x]?.claimCharges.Emergency_Related;
                                    cc.Notes = cr?.claimCharges[x]?.claimCharges.Notes;
                                    cc.Physical_Modifier = cr?.claimCharges[x]?.claimCharges.Physical_Modifier;
                                    db.Claim_Charges.Add(cc);
                                }
                            }
                            cptLog += "</claimCharges>";
                            #endregion
                            #region Claim Payments
                            for (var x = 0; x < cr.claimPayments.Count; x++)
                            {
                                Claim_Payments cp = new Claim_Payments();
                                if (cr?.claimPayments[x]?.claimPayments.Deleted != true)
                                {
                                    long iClaimPaymentsId = Convert.ToInt64(db.SP_TableIdGenerator("Claim_Payments_Id").FirstOrDefault().ToString());//(from p in db.Web_GetMaxColumnID("Claim_Payments_Id") select p).SingleOrDefault();
                                    cp.claim_payments_id = iClaimPaymentsId;
                                    cp.Deleted = false;
                                    cp.Created_By = userId;
                                    cp.Created_Date = DateTime.Now;
                                    cp.Date_Adj_Payment = DateTime.Now;
                                    cp.Insurance_Id = cr.claimPayments[x].claimPayments.Insurance_Id;
                                    cp.Payment_Source = cr.claimPayments[x].claimPayments.Payment_Source;
                                    if (cr.claimPayments[x].claimPayments.ENTERED_FROM != null && cr.claimPayments[x].claimPayments.ENTERED_FROM.Length == 1)
                                        cp.ENTERED_FROM = cr?.claimPayments[x]?.claimPayments.ENTERED_FROM;
                                    else
                                        cp.ENTERED_FROM = "";
                                    if (cr.claimPayments[x].claimPayments.Payment_Type != null && cr.claimPayments[x].claimPayments.Payment_Type.Length == 1)
                                        cp.Payment_Type = cr.claimPayments[x].claimPayments.Payment_Type;
                                    else
                                        cp.Payment_Type = "";
                                    cp.Amount_Paid = cr.claimPayments[x].claimPayments.Amount_Paid;
                                    cp.Date_Entry = cr?.claimPayments[x]?.claimPayments.Date_Entry;
                                    cp.Date_Filing = cr.claimPayments[x].claimPayments.Date_Filing;
                                    cp.Sequence_No = cr?.claimPayments[x]?.claimPayments.Sequence_No;
                                    cp.Charged_Proc_Code = cr.claimPayments[x].claimPayments.Charged_Proc_Code;
                                    //Uncommentted the below code by Pir Ubaid (REASON : this code was  commented and Dos_From value was not binding) 8 March,2024.
                                    cp.DOS_From = cr.claimPayments[x].claimPayments.DOS_From;
                                    //Uncommentted the above code by Pir Ubaid (REASON : this code was  commented and Dos_From value was not binding) 8 March,2024.
                                    cp.Dos_To = cr.claimPayments[x].claimPayments.Dos_To;
                                    cp.Paid_Proc_Code = cr.claimPayments[x].claimPayments.Paid_Proc_Code;
                                    cp.Units = cr?.claimPayments[x]?.claimPayments.Units;
                                    cp.MODI_CODE1 = cr.claimPayments[x].claimPayments.MODI_CODE1;
                                    cp.MODI_CODE2 = cr.claimPayments[x].claimPayments.MODI_CODE2;
                                    cp.Amount_Approved = cr?.claimPayments[x]?.claimPayments.Amount_Approved;
                                    cp.Amount_Adjusted = cr?.claimPayments[x]?.claimPayments.Amount_Adjusted;
                                    cp.Claim_No = c.Claim_No;
                                    cp.ERA_ADJUSTMENT_CODE = cr.claimPayments[x].claimPayments.ERA_ADJUSTMENT_CODE;
                                    cp.ERA_Rejection_CATEGORY_CODE = cr.claimPayments[x].claimPayments.ERA_Rejection_CATEGORY_CODE;
                                    cp.Payment_No = 123;
                                    cp.DEPOSITSLIP_ID = cr.claimPayments[x].claimPayments.DEPOSITSLIP_ID;
                                    cp.BATCH_NO = cr.claimPayments[x].claimPayments.BATCH_NO;
                                    cp.BATCH_DATE = cr?.claimPayments[x]?.claimPayments.BATCH_DATE;
                                    cp.Details = cr.claimPayments[x].claimPayments.Details;
                                    cp.Check_No = cr.claimPayments[x].claimPayments.Check_No;
                                    cp.Reject_Type = cr.claimPayments[x].claimPayments.Reject_Type;
                                    cp.Reject_Amount = cr?.claimPayments[x]?.claimPayments.Reject_Amount;
                                    cp.DepositDate = cr.claimPayments[x]?.claimPayments.DepositDate;
                                    db.Claim_Payments.Add(cp);
                                }
                            }
                            #endregion
                            #region Claim Insurance
                            for (var x = 0; x < cr.claimInusrance.Count; x++)
                            {
                                if (cr.claimInusrance[x] != null && cr.claimInusrance[x].claimInsurance != null && cr.claimInusrance[x].claimInsurance.Deleted != null && !(bool)cr.claimInusrance[x].claimInsurance.Deleted)
                                {
                                    Claim_Insurance ci = new Claim_Insurance();
                                    long iClaimInsuranceId = Convert.ToInt64(db.SP_TableIdGenerator("Claim_Insurance_Id").FirstOrDefault().ToString());
                                    ci.Claim_Insurance_Id = iClaimInsuranceId;
                                    ci.Claim_No = c.Claim_No;
                                    ci.Deleted = false;
                                    ci.Created_By = userId;
                                    ci.Created_Date = DateTime.Now;
                                    ci.Insurance_Id = cr.claimInusrance[x].claimInsurance.Insurance_Id;
                                    ci.Patient_Account = cr.PatientAccount;
                                    ci.Pri_Sec_Oth_Type = cr.claimInusrance[x].claimInsurance.Pri_Sec_Oth_Type;
                                    ci.Co_Payment = cr.claimInusrance[x].claimInsurance.Co_Payment;
                                    ci.Deductions = cr?.claimInusrance[x]?.claimInsurance.Deductions;
                                    ci.Co_Insurance = cr.claimInusrance[x].claimInsurance.Co_Insurance;
                                    ci.Policy_Number = cr.claimInusrance[x].claimInsurance.Policy_Number;
                                    ci.Group_Number = cr.claimInusrance[x].claimInsurance.Group_Number;
                                    ci.Group_Name = cr.claimInusrance[x].claimInsurance.Group_Name;
                                    if (cr.claimInusrance[x].claimInsurance.Subscriber != null && cr.claimInusrance[x].claimInsurance.Subscriber != 0)
                                        ci.Subscriber = cr.claimInusrance[x].claimInsurance.Subscriber;
                                    ci.Relationship = cr.claimInusrance[x].claimInsurance.Relationship;
                                    ci.MCR_SEC_Payer_Code = cr.claimInusrance[x].claimInsurance.MCR_SEC_Payer_Code;
                                    ci.Print_Center = cr.claimInusrance[x].claimInsurance.Print_Center;
                                    ci.Corrected_Claim = cr.claimInusrance[x].claimInsurance.Corrected_Claim;
                                    ci.ICN = cr.claimInusrance[x].claimInsurance.ICN;
                                    ci.Late_Filing = cr.claimInusrance[x].claimInsurance.Late_Filing;
                                    ci.Send_notes = cr.claimInusrance[x].claimInsurance.Send_notes;
                                    ci.Late_Filing_Reason = cr.claimInusrance[x].claimInsurance.Late_Filing_Reason;
                                    ci.Notes = cr.claimInusrance[x].claimInsurance.Notes;
                                    ci.Send_Appeal = cr.claimInusrance[x].claimInsurance.Send_Appeal;
                                    ci.Medical_Notes = cr.claimInusrance[x].claimInsurance.Medical_Notes;
                                    ci.Reconsideration = cr.claimInusrance[x].claimInsurance.Reconsideration;
                                    ci.Returned_Hcfa = cr.claimInusrance[x].claimInsurance.Returned_Hcfa;
                                    ci.Effective_Date = cr?.claimInusrance[x]?.claimInsurance.Effective_Date;
                                    ci.Termination_Date = cr?.claimInusrance[x]?.claimInsurance.Termination_Date;
                                    ci.Plan_Name = cr?.claimInusrance[x]?.claimInsurance.Plan_Name;
                                    ci.Filing_Indicator = cr.claimInusrance[x].claimInsurance.Filing_Indicator;
                                    ci.Access_Carolina_Number = cr.claimInusrance[x].claimInsurance.Access_Carolina_Number;
                                    ci.Is_Capitated_Claim = cr?.claimInusrance[x]?.claimInsurance.Is_Capitated_Claim;
                                    ci.Allowed_Visits = cr?.claimInusrance[x]?.claimInsurance.Allowed_Visits;
                                    ci.Remaining_Visits = cr?.claimInusrance[x]?.claimInsurance.Remaining_Visits;
                                    ci.Visits_Start_Date = cr?.claimInusrance[x]?.claimInsurance.Termination_Date;
                                    ci.Visits_End_Date = cr?.claimInusrance[x]?.claimInsurance.Visits_End_Date;
                                    ci.Admission_Type_Code = cr.claimInusrance[x].claimInsurance.Admission_Type_Code;
                                    ci.Admission_Source_Code = cr.claimInusrance[x].claimInsurance.Admission_Source_Code;
                                    ci.Patient_Status_Code = cr.claimInusrance[x].claimInsurance.Patient_Status_Code;
                                    ci.Claim_Status_Request = cr?.claimInusrance[x]?.claimInsurance.Claim_Status_Request;
                                    ci.Co_Payment_Per = cr?.claimInusrance[x].claimInsurance.Co_Payment_Per;
                                    db.Claim_Insurance.Add(ci);
                                }
                            }
                            #endregion
                            #region Inbox Sync
                            //Check if the claim practice is already synced with inbox the set the UpdatedDate column of PracticeSynchronization table
                            //so that the next time the sync worker runs, it add this new patient, if it's due amount is > 0
                            if (syncedPractice != null)
                            {
                                PracticeSynchronization practiceSynchronization = db.PracticeSynchronizations.Where(x => x.PracticeId == syncedPractice.Practice_Code).FirstOrDefault();
                                practiceSynchronization.UpdatedDate = DateTime.Now;
                            }
                            #endregion


                            //if (claimType == "I") {
                            //    for (var i = 0; i < cr.UbClaimDropdown.CcOde.Count; i++) {
                            //        if (cr.UbClaimDropdown.CcOde[i].Date == null)
                            //        {
                            //            cr.UbClaimDropdown.CcOde[i].Date =DateTime.UtcNow;
                            //        }
                            //        }
                             
                            //    UBClaim ubService = new UBClaim();


                            //    await ubService.SaveDropdownsData(cr.UbClaimDropdown, c.Claim_No, cr.PracticeCode);


                            //}


                            db.SaveChanges();
                            db.Database.CurrentTransaction.Commit();
                            objResponse.Status = "Sucess";
                            objResponse.Response = c.Claim_No;
                        }
                        catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                        {
                            objResponse.Status = "Error";
                            objResponse.Response = dbEx;
                        }
                        catch (Exception ex)
                        {
                            objResponse.Status = "Error";
                            objResponse.Response = ex;
                        }
                    }
                }
            }
            #endregion
            #region Update Claim
            else
            {
                bool isInsuranceChanged = false;
                ClaimsViewModel UpdateClaim = cr;
                using (var db = new NPMDBEntities())
                {
                    using (db.Database.BeginTransaction())
                    {
                        // Log start of operation for debugging
                        Console.WriteLine("Starting to search for existing appointment...");

                        // Get the actual Billing Physician's full name
                        var billingPhysicianName = (from p in db.Providers
                                                    where p.Provider_Code == cr.ClaimModel.Billing_Physician
                                                    select p.Provid_LName + ", " + p.Provid_FName).FirstOrDefault();

                        // Get the actual Location description
                        var locationDescription = (from loc in db.Practice_Locations
                                                   where loc.Location_Code == cr.ClaimModel.Location_Code
                                                   select loc.Location_Name).FirstOrDefault();

                        // Log the key details to debug
                        Console.WriteLine($"Billing Physician: {billingPhysicianName}");
                        Console.WriteLine($"Location: {locationDescription}");
                        Console.WriteLine($"DOS: {cr.ClaimModel.DOS?.ToString("yyyy-MM-dd")}");

                        // Check if an appointment exists with the same DOS, Billing Provider, Location, and Patient Account
                        var existingAppointment = db.SP_PATIENTAPPSEARCH(cr.ClaimModel.Patient_Account)
                            .FirstOrDefault(app => app.DATE == cr.ClaimModel.DOS?.ToString("MM/dd/yyyy")
                                && app.BILLING_PHYSICIAN == billingPhysicianName
                                && app.FACILITY == locationDescription
                                && app.STATUS == "Pending");

                        if (existingAppointment != null)
                        {
                            Console.WriteLine("Matching appointment found, updating status.");

                            // Update the status of the appointment to "Completed"
                            existingAppointment.STATUS = "Completed";
                            var appn = db.Appointments.Where(x => x.Appointment_Id == existingAppointment.APPOINTMENT_ID).FirstOrDefault();
                            if (appn != null)
                            {
                                appn.Appointment_Status_Id = 6;
                                db.Entry(appn).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            // Save changes to the database


                            Console.WriteLine("Appointment status updated to 'Completed'.");
                        }
                        else
                        {
                            Console.WriteLine("No matching appointment found.");
                        }
                        try
                        {
                            #region claim

                            var originalclaim = db.Claims.Where(x => x.Claim_No == UpdateClaim.ClaimModel.Claim_No).FirstOrDefault();

                            if (originalclaim != null)
                            {
                                originalclaim.resolve = UpdateClaim.ClaimModel.resolve;
                                originalclaim.PAGE_NO = UpdateClaim.ClaimModel.PAGE_NO;
                                originalclaim.scan_no = UpdateClaim.ClaimModel.scan_no;
                                originalclaim.Scan_Date = UpdateClaim?.ClaimModel?.Scan_Date;
                                originalclaim.BATCH_NO = UpdateClaim?.ClaimModel?.BATCH_NO;
                                originalclaim.Batch_Date = UpdateClaim?.ClaimModel?.Batch_Date;
                                originalclaim.SCAN_DATE_PTL = UpdateClaim.ClaimModel.SCAN_DATE_PTL;
                                originalclaim.Is_Corrected = UpdateClaim.ClaimModel.Is_Corrected;
                                if (!(UpdateClaim.ClaimModel.Is_Corrected ?? false))
                                {
                                    originalclaim.ICN_Number = null;
                                    originalclaim.RSCode = null;
                                }
                                else
                                {
                                    originalclaim.ICN_Number = UpdateClaim.ClaimModel.ICN_Number;
                                    originalclaim.RSCode = UpdateClaim.ClaimModel.RSCode;
                                }
                                var cn = db.CLAIM_NOTES.Where(x => x.Claim_No == UpdateClaim.ClaimModel.Claim_No).FirstOrDefault();
                                if (cn != null)
                                {
                                    cn.Modified_By = userId;
                                    cn.Modified_Date = DateTime.Now;
                                    if (UpdateClaim.claimNotes != null)
                                    {
                                        if (UpdateClaim.claimNotes?.Scan_Date != null)
                                        {
                                            cn.Scan_Date = UpdateClaim.claimNotes.Scan_Date;
                                        }
                                        if (UpdateClaim.claimNotes?.Page_No != null)
                                        {
                                            cn.Page_No = UpdateClaim.claimNotes.Page_No;
                                        }
                                        if (UpdateClaim.claimNotes?.Scan_No != null)
                                        {
                                            cn.Scan_No = UpdateClaim.claimNotes.Scan_No;
                                        }
                                    }
                                    db.Entry(cn).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                var orignalDOS = originalclaim.DOS;
                                if (UpdateClaim.ClaimModel.DOS != null)
                                    originalclaim.DOS = UpdateClaim.ClaimModel.DOS;
                                //..below  code is commented by pir ubaid to resolve time zone issue bill date should not be updated.
                                //if (UpdateClaim.ClaimModel.Bill_Date != null)
                                //    originalclaim.Bill_Date = UpdateClaim.ClaimModel.Bill_Date;
                                if (UpdateClaim.ClaimModel.Location_Code != null)
                                    originalclaim.Location_Code = UpdateClaim.ClaimModel.Location_Code;
                                if (UpdateClaim.ClaimModel.Attending_Physician != null)
                                    originalclaim.Attending_Physician = UpdateClaim.ClaimModel.Attending_Physician;
                                //if (UpdateClaim.ClaimModel.Resource_Physician != null)
                                originalclaim.Resource_Physician = UpdateClaim.ClaimModel.Resource_Physician;
                                if (UpdateClaim.ClaimModel.Billing_Physician != null)
                                    originalclaim.Billing_Physician = UpdateClaim.ClaimModel.Billing_Physician;
                                originalclaim.Referring_Physician = UpdateClaim.ClaimModel.Referring_Physician;
                                originalclaim.Supervising_Physician = UpdateClaim.ClaimModel.Supervising_Physician;
                                originalclaim.Additional_Claim_Info = UpdateClaim.ClaimModel.Additional_Claim_Info;
                                originalclaim.Referral_Number = UpdateClaim.ClaimModel.Referral_Number;
                                originalclaim.PA_Number = UpdateClaim.ClaimModel.PA_Number;
                                originalclaim.Is_Self_Pay = UpdateClaim.ClaimModel.Is_Self_Pay;
                                originalclaim.Copay_Owed = UpdateClaim.ClaimModel.Copay_Owed;
                                originalclaim.Copay_Waived = UpdateClaim.ClaimModel.Copay_Waived;
                                originalclaim.Stop_Submission = UpdateClaim.ClaimModel.Stop_Submission;
                                originalclaim.Include_In_Sdf = UpdateClaim.ClaimModel.Include_In_Sdf;
                                originalclaim.Facility_Code = UpdateClaim.ClaimModel.Facility_Code;
                                originalclaim.Reference_Number = UpdateClaim.ClaimModel.Reference_Number;
                                if (UpdateClaim.ClaimModel.Hospital_From != null)
                                {
                                    originalclaim.Hospital_From = UpdateClaim.ClaimModel.Hospital_From;
                                }
                                else
                                {
                                    originalclaim.Hospital_From = null;
                                }
                                if (UpdateClaim.ClaimModel.Hospital_To != null)
                                {
                                    originalclaim.Hospital_To = UpdateClaim.ClaimModel.Hospital_To;
                                }
                                else
                                {
                                    originalclaim.Hospital_To = null;
                                }
                                originalclaim.Referral_Number = UpdateClaim.ClaimModel.Referral_Number;
                                originalclaim.Is_Draft = UpdateClaim.ClaimModel.Is_Draft;
                                originalclaim.PTL_Status = originalclaim.Is_Draft == true ? true : false;
                                //Self Pay Change
                                if (originalclaim.PTL_Status == true)
                                {
                                    originalclaim.Pri_Status = "";
                                    originalclaim.Sec_Status = "";
                                    originalclaim.Oth_Status = "";
                                    originalclaim.Pat_Status = "";
                                    originalclaim.Delay_Reason_Code = UpdateClaim.ClaimModel.Delay_Reason_Code;
                                }
                                //Added By Hamza Akhlaq For Inbox Delete API
                                if (UpdateClaim.ClaimModel.Pat_Status == "W")
                                {

                                    if (originalclaim.Pat_Status == "N" || originalclaim.Pat_Status == "R" ||
                                        originalclaim.Pat_Status == "B" || originalclaim.Pat_Status == "D")
                                    {
                                        var invoiceId = db.SyncedClaims
                                           .Where(x => x.Claim_no == UpdateClaim.ClaimModel.Claim_No)
                                           .FirstOrDefault()?.GeneratedId;
                                        if (invoiceId != null)
                                        {
                                            BaseResponse deleteResponse = DeleteInboxInvoice(invoiceId.Value);
                                            if (deleteResponse.IsSuccessful)
                                            {
                                                DeleteSyncedClaim(originalclaim.Claim_No);
                                            }

                                            if (syncedPractice != null)
                                            {
                                                PracticeSynchronization practiceSynchronization = db.PracticeSynchronizations.Where(x => x.PracticeId == syncedPractice.Practice_Code).FirstOrDefault();
                                                practiceSynchronization.UpdatedDate = DateTime.Now;
                                            }
                                        }
                                    }
                                }



                                else
                                {
                                    originalclaim.Pri_Status = UpdateClaim.ClaimModel.Pri_Status;
                                    originalclaim.Sec_Status = UpdateClaim.ClaimModel.Sec_Status;
                                    originalclaim.Oth_Status = UpdateClaim.ClaimModel.Oth_Status;
                                    originalclaim.Pat_Status = UpdateClaim.ClaimModel.Pat_Status;
                                    originalclaim.Delay_Reason_Code = UpdateClaim.ClaimModel.Delay_Reason_Code;
                                }
                                originalclaim.DX_Code1 = UpdateClaim.ClaimModel.DX_Code1;
                                originalclaim.DX_Code2 = UpdateClaim.ClaimModel.DX_Code2;
                                originalclaim.DX_Code3 = UpdateClaim.ClaimModel.DX_Code3;
                                originalclaim.DX_Code4 = UpdateClaim.ClaimModel.DX_Code4;
                                originalclaim.DX_Code5 = UpdateClaim.ClaimModel.DX_Code5;
                                originalclaim.DX_Code6 = UpdateClaim.ClaimModel.DX_Code6;
                                originalclaim.DX_Code7 = UpdateClaim.ClaimModel.DX_Code7;
                                originalclaim.DX_Code8 = UpdateClaim.ClaimModel.DX_Code8;
                                originalclaim.DX_Code9 = UpdateClaim.ClaimModel.DX_Code9;
                                originalclaim.DX_Code10 = UpdateClaim.ClaimModel.DX_Code10;
                                originalclaim.DX_Code11 = UpdateClaim.ClaimModel.DX_Code11;
                                originalclaim.DX_Code12 = UpdateClaim.ClaimModel.DX_Code12;
                                originalclaim.Claim_Status = UpdateClaim.ClaimModel.Claim_Status;
                                originalclaim.Injury_Date = UpdateClaim.ClaimModel.Injury_Date;
                                originalclaim.Accident_Auto = UpdateClaim.ClaimModel.Accident_Auto;
                                originalclaim.Accident_Other = UpdateClaim.ClaimModel.Accident_Other;
                                originalclaim.Employment = UpdateClaim.ClaimModel.Employment;
                                originalclaim.Accident_Emergency = UpdateClaim.ClaimModel.Accident_Emergency;
                                if (UpdateClaim.ClaimModel.Accident_Date != null)
                                {
                                    originalclaim.Accident_Date = UpdateClaim.ClaimModel.Accident_Date;
                                }
                                else
                                {
                                    originalclaim.Accident_Date = null;
                                }
                                originalclaim.Accident_State = UpdateClaim.ClaimModel.Accident_State;
                                originalclaim.Spinal_Manipulation_Xray_Availability = UpdateClaim.ClaimModel.Spinal_Manipulation_Xray_Availability;
                                if (UpdateClaim.ClaimModel.Manifestation_Date != null)
                                {
                                    originalclaim.Manifestation_Date = UpdateClaim.ClaimModel.Manifestation_Date;
                                }
                                else
                                {
                                    originalclaim.Manifestation_Date = null;
                                }
                                originalclaim.Spinal_Manipulation_Description = UpdateClaim.ClaimModel.Spinal_Manipulation_Description;
                                originalclaim.Spinal_Manipulation_Condition_Code = UpdateClaim.ClaimModel.Spinal_Manipulation_Condition_Code;
                                originalclaim.Phy_Exam_Code = UpdateClaim.ClaimModel.Phy_Exam_Code;
                                originalclaim.Phy_Exam_Desc = UpdateClaim.ClaimModel.Phy_Exam_Desc;
                                originalclaim.Institution_Condition_Code = UpdateClaim.ClaimModel.Institution_Condition_Code;
                                originalclaim.Transportation_Condition_Code = UpdateClaim.ClaimModel.Transportation_Condition_Code;
                                originalclaim.ServiceAuthExceptionCode = UpdateClaim.ClaimModel.ServiceAuthExceptionCode;
                                originalclaim.SpecialProgramCode = UpdateClaim.ClaimModel.SpecialProgramCode;
                                originalclaim.Weight = UpdateClaim.ClaimModel.Weight;
                                originalclaim.Transport_Distance = UpdateClaim.ClaimModel.Transport_Distance;
                                originalclaim.Transportation_Reason_Code = UpdateClaim.ClaimModel.Transportation_Reason_Code;
                                originalclaim.Transportation_Condition_Code = UpdateClaim.ClaimModel.Transportation_Condition_Code;
                                originalclaim.Condition_Indicator = UpdateClaim.ClaimModel.Condition_Indicator;
                                originalclaim.Transport_Code = UpdateClaim.ClaimModel.Transport_Code;
                                if (UpdateClaim.ClaimModel.Ordering_Physician != null)
                                {
                                    originalclaim.Ordering_Physician = UpdateClaim.ClaimModel.Ordering_Physician;
                                }
                                else
                                {
                                    originalclaim.Ordering_Physician = null;
                                }
                                if (UpdateClaim.ClaimModel.Attach_Type_Code != null)
                                {
                                    originalclaim.Attach_Type_Code = UpdateClaim.ClaimModel.Attach_Type_Code;
                                }
                                else
                                {
                                    originalclaim.Attach_Type_Code = null;
                                }
                                if (UpdateClaim.ClaimModel.Last_Seen_Physician != null)
                                {
                                    originalclaim.Last_Seen_Physician = UpdateClaim.ClaimModel.Last_Seen_Physician;
                                }
                                else
                                {
                                    originalclaim.Last_Seen_Physician = null;
                                }
                                if (UpdateClaim.ClaimModel.Ordering_Physician != null)
                                {
                                    originalclaim.Ordering_Physician = UpdateClaim.ClaimModel.Ordering_Physician;
                                }
                                else
                                {
                                    originalclaim.Ordering_Physician = null;
                                }
                                if (UpdateClaim.ClaimModel.Last_Seen_Date != null)
                                {
                                    originalclaim.Last_Seen_Date = UpdateClaim.ClaimModel.Last_Seen_Date;
                                }
                                else
                                {
                                    originalclaim.Last_Seen_Date = null;
                                }
                                if (UpdateClaim.ClaimModel.Plan_Code != null)
                                {
                                    originalclaim.Plan_Code = UpdateClaim.ClaimModel.Plan_Code;
                                }
                                else
                                {
                                    originalclaim.Plan_Code = null;
                                }
                                if (UpdateClaim.ClaimModel.Start_Care_Date != null)
                                {
                                    originalclaim.Start_Care_Date = UpdateClaim.ClaimModel.Start_Care_Date;
                                }
                                else
                                {
                                    originalclaim.Start_Care_Date = null;
                                }
                                if (UpdateClaim.ClaimModel.ASSUMED_CARE_DATE != null)
                                {
                                    originalclaim.ASSUMED_CARE_DATE = UpdateClaim.ClaimModel.ASSUMED_CARE_DATE;
                                }
                                else
                                {
                                    originalclaim.ASSUMED_CARE_DATE = null;
                                }
                                if (UpdateClaim.ClaimModel.LMP_Date != null)
                                {
                                    originalclaim.LMP_Date = UpdateClaim.ClaimModel.LMP_Date;
                                }
                                else
                                {
                                    originalclaim.LMP_Date = null;
                                }
                                if (UpdateClaim.ClaimModel.X_Ray_Date != null)
                                {
                                    originalclaim.X_Ray_Date = UpdateClaim.ClaimModel.X_Ray_Date;
                                }
                                else
                                {
                                    originalclaim.X_Ray_Date = null;
                                }
                                if (UpdateClaim.ClaimModel.Last_Work_Date != null)
                                {
                                    originalclaim.Last_Work_Date = UpdateClaim.ClaimModel.Last_Work_Date;
                                }
                                else
                                {
                                    originalclaim.Last_Work_Date = null;
                                }
                                if (UpdateClaim.ClaimModel.EDC_date != null)
                                {
                                    originalclaim.EDC_date = UpdateClaim.ClaimModel.EDC_date;
                                }
                                else
                                {
                                    originalclaim.EDC_date = null;
                                }
                                if (UpdateClaim.ClaimModel.RELINQISHED_CARE_DATE != null)
                                {
                                    originalclaim.RELINQISHED_CARE_DATE = UpdateClaim.ClaimModel.RELINQISHED_CARE_DATE;
                                }
                                else
                                {
                                    originalclaim.RELINQISHED_CARE_DATE = null;
                                }
                                if (UpdateClaim.ClaimModel.AA != null)
                                    originalclaim.AA = Convert.ToByte(UpdateClaim.ClaimModel.AA);
                                if (UpdateClaim.ClaimModel.Current_Illness_Date != null)
                                {
                                    originalclaim.Current_Illness_Date = UpdateClaim.ClaimModel.Current_Illness_Date;
                                }
                                else
                                {
                                    originalclaim.Current_Illness_Date = null;
                                }
                                if (UpdateClaim.ClaimModel.TCM_Cal_Dos != null)
                                {
                                    originalclaim.TCM_Cal_Dos = UpdateClaim.ClaimModel.TCM_Cal_Dos;
                                }
                                else
                                {
                                    originalclaim.TCM_Cal_Dos = null;
                                }
                                originalclaim.BLOCK1213 = UpdateClaim.ClaimModel.BLOCK1213;
                                originalclaim.HCFA_Note = UpdateClaim.ClaimModel.HCFA_Note;
                                originalclaim.Modified_By = userId;
                                originalclaim.Modified_Date = DateTime.Now;
                                if (UpdateClaim.claimPayments != null)
                                {
                                    originalclaim.Pri_Ins_Payment = 0;
                                    originalclaim.Sec_Ins_Payment = 0;
                                    originalclaim.Oth_Ins_Payment = 0;
                                    originalclaim.Patient_Payment = 0;
                                    originalclaim.Amt_Paid = 0;
                                    originalclaim.Adjustment = 0;
                                    originalclaim.Claim_Total = 0;
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
                                            if (UpdateClaim.claimPayments[i].claimPayments.Payment_Source == "P" || UpdateClaim.claimPayments[i].claimPayments.Payment_Source == "C" || UpdateClaim.claimPayments[i].claimPayments.Payment_Source == "I" || UpdateClaim.claimPayments[i].claimPayments.Payment_Source == "T")
                                                originalclaim.Patient_Payment += UpdateClaim.claimPayments[i].claimPayments.Amount_Paid != null ? UpdateClaim.claimPayments[i].claimPayments.Amount_Paid : 0;
                                            originalclaim.Amt_Paid += UpdateClaim.claimPayments[i].claimPayments.Amount_Paid != null ? UpdateClaim.claimPayments[i].claimPayments.Amount_Paid : 0;
                                            if (UpdateClaim.claimPayments[i].claimPayments.Amount_Adjusted != null)
                                                originalclaim.Adjustment += UpdateClaim.claimPayments[i].claimPayments.Amount_Adjusted;// != null ? decimal.Parse(UpdateClaim.claimPayments[i].AmountAdjusted) : 0;
                                        }
                                    }
                                }
                                if (UpdateClaim.claimCharges != null)
                                {
                                    originalclaim.Claim_Total = 0;
                                    for (int i = 0; i < UpdateClaim.claimCharges.Count(); i++)
                                    {
                                        bool isDeleted = (UpdateClaim.claimCharges[i].claimCharges.Deleted != null ? (bool)UpdateClaim.claimCharges[i].claimCharges.Deleted : false);
                                        bool IsRectify = (UpdateClaim.claimCharges[i].claimCharges.IsRectify != null ? (bool)UpdateClaim.claimCharges[i].claimCharges.IsRectify : false);
                                        if (!isDeleted && !IsRectify)
                                        {
                                            originalclaim.Claim_Total += UpdateClaim.claimCharges[i].claimCharges.Amount != null ? UpdateClaim.claimCharges[i].claimCharges.Amount : 0;
                                        }
                                    }
                                    originalclaim.Amt_Due = Convert.ToDecimal(originalclaim.Claim_Total) - (Convert.ToDecimal(originalclaim.Amt_Paid) + Convert.ToDecimal(originalclaim.Adjustment));
                                }
                                if (originalclaim.Amt_Due > 0 && originalclaim.PTL_Status != true)
                                {
                                    originalclaim.Pri_Status = UpdateClaim.ClaimModel.Pri_Status;
                                    originalclaim.Sec_Status = UpdateClaim.ClaimModel.Sec_Status;
                                    originalclaim.Oth_Status = UpdateClaim.ClaimModel.Oth_Status;
                                    originalclaim.Pat_Status = UpdateClaim.ClaimModel.Pat_Status;
                                }
                                else if (originalclaim.Amt_Due <= 0 && originalclaim.PTL_Status != true)
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
                                originalclaim.Add_CLIA_Number = UpdateClaim.ClaimModel.Add_CLIA_Number;
                                originalclaim.Additional_Estatement = UpdateClaim.ClaimModel.Additional_Estatement;
                                originalclaim.Patient_Statement = UpdateClaim.ClaimModel.Patient_Statement;
                                bool iCLIA = false;
                                string getClia = "select clia_number from practice_locations where practice_code ='" + UpdateClaim.PracticeCode + "' and Location_Code = '" + UpdateClaim.ClaimModel.Location_Code + "'";
                                string clia = db.Database.SqlQuery<string>(getClia).FirstOrDefault();
                                if (!string.IsNullOrEmpty(clia))
                                {
                                    if (UpdateClaim.claimCharges != null)
                                    {
                                        foreach (var cClaimChargesItes in UpdateClaim.claimCharges)
                                        {
                                            var item = cClaimChargesItes.claimCharges;
                                            if (item.Deleted != null && item.IsRectify != null && ((bool)item.Deleted == true || (bool)item.IsRectify))
                                                continue;
                                            if (!iCLIA)
                                            {
                                                var isCLIA = (from y in db.Procedures.Where(y => y.ProcedureCode == item.Procedure_Code) select y.clia_number).FirstOrDefault();
                                                if (isCLIA == true && item.Include_In_Edi == true)
                                                {
                                                    originalclaim.Add_CLIA_Number = true;
                                                    iCLIA = true;
                                                    break;
                                                }
                                            }
                                            else
                                                break;
                                        }
                                    }
                                }
                                if (iCLIA != true)
                                    originalclaim.Add_CLIA_Number = false;
                                originalclaim.Created_From = UpdateClaim.ClaimModel.Created_From;
                                db.SaveChanges();
                                //Check if the claim's DOS was updated and that the claim synced with InboxHealth
                                if (orignalDOS?.ToString("MM/dd/yyyy") != UpdateClaim.ClaimModel.DOS?.ToString("MM/dd/yyyy"))
                                {
                                    SyncedClaim syncedClaim = db.SyncedClaims.Where(c => c.Claim_no == originalclaim.Claim_No).FirstOrDefault<SyncedClaim>();
                                    if (syncedClaim != null)
                                    {
                                        syncedClaim.UpdatedDate = DateTime.Now;
                                        db.SaveChanges();
                                        //Update the Patient in Inbox Health if the Practice is Synced 
                                        UpdateSyncedClaim(syncedClaim, db);
                                    }
                                }
                            }
                            #endregion claim
                            #region claimCharges
                            List<Claim_Charges> allClaimCharges = new List<Claim_Charges>();
                            //.. Below 2 lines are commented to hold inbox sync optimzation change done by Hamza Akhlaq
                            //string chargesque = "UPDATE syncedclaims SET IsSynced = 0 WHERE Claim_no = @id  ";
                            //var chargesres= db.Database.ExecuteSqlCommand(chargesque, new SqlParameter("@id", UpdateClaim.ClaimModel.Claim_No));
                            if (UpdateClaim.claimCharges != null)
                            {
                                foreach (var cChargesItem in UpdateClaim.claimCharges)
                                {
                                    var item = cChargesItem.claimCharges;
                                    if (item.claim_charges_id == 0 && !(bool)item.Deleted && item.Procedure_Code != "" && item.Procedure_Code != null)
                                    {
                                        Claim_Charges cc = new Claim_Charges();
                                        var iClaimChargesId = Convert.ToInt64(db.SP_TableIdGenerator("Claim_Charges_Id").FirstOrDefault().ToString());//(from p in db.GetMaxColumnID("Claim_Charges_Id") select p).SingleOrDefault();
                                        cc.DOE = DateTime.Now;
                                        cc.claim_charges_id = iClaimChargesId;
                                        cc.Claim_No = UpdateClaim.ClaimModel.Claim_No;
                                        cc.Procedure_Code = item.Procedure_Code;
                                        cc.Alternate_Code = item.Alternate_Code;
                                        cc.Sequence_No = item.Sequence_No;
                                        if (item.POS != null)
                                            cc.POS = item.POS;
                                        cc.Created_By = userId;
                                        cc.Created_Date = DateTime.Now;
                                        if (!string.IsNullOrEmpty(item.Units.Value.ToString()))
                                            cc.Units = item.Units;
                                        if (item.Amount != null)
                                            cc.Amount = item.Amount;
                                        if (item.Dos_From != null)
                                            cc.Dos_From = item.Dos_From;
                                        if (item.Dos_To != null)
                                            cc.Dos_To = item.Dos_To;
                                        if (item.Start_Time != null)
                                            cc.Start_Time = item.Start_Time;

                                        if (item.Stop_Time != null)
                                            cc.Stop_Time = item.Stop_Time;
                                        cc.DX_Pointer1 = item.DX_Pointer1;
                                        cc.DX_Pointer2 = item.DX_Pointer2;
                                        cc.DX_Pointer3 = item.DX_Pointer3;
                                        cc.DX_Pointer4 = item.DX_Pointer4;
                                        cc.Modi_Code1 = item.Modi_Code1;
                                        cc.Modi_Code2 = item.Modi_Code2;
                                        cc.Modi_Code3 = item.Modi_Code3;
                                        cc.Modi_Code4 = item.Modi_Code4;
                                        cc.Include_In_Edi = item.Include_In_Edi;
                                        cc.IsRectify = item.IsRectify;
                                        if (item.NDC_Quantity != null)
                                            cc.NDC_Quantity = item.NDC_Quantity;
                                        cc.Drug_Code = (cChargesItem.Drug_Code)?.Replace("-", "");
                                        cc.Cpt_Type = "CPT";
                                        cc.NDC_Qualifier = item.NDC_Qualifier;
                                        if (item.Contractual_Amt != null)
                                            cc.Contractual_Amt = item.Contractual_Amt;
                                        cc.Include_In_Sdf = item.Include_In_Sdf;
                                        cc.Accession_No = item.Accession_No;
                                        cc.NDC_Service_Description = item.NDC_Service_Description;
                                        cc.Revenue_Code = item.Revenue_Code;
                                        cc.Weight = item.Weight;
                                        cc.Transport_Distance = item.Transport_Distance;
                                        cc.Transportation_Reason_Code = item.Transportation_Reason_Code;
                                        cc.Transportation_Condition_Code = item.Transportation_Condition_Code;
                                        cc.Transport_Code = item.Transport_Code;
                                        cc.Condition_Indicator = item.Condition_Indicator;
                                        cc.Emergency_Related = item.Emergency_Related;
                                        cc.Notes = item.Notes;
                                        cc.Physical_Modifier = item.Physical_Modifier;
                                        allClaimCharges.Add(cc);
                                        db.Claim_Charges.Add(cc);

                                        //Check if the claim practice is already synced with inbox the set the UpdatedDate column of PracticeSynchronization table
                                        //so that the next time the sync worker runs, it add this new patient, if it's due amount is > 0


                                        if (syncedPractice != null)
                                        {
                                            PracticeSynchronization practiceSynchronization = db.PracticeSynchronizations.Where(x => x.PracticeId == syncedPractice.Practice_Code).FirstOrDefault();
                                            practiceSynchronization.UpdatedDate = DateTime.Now;

                                            var syncedClaim = _deltaSyncRepository.GetSyncedclaim(UpdateClaim.ClaimModel.Claim_No);
                                            if (syncedClaim != null)
                                            {


                                                ClaimChargesCreateRequest ClaimChargesCreateRequest = new ClaimChargesCreateRequest();
                                                LineItemsAttribute claimChargescreateRequest = new LineItemsAttribute()
                                                {
                                                    invoice_id = syncedClaim.GeneratedId,
                                                    service_code = item.Procedure_Code,
                                                    description = (db.Procedures.Where(p => p.ProcedureCode == item.Procedure_Code).FirstOrDefault())?.ProcedureDescription,
                                                    date_of_service = item.Dos_From?.ToString("yyyy-MM-dd"),
                                                    total_charge_amount_cents = ((decimal)(item.Amount ?? 0) * 100) / (decimal)(item.Units ?? 1),
                                                    covered_amount_cents = 0,
                                                    insurance_owed_amount_cents = 0,
                                                    quantity = (int)item.Units
                                                };

                                                createSyncedClaimCharge(claimChargescreateRequest, syncedClaim.Claim_no, iClaimChargesId, db);
                                            }

                                        }
                                    }
                                    else
                                    {
                                        if (item.claim_charges_id == 0)
                                            continue;
                                        var cc = db.Claim_Charges.Find(item.claim_charges_id);
                                        if (item.Deleted != null && (bool)item.Deleted)
                                        {
                                            cc.Deleted = true;

                                        }
                                        cc.Procedure_Code = item.Procedure_Code;
                                        cc.Alternate_Code = item.Alternate_Code;
                                        cc.Sequence_No = item.Sequence_No;
                                        if (item.POS != null)
                                            cc.POS = item.POS;
                                        cc.Modified_By = userId;
                                        cc.Modified_Date = DateTime.Now;
                                        cc.Units = item.Units;
                                        if (item.Amount != null)
                                            cc.Amount = item.Amount;
                                        if (item.Dos_From != null)
                                            cc.Dos_From = item.Dos_From;
                                        if (item.Dos_To != null)
                                            cc.Dos_To = item.Dos_To;

                                        if (item.Start_Time != null)
                                            cc.Start_Time = item.Start_Time;

                                        if (item.Stop_Time != null)
                                            cc.Stop_Time = item.Stop_Time;

                                        cc.DX_Pointer1 = item.DX_Pointer1;
                                        cc.DX_Pointer2 = item.DX_Pointer2;
                                        cc.DX_Pointer3 = item.DX_Pointer3;
                                        cc.DX_Pointer4 = item.DX_Pointer4;
                                        cc.Modi_Code1 = item.Modi_Code1;
                                        cc.Modi_Code2 = item.Modi_Code2;
                                        cc.Modi_Code3 = item.Modi_Code3;
                                        cc.Modi_Code4 = item.Modi_Code4;
                                        cc.Include_In_Edi = item.Include_In_Edi;
                                        cc.IsRectify = item.IsRectify;
                                        if (item.NDC_Quantity != null)
                                            cc.NDC_Quantity = item.NDC_Quantity;
                                        cc.Drug_Code = (cChargesItem.Drug_Code)?.Replace("-", "");
                                        cc.Cpt_Type = "CPT";
                                        cc.NDC_Qualifier = item.NDC_Qualifier;
                                        if (item.Contractual_Amt != null)
                                            cc.Contractual_Amt = item.Contractual_Amt;
                                        cc.Include_In_Sdf = item.Include_In_Sdf;
                                        cc.Accession_No = item.Accession_No;
                                        cc.NDC_Service_Description = item.NDC_Service_Description;
                                        cc.Revenue_Code = item.Revenue_Code;
                                        cc.Weight = item.Weight;
                                        cc.Transport_Distance = item.Transport_Distance;
                                        cc.Transportation_Reason_Code = item.Transportation_Reason_Code;
                                        cc.Transportation_Condition_Code = item.Transportation_Condition_Code;
                                        cc.Transport_Code = item.Transport_Code;
                                        cc.Condition_Indicator = item.Condition_Indicator;
                                        cc.Emergency_Related = item.Emergency_Related;
                                        cc.Notes = item.Notes;
                                        cc.Physical_Modifier = item.Physical_Modifier;
                                        allClaimCharges.Add(cc);
                                        db.SaveChanges();

                                        decimal? Covered = 0;
                                        decimal? ins_Covered = 0;
                                        //Check if the claim charge was synced with InboxHealth
                                        SyncedClaimCharge syncedClaimCharge = db.SyncedClaimCharges.Where(c => c.claim_charges_id == item.claim_charges_id && c.IsDeleted == false).FirstOrDefault<SyncedClaimCharge>();
                                        if (syncedClaimCharge != null)
                                        {
                                            if (UpdateClaim.claimPayments != null)
                                            {
                                                foreach (var Payment in UpdateClaim.claimPayments)
                                                {
                                                    if (Payment.claimPayments.Charged_Proc_Code != null && cc.Procedure_Code != null && (Payment.claimPayments.Payment_Source != "P" && Payment.claimPayments.Payment_Source != "I"
                                                        && Payment.claimPayments.Payment_Source != "C" && Payment.claimPayments.Payment_Source != "Q"))
                                                    {
                                                        if (cc.Procedure_Code.ToLower() == Payment.claimPayments.Charged_Proc_Code.ToLower())
                                                        {
                                                            if (Payment.claimPayments.Amount_Adjusted != null)
                                                            {
                                                                Covered = Covered + Payment.claimPayments.Amount_Adjusted;
                                                            }
                                                            if (Payment.claimPayments.Amount_Paid != null)
                                                            {
                                                                ins_Covered = ins_Covered + Payment.claimPayments.Amount_Paid;
                                                            }
                                                        }

                                                    }

                                                }
                                            }


                                            syncedClaimCharge.UpdatedDate = DateTime.Now;
                                            db.SaveChanges();
                                            //Update the Patient in Inbox Health if the Practice is Synced 
                                            var invoiceId = db.SyncedClaims.Where(c => c.Claim_no == item.Claim_No).FirstOrDefault()?.GeneratedId;
                                            // Added By Hamza Akhlaq For D
                                            if (invoiceId != null)
                                            {
                                                UpdateSyncedClaimCharge(syncedClaimCharge, (long)invoiceId, Covered, ins_Covered, db);

                                            }

                                            if (item.Deleted != null && (bool)item.Deleted)
                                            {
                                                syncedClaimCharge.IsDeleted = true;
                                                db.SaveChanges();
                                                deleteSyncedClaimCharge(syncedClaimCharge, (long)cc.Claim_No, db);
                                            }

                                        }
                                    }
                                }
                                originalclaim.Pos = GetClaimPOS(allClaimCharges);
                                db.SaveChanges();
                            }
                            #endregion claimCharges
                            #region claimPayments
                            if (UpdateClaim.claimPayments != null)
                            {
                                //.. Below 2 lines are commented to hold inbox sync optimzation change done by Hamza Akhlaq
                                //string paymnetque = "UPDATE syncedclaims SET IsSynced = 0 WHERE Claim_no = @id  ";
                                //var paymentres = db.Database.ExecuteSqlCommand(paymnetque, new SqlParameter("@id", UpdateClaim.ClaimModel.Claim_No));

                                foreach (var cPaymentItem in UpdateClaim.claimPayments)
                                {
                                    //Added By Hamza Akhlaq For Delete API of Inbox For Amount paid is less then 0
                                    var item = cPaymentItem.claimPayments;
                                    var existingclaimpayments = db.Claim_Payments.Where(p => p.Claim_No == UpdateClaim.ClaimModel.Claim_No && p.Deleted != true).ToList();
                                    for (int i = 0; i < existingclaimpayments.Count(); i++)
                                    {
                                        if ((existingclaimpayments[i].claim_payments_id != item.claim_payments_id && item.claim_payments_id != 0))
                                        {
                                            // Added BY Hamza Akhlaq For hanlde Rreversal Payments Here 
                                            if (item.Amount_Paid < 0)

                                            {
                                                var invoiceId = db.SyncedClaims
                                            .Where(x => x.Claim_no == UpdateClaim.ClaimModel.Claim_No)
                                            .FirstOrDefault()?.GeneratedId;
                                                if (invoiceId != null)
                                                {
                                                    BaseResponse deleteResponse = DeleteInboxInvoice(invoiceId.Value);
                                                    if (deleteResponse.IsSuccessful)
                                                    {
                                                        var query = "DELETE FROM SyncedClaims WHERE Claim_no = @ClaimNumber";
                                                        var result = db.Database.ExecuteSqlCommand(query, new SqlParameter("@ClaimNumber", UpdateClaim.ClaimModel.Claim_No));
                                                        if (result >= 0)
                                                        {
                                                            // Deleting from syncedclaimcharges
                                                            var claimCharges = db.Claim_Charges.Where(x => x.Claim_No == UpdateClaim.ClaimModel.Claim_No).ToList();

                                                            if (claimCharges != null && claimCharges.Count > 0)
                                                            {
                                                                foreach (var charge in claimCharges)
                                                                {
                                                                    if (charge.claim_charges_id != null)
                                                                    {
                                                                        var q = "DELETE FROM syncedclaimcharges WHERE claim_charges_id = @id";
                                                                        db.Database.ExecuteSqlCommand(q, new SqlParameter("@id", charge.claim_charges_id));
                                                                    }
                                                                }
                                                            }
                                                            // Deleting from syncedclaimpayments
                                                            var claimPayments = db.Claim_Payments.Where(x => x.Claim_No == UpdateClaim.ClaimModel.Claim_No).ToList();
                                                            foreach (var payment in claimPayments)
                                                            {
                                                                if (payment.claim_payments_id != null)
                                                                {
                                                                    var queryPayments = "DELETE FROM syncedclaimpayments WHERE claim_payments_id = @Payment_id";
                                                                    db.Database.ExecuteSqlCommand(queryPayments, new SqlParameter("@Payment_id", payment.claim_payments_id));
                                                                }
                                                            }
                                                        }
                                                    }

                                                    if (syncedPractice != null)
                                                    {
                                                        PracticeSynchronization practiceSynchronization = db.PracticeSynchronizations.Where(x => x.PracticeId == syncedPractice.Practice_Code).FirstOrDefault();
                                                        practiceSynchronization.UpdatedDate = DateTime.Now;
                                                    }
                                                }
                                            }

                                        }
                                    }


                                    SyncedClaimPayment syncedClaimPayment = db.SyncedClaimPayments.Where(c => c.claim_payments_id == item.claim_payments_id && c.IsDeleted == false).FirstOrDefault();
                                    if (syncedClaimPayment != null)
                                    {

                                        //var claimpaymentsforinbox = db.Claim_Payments.Where(p => p.Claim_No == UpdateClaim.ClaimModel.Claim_No && p.Deleted !=true).ToList();
                                        //Added By Hamza Akhlaq For Delete API of Inbox For Payment is deleted or updated
                                        if (UpdateClaim.ClaimModel.Pat_Status != "P")
                                        {
                                            for (int i = 0; i < existingclaimpayments.Count(); i++)
                                            {
                                                if ((existingclaimpayments[i].claim_payments_id == item.claim_payments_id && existingclaimpayments[i].Amount_Paid != item.Amount_Paid) || item.Deleted == true)
                                                {
                                                    var invoiceId = db.SyncedClaims
                                               .Where(x => x.Claim_no == UpdateClaim.ClaimModel.Claim_No)
                                               .FirstOrDefault()?.GeneratedId;
                                                    if (invoiceId != null)
                                                    {
                                                        BaseResponse deleteResponse = DeleteInboxInvoice(invoiceId.Value);
                                                        var query = "DELETE FROM SyncedClaims WHERE Claim_no = @ClaimNumber";
                                                        var result = db.Database.ExecuteSqlCommand(query, new SqlParameter("@ClaimNumber", UpdateClaim.ClaimModel.Claim_No));

                                                        if (result >= 0)
                                                        {
                                                            // Deleting from syncedclaimcharges
                                                            var claimCharges = db.Claim_Charges.Where(x => x.Claim_No == UpdateClaim.ClaimModel.Claim_No).ToList();

                                                            if (claimCharges != null && claimCharges.Count > 0)
                                                            {
                                                                foreach (var charge in claimCharges)
                                                                {
                                                                    if (charge.claim_charges_id != null)
                                                                    {
                                                                        var q = "DELETE FROM syncedclaimcharges WHERE claim_charges_id = @id";
                                                                        db.Database.ExecuteSqlCommand(q, new SqlParameter("@id", charge.claim_charges_id));
                                                                    }
                                                                }
                                                            }
                                                            // Deleting from syncedclaimpayments
                                                            var claimPayments = db.Claim_Payments.Where(x => x.Claim_No == UpdateClaim.ClaimModel.Claim_No).ToList();
                                                            foreach (var payment in claimPayments)
                                                            {
                                                                if (payment.claim_payments_id != null)
                                                                {
                                                                    var queryPayments = "DELETE FROM syncedclaimpayments WHERE claim_payments_id = @Payment_id";
                                                                    db.Database.ExecuteSqlCommand(queryPayments, new SqlParameter("@Payment_id", payment.claim_payments_id));
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                if (syncedPractice != null)
                                                {
                                                    PracticeSynchronization practiceSynchronization = db.PracticeSynchronizations.Where(x => x.PracticeId == syncedPractice.Practice_Code).FirstOrDefault();
                                                    practiceSynchronization.UpdatedDate = DateTime.Now;
                                                }


                                            }

                                        }

                                    }

                                    if (item.claim_payments_id == 0 && (item.Deleted != null && !(bool)item.Deleted))
                                    {
                                        Claim_Payments cp = new Claim_Payments();
                                        long iClaimPaymentsId = Convert.ToInt64(db.SP_TableIdGenerator("Claim_Payments_Id").FirstOrDefault().ToString());
                                        cp.claim_payments_id = iClaimPaymentsId;
                                        cp.Created_By = userId;
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
                                        //Uncommentted the below code by Pir Ubaid (REASON : this code was  commented and Dos_From value was not binding) 8 March,2024.
                                        cp.DOS_From = item.DOS_From;
                                        //Uncommentted the above code by Pir Ubaid (REASON : this code was  commented and Dos_From value was not binding) 8 March,2024.
                                        cp.Dos_To = item.Dos_To;
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
                                        cp.Claim_No = UpdateClaim.ClaimModel.Claim_No;
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
                                        cp.Alternate_Code=item.Alternate_Code;
                                        db.Claim_Payments.Add(cp);
                                        //Check if the claim practice is already synced with inbox then set the UpdatedDate column of PracticeSynchronization table
                                        //so that the next time the sync worker runs, it add this new patient, if it's due amount is > 0
                                        if (syncedPractice != null)
                                        {
                                            PracticeSynchronization practiceSynchronization = db.PracticeSynchronizations.Where(x => x.PracticeId == syncedPractice.Practice_Code).FirstOrDefault();
                                            practiceSynchronization.UpdatedDate = DateTime.Now;
                                        }
                                    }
                                    else
                                    {
                                        var cp = db.Claim_Payments.Find(item.claim_payments_id);
                                        if (cp != null)
                                        {
                                            if (item.Deleted != null && (bool)item.Deleted)
                                            {
                                                cp.Deleted = true;
                                                if (item.Payment_Source == "T")
                                                {
                                                    var transferpayment = db.Claim_Payments.FirstOrDefault(p => p.T_payment_id == cp.claim_payments_id);
                                                    if (transferpayment != null)
                                                    {
                                                        transferpayment.Deleted = true;
                                                        var targetclaim = db.Claims.FirstOrDefault(c => c.Claim_No == transferpayment.Claim_No);
                                                        targetclaim.Amt_Due = targetclaim.Amt_Due + transferpayment.Amount_Paid;
                                                        targetclaim.Amt_Paid = targetclaim.Amt_Paid - transferpayment.Amount_Paid;
                                                        targetclaim.Patient_Payment = targetclaim.Patient_Payment - transferpayment.Amount_Paid;
                                                        List<CLAIM_NOTES> cLAIM_NOTEs = new List<CLAIM_NOTES>();
                                                        cLAIM_NOTEs.Add(new CLAIM_NOTES
                                                        {
                                                            Claim_Notes_Id = Convert.ToInt64(db.SP_TableIdGenerator("Claim_Notes_Id").FirstOrDefault().ToString()),
                                                            Claim_No = cp.Claim_No,
                                                            IsAuto_Note = true,
                                                            Note_Detail = $"Transfer Credit Entry deleted from the claim and corresponding credit entry removed from the associated claim [{ transferpayment.Claim_No}]",
                                                            Created_By = userId,
                                                            Created_Date = DateTime.Now
                                                        });

                                                        cLAIM_NOTEs.Add(new CLAIM_NOTES
                                                        {
                                                            Claim_Notes_Id = Convert.ToInt64(db.SP_TableIdGenerator("Claim_Notes_Id").FirstOrDefault().ToString()),
                                                            Claim_No = transferpayment.Claim_No,
                                                            IsAuto_Note = true,
                                                            Note_Detail = $"Transfer Credit Balance entry removed from the claim due to deletion of the same entry from the source claim [{cp.Claim_No}]",
                                                            Created_By = userId,
                                                            Created_Date = DateTime.Now
                                                        });
                                                        db.CLAIM_NOTES.AddRange(cLAIM_NOTEs);

                                                    };

                                                }



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
                                            //Uncommentted the below code by Pir Ubaid (REASON : this code was  commented and Dos_From value was not binding) 8 March,2024.
                                            cp.DOS_From = item.DOS_From;
                                            //Uncommentted the above code by Pir Ubaid (REASON : this code was  commented and Dos_From value was not binding) 8 March,2024.
                                            cp.Dos_To = item.Dos_To;
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
                                            cp.Modified_By = userId;
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
                                        //Check if the claim payment was synced with InboxHealth
                                        SyncedClaimPayment syncedPayment = db.SyncedClaimPayments.Where(c => c.claim_payments_id == cp.claim_payments_id).FirstOrDefault();
                                        if (syncedPayment != null)
                                        {
                                            //DeleteSyncedClaim(item.Claim_No);
                                            syncedClaimPayment.UpdatedDate = DateTime.Now;
                                            //db.SaveChanges();
                                            //Update the Payment in Inbox Health if the Practice is Synced 
                                            UpdateSyncedClaimPayment(syncedClaimPayment, db);
                                        }
                                    }
                                    db.SaveChanges();
                                }

                                #endregion claimPayments
                                #region claimInsurance
                                if (UpdateClaim.claimInusrance != null)
                                {
                                    foreach (var claimItem in UpdateClaim.claimInusrance)
                                    {
                                        var item = claimItem.claimInsurance;
                                        if (item.Claim_Insurance_Id == 0 && !(bool)item.Deleted)
                                        {
                                            Claim_Insurance ci = new Claim_Insurance();
                                            long iClaimInsuranceId = Convert.ToInt64(db.SP_TableIdGenerator("Claim_Insurance_Id").FirstOrDefault().ToString());
                                            ci.Claim_Insurance_Id = iClaimInsuranceId;
                                            ci.Claim_No = UpdateClaim.ClaimModel.Claim_No;
                                            ci.Created_By = userId;
                                            ci.Created_Date = DateTime.Now;
                                            ci.Pri_Sec_Oth_Type = item.Pri_Sec_Oth_Type;
                                            if (item.Insurance_Id > 0)
                                                ci.Insurance_Id = item.Insurance_Id;
                                            if (item.Co_Payment != null)
                                                ci.Co_Payment = item.Co_Payment;
                                            if (item.Deductions != null)
                                                ci.Deductions = item.Deductions;
                                            ci.Co_Insurance = item.Co_Insurance;
                                            ci.Policy_Number = item.Policy_Number == null ? string.Empty : item.Policy_Number;
                                            ci.Pri_Sec_Oth_Type = item.Pri_Sec_Oth_Type;
                                            ci.Group_Number = item.Group_Number;
                                            ci.Group_Name = item.Group_Name;
                                            ci.Send_notes = item.Send_notes;
                                            ci.Patient_Account = cr.PatientAccount;
                                            if (item.Subscriber != 0)
                                                ci.Subscriber = item.Subscriber;
                                            if (item.Subscriber != null)
                                                ci.Subscriber = item.Subscriber;
                                            ci.Relationship = item.Relationship;
                                            if (item.MCR_SEC_Payer_Code != null)
                                                ci.MCR_SEC_Payer_Code = item.MCR_SEC_Payer_Code;
                                            if (item.Print_Center != null)
                                                ci.Print_Center = item.Print_Center;
                                            ci.Corrected_Claim = item.Corrected_Claim;
                                            ci.ICN = item.ICN;
                                            ci.Late_Filing = item.Late_Filing;
                                            ci.Late_Filing_Reason = item.Late_Filing_Reason;
                                            ci.Notes = item.Notes;
                                            ci.Send_Appeal = item.Send_Appeal;
                                            ci.Medical_Notes = item.Medical_Notes;
                                            ci.Reconsideration = item.Reconsideration;
                                            ci.Returned_Hcfa = item.Returned_Hcfa;
                                            if (item.Effective_Date != null)
                                                ci.Effective_Date = item.Effective_Date;
                                            if (item.Termination_Date != null)
                                                ci.Termination_Date = item.Termination_Date;
                                            if (!string.IsNullOrEmpty(ci.Plan_Name))
                                                ci.Plan_Name = item.Plan_Name.ToString();
                                            ci.Filing_Indicator = item.Filing_Indicator;
                                            ci.Access_Carolina_Number = item.Access_Carolina_Number;
                                            if (item.Is_Capitated_Claim != null)
                                                ci.Is_Capitated_Claim = bool.Parse(item.Is_Capitated_Claim.ToString());
                                            if (item.Allowed_Visits != null)
                                                ci.Allowed_Visits = item.Allowed_Visits;
                                            if (item.Remaining_Visits != null)
                                                ci.Remaining_Visits = item.Remaining_Visits;
                                            if (item.Termination_Date != null)
                                                ci.Visits_Start_Date = item.Termination_Date;
                                            if (item.Visits_End_Date != null)
                                                ci.Visits_End_Date = item.Visits_End_Date;
                                            ci.Admission_Type_Code = item.Admission_Type_Code;
                                            ci.Admission_Source_Code = item.Admission_Source_Code;
                                            ci.Patient_Status_Code = item.Patient_Status_Code;
                                            if (item.Claim_Status_Request != null)
                                                ci.Claim_Status_Request = item.Claim_Status_Request;
                                            ci.Co_Payment_Per = item.Co_Payment_Per;

                                            db.Claim_Insurance.Add(ci);
                                        }
                                        else
                                        {
                                            if (item.Claim_Insurance_Id != 0)
                                            {
                                                long InsuranceId = item.Insurance_Id;
                                                var ci = (from x in db.Claim_Insurance
                                                          where x.Claim_Insurance_Id == item.Claim_Insurance_Id &&
                                                           (x.Deleted ?? false) == false
                                                          select x).FirstOrDefault();
                                                if (ci != null)
                                                {
                                                    ci.Deleted = item.Deleted;
                                                    ci.Modified_By = userId;
                                                    ci.Modified_Date = DateTime.Now;
                                                    ci.Insurance_Id = item.Insurance_Id;
                                                    if (ci.Insurance_Id != item.Insurance_Id)
                                                    {
                                                        //RemoveImageAttachmet(UpdateClaim.claimNo, ci.Pri_Sec_Oth_Type);
                                                    }
                                                    ci.Pri_Sec_Oth_Type = item.Pri_Sec_Oth_Type;
                                                    if (item.Co_Payment != null)
                                                        ci.Co_Payment = item.Co_Payment;
                                                    if (item.Deductions != null)
                                                        ci.Deductions = item.Deductions;
                                                    ci.Co_Insurance = item.Co_Insurance;
                                                    ci.Policy_Number = item.Policy_Number == null ? string.Empty : item.Policy_Number;
                                                    ci.Pri_Sec_Oth_Type = item.Pri_Sec_Oth_Type;
                                                    ci.Send_notes = item.Send_notes;
                                                    if (ci.Insurance_Id != item.Insurance_Id)
                                                        isInsuranceChanged = true;
                                                    ci.Group_Number = item.Group_Number;
                                                    ci.Group_Name = item.Group_Name;
                                                    if (item.Subscriber != 0)
                                                        ci.Subscriber = item.Subscriber;
                                                    else
                                                        ci.Subscriber = null;
                                                    ci.Relationship = item.Relationship;
                                                    if (item.MCR_SEC_Payer_Code != null)
                                                        ci.MCR_SEC_Payer_Code = item.MCR_SEC_Payer_Code;
                                                    if (item.Print_Center != null)
                                                        ci.Print_Center = item.Print_Center;
                                                    ci.Corrected_Claim = item.Corrected_Claim;
                                                    ci.ICN = item.ICN;
                                                    ci.Late_Filing = item.Late_Filing;
                                                    ci.Late_Filing_Reason = item.Late_Filing_Reason;
                                                    ci.Notes = item.Notes;
                                                    ci.Patient_Account = cr.PatientAccount;
                                                    ci.Send_Appeal = item.Send_Appeal;
                                                    ci.Medical_Notes = item.Medical_Notes;
                                                    ci.Reconsideration = item.Reconsideration;
                                                    ci.Returned_Hcfa = item.Returned_Hcfa;
                                                    if (item.Effective_Date != null)
                                                        ci.Effective_Date = item.Effective_Date;
                                                    if (item.Termination_Date != null)
                                                        ci.Termination_Date = item.Termination_Date;
                                                    if (!string.IsNullOrEmpty(ci.Plan_Name))
                                                        ci.Plan_Name = item.Plan_Name.ToString();
                                                    ci.Filing_Indicator = item.Filing_Indicator;
                                                    ci.Access_Carolina_Number = item.Access_Carolina_Number;
                                                    if (item.Is_Capitated_Claim != null)
                                                        ci.Is_Capitated_Claim = bool.Parse(item.Is_Capitated_Claim.ToString());
                                                    if (item.Allowed_Visits != null)
                                                        ci.Allowed_Visits = item.Allowed_Visits;
                                                    if (item.Remaining_Visits != null)
                                                        ci.Remaining_Visits = item.Remaining_Visits;
                                                    if (item.Termination_Date != null)
                                                        ci.Visits_Start_Date = item.Termination_Date;
                                                    if (item.Visits_End_Date != null)
                                                        ci.Visits_End_Date = item.Visits_End_Date;
                                                    ci.Admission_Type_Code = item.Admission_Type_Code;
                                                    ci.Admission_Source_Code = item.Admission_Source_Code;
                                                    ci.Patient_Status_Code = item.Patient_Status_Code;
                                                    if (item.Claim_Status_Request != null)
                                                        ci.Claim_Status_Request = item.Claim_Status_Request;
                                                    ci.Co_Payment_Per = item.Co_Payment_Per;
                                                }
                                            }
                                        }
                                        db.SaveChanges();
                                        if (isInsuranceChanged)
                                        {
                                        }
                                    }
                                }
                                #endregion claimInsurance
                                db.Database.CurrentTransaction.Commit();
                                objResponse.Status = "Sucess";
                            }
                        }

                        catch (System.Data.Entity.Validation.DbEntityValidationException)
                        {
                            db.Database.CurrentTransaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            #endregion


            if (cr.ClaimModel.Oth_Status == null ||
                cr.ClaimModel.Pri_Status == null ||
                cr.ClaimModel.Sec_Status == null ||
                cr.ClaimModel.Pat_Status == null ||
                cr.ClaimModel.Oth_Status.ToUpper() == "N" ||
                cr.ClaimModel.Pri_Status.ToUpper() == "N" ||
                cr.ClaimModel.Sec_Status.ToUpper() == "N" ||
                cr.ClaimModel.Pat_Status.ToUpper() == "N" ||
                cr.ClaimModel.Oth_Status.ToUpper() == "R" ||
                cr.ClaimModel.Pri_Status.ToUpper() == "R" ||
                cr.ClaimModel.Sec_Status.ToUpper() == "R" ||
                cr.ClaimModel.Pat_Status.ToUpper() == "R"
                )
            {

                var res = _scrubberService.AddToScrubberQueue(cr);
            }


            return objResponse;
        }

        private void UpdateSyncedClaimPayment(SyncedClaimPayment syncedClaimPayment, NPMDBEntities db)
        {
            ClaimPaymentUpdateRequest claimPaymentUpdateRequest;
            try
            {
                var claimP = db.Claim_Payments.Where(cp => cp.claim_payments_id == syncedClaimPayment.claim_payments_id).FirstOrDefault();
                var claim = db.Claims.Where(cp => cp.Claim_No == claimP.Claim_No).FirstOrDefault();
                var payment_type = CheckAmountDue(claim.Pri_Ins_Payment.HasValue ? (decimal)claim.Pri_Ins_Payment : 0,
                                                  claim.Sec_Ins_Payment.HasValue ? (decimal)claim.Sec_Ins_Payment : 0,
                                                  claim.Oth_Ins_Payment.HasValue ? (decimal)claim.Oth_Ins_Payment : 0,
                                                  claim.Patient_Payment.HasValue ? (decimal)claim.Patient_Payment : 0);
                claimPaymentUpdateRequest = new ClaimPaymentUpdateRequest()
                {
                    id = syncedClaimPayment.GeneratedId,
                    payment = new Payment
                    {
                        expected_amount_cents = (decimal)((claimP.Amount_Paid ?? 0) * 100),
                        payment_method_type = payment_type
                    }
                };
                var update = _deltaSyncRepository.UpdateClaimPayment(claimPaymentUpdateRequest);
                var practiceSyncId = db.PracticeSynchronizations.Where(x => x.PracticeId == claim.practice_code).FirstOrDefault().PracticeSyncId;
                if (update.IsSuccessful)
                {
                    db.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                    {
                        PracticeSyncId = practiceSyncId,
                        LogMessage = "Claim payment with Id " + syncedClaimPayment.claim_payments_id + " has been successfully updated.",
                        LogTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
                else
                {
                    db.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                    {
                        PracticeSyncId = practiceSyncId,
                        LogMessage = "Claim payment with Id " + syncedClaimPayment.GeneratedId + " has failed to updated.",
                        LogTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string CheckAmountDue(decimal pri_Ins_Payment, decimal sec_Ins_Payment, decimal oth_Ins_Payment, decimal patient_Payment)
        {
            var sum = pri_Ins_Payment + sec_Ins_Payment + oth_Ins_Payment;
            if (sum > 0 && patient_Payment == 0)
                return "insurance";
            else
                return "external_card";
        }
        private void createSyncedClaimCharge(LineItemsAttribute syncedClaimCharge, long claimno, long ClaimChargesId, NPMDBEntities db)
        {

            try
            {

                ClaimChargesCreateRequest ClaimChargescreateRequest;

                ClaimChargescreateRequest = new ClaimChargesCreateRequest()
                {
                    line_item = syncedClaimCharge

                };


                var update = _deltaSyncRepository.CreateClaimCharge(ClaimChargescreateRequest);
                var pracCode = db.Claims.Where(c => c.Claim_No == claimno)?.Select(i => i.practice_code).FirstOrDefault();
                var practiceSyncId = db.PracticeSynchronizations.Where(x => x.PracticeId == pracCode).FirstOrDefault().PracticeSyncId;
                if (update.IsSuccessful)
                {
                    db.SyncedClaimCharges.Add(new SyncedClaimCharge()
                    {
                        claim_charges_id = ClaimChargesId,
                        GeneratedId = update.line_item.id,
                        CreatedBy = 100,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false,



                    }
                    );
                    db.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                    {
                        PracticeSyncId = practiceSyncId,
                        LogMessage = "Claim charge with Id " + update.line_item.id + " has been successfully created.",
                        LogTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
                else
                {
                    db.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                    {
                        PracticeSyncId = practiceSyncId,
                        LogMessage = "Claim charge with Id  has failed to created.",
                        LogTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void UpdateSyncedClaimCharge(SyncedClaimCharge syncedClaimCharge, long invoiceId, decimal? covered, decimal? ins_covered, NPMDBEntities db)
        {
            ClaimChargesUpdateRequest claimChargesUpdateRequest;
            try
            {
                var claimCharges = db.Claim_Charges.Where(c => c.claim_charges_id == syncedClaimCharge.claim_charges_id).FirstOrDefault();
                claimChargesUpdateRequest = new ClaimChargesUpdateRequest()
                {
                    id = syncedClaimCharge.GeneratedId,
                    line_item = new LineItemsAttribute()
                    {
                        invoice_id = invoiceId,
                        service_code = claimCharges.Procedure_Code,
                        description = (db.Procedures.Where(p => p.ProcedureCode == claimCharges.Procedure_Code).FirstOrDefault())?.ProcedureDescription,
                        date_of_service = claimCharges.Dos_From?.ToString("yyyy-MM-dd"),
                        total_charge_amount_cents = ((decimal)(claimCharges.Amount ?? 0) * 100) / (decimal)(claimCharges.Units ?? 1),
                        //Commented below one line by Hamza Ikhlaq and set insurance_owed_amount_cents for  inbox amount due fixation
                        //insurance_owed_amount_cents = ((decimal)(ins_covered ?? 0) * 100),
                        insurance_owed_amount_cents = ((decimal)(ins_covered ?? 0) * 100),
                        //insurance_balance_cents=0,
                        insurance_balance_cents = ((decimal)(ins_covered ?? 0) * 100),
                        covered_amount_cents = ((decimal)(covered ?? 0) * 100),
                        quantity = (int)claimCharges.Units
                    }

                };
                var update = _deltaSyncRepository.UpdateClaimCharge(claimChargesUpdateRequest);
                var pracCode = db.Claims.Where(c => c.Claim_No == claimCharges.Claim_No)?.Select(i => i.practice_code).FirstOrDefault();
                var practiceSyncId = db.PracticeSynchronizations.Where(x => x.PracticeId == pracCode).FirstOrDefault().PracticeSyncId;
                if (update.IsSuccessful)
                {
                    db.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                    {
                        PracticeSyncId = practiceSyncId,
                        LogMessage = "Claim charge with Id " + syncedClaimCharge.GeneratedId + " has been successfully updated.",
                        LogTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
                else
                {
                    db.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                    {
                        PracticeSyncId = practiceSyncId,
                        LogMessage = "Claim charge with Id " + syncedClaimCharge.GeneratedId + " has failed to updated.",
                        LogTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void deleteSyncedClaimCharge(SyncedClaimCharge syncedClaimCharge, long claimno, NPMDBEntities db)
        {
            ClaimChargesUpdateRequest claimChargesUpdateRequest;
            try
            {

                var update = _deltaSyncRepository.deleteClaimCharge(syncedClaimCharge.GeneratedId);
                var pracCode = db.Claims.Where(c => c.Claim_No == claimno)?.Select(i => i.practice_code).FirstOrDefault();
                var practiceSyncId = db.PracticeSynchronizations.Where(x => x.PracticeId == pracCode).FirstOrDefault().PracticeSyncId;
                if (update.IsSuccessful)
                {
                    db.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                    {
                        PracticeSyncId = practiceSyncId,
                        LogMessage = "Claim charge with Id " + syncedClaimCharge.GeneratedId + " has been successfully Deleted.",
                        LogTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
                else
                {
                    db.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                    {
                        PracticeSyncId = practiceSyncId,
                        LogMessage = "Claim charge with Id " + syncedClaimCharge.GeneratedId + " has failed to Deleted.",
                        LogTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void UpdateSyncedClaim(SyncedClaim syncedClaim, NPMDBEntities db)
        {
            ClaimUpdateRequest claimUpdateRequest;
            try
            {
                //using (var ctx = new NPMDBEntities())
                //{
                var claim = db.Claims.Where(c => c.Claim_No == syncedClaim.Claim_no).FirstOrDefault();
                claimUpdateRequest = new ClaimUpdateRequest()
                {
                    id = syncedClaim.GeneratedId,
                    invoice = new Invoice()
                    {
                        practice_id = db.SyncedPracticeLocations.Where(p => p.Location_Code == claim.Location_Code).FirstOrDefault().GeneratedId,
                        patient_id = db.SyncedPatients.Where(p => p.Patient_Account == claim.Patient_Account).FirstOrDefault().GeneratedId,
                        date_of_service = claim.DOS?.ToString("yyyy-MM-dd")
                    }
                };
                var update = _deltaSyncRepository.UpdateClaim(claimUpdateRequest);
                var practiceSyncId = db.PracticeSynchronizations.Where(x => x.PracticeId == claim.practice_code).FirstOrDefault().PracticeSyncId;
                if (update.IsSuccessful)
                {
                    db.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                    {
                        PracticeSyncId = practiceSyncId,
                        LogMessage = "Claim with Id " + syncedClaim.GeneratedId + " has been successfully updated.",
                        LogTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
                else
                {
                    db.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                    {
                        PracticeSyncId = practiceSyncId,
                        LogMessage = "Claim with Id " + syncedClaim.GeneratedId + " has failed to updated.",
                        LogTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
                //}
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool CanSaveClaim(ClaimsViewModel cr)
        {
            try
            {
                using (var db = new NPMDBEntities())
                {
                    var patient = db.Patients.Where(p => p.Patient_Account == cr.PatientAccount && !(p.Deleted ?? false)).Select(p => new { p.IsDeceased, p.DeathDate }).FirstOrDefault();
                    if (patient == null)
                    {
                        return false;
                    }
                    else
                    {
                        if (patient.IsDeceased == true)
                        {
                            return cr.ClaimModel.DOS <= patient.DeathDate;
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
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
        public ResponseModel GetProcedureCharges([FromBody] CPTWiseCharges obj)
        {
            ResponseModel objResponse = new ResponseModel();
            string strState = "";
            string strInsurance = "";
            string strIsParticipating = "False";
            string strIsFacility = "False";
            string strField = "";
            string strcode = "";
            string strPlan = "";
            double curPer = 0.0;
            string description = string.Empty;
            string alternateCode = string.Empty;
            Practice objPractice = null;
            using (var db = new NPMDBEntities())
            {
                //..added by Pir Ubaid Ullah
                if (obj.Alternate_Code != null && obj.Alternate_Code != "")
                {
                    var _checkCode = CheckCode(obj.Alternate_Code);
                    if (_checkCode != null && _checkCode.Response != null)
                    {
                        if (_checkCode.Response.Alternate_Code != null)
                        {
                            alternateCode = _checkCode.Response.Alternate_Code;
                        }
                        obj.ProcedureCode = _checkCode.Response.Procedure_Code;
                    }
                }
                else
                {
                    var _checkCode = CheckCode(obj.ProcedureCode);
                    if (_checkCode != null && _checkCode.Response != null)
                    {
                        if (_checkCode.Response.Alternate_Code != null)
                        {
                            alternateCode = _checkCode.Response.Alternate_Code;
                        }
                        obj.ProcedureCode = _checkCode.Response.Procedure_Code;
                    }
                }



                var pDescription = db.Procedures.FirstOrDefault(p => (p.ProcedureCode == obj.ProcedureCode && p.Alternate_Code == alternateCode));
                description = ((pDescription != null && pDescription.ProcedureDescription != null) ? pDescription.ProcedureDescription : "");
                long pracCode = Convert.ToInt64(obj.PracticeCode);
                objPractice = db.Practices.FirstOrDefault(p => p.Practice_Code == pracCode);
            }
            if (string.IsNullOrEmpty(obj.ProviderCode))
            {
                obj.ProviderCode = "0";
            }
            string locationCode = "0";
            if (!string.IsNullOrEmpty(obj.LocationCode))
            {
                locationCode = obj.LocationCode;
            }
            if (string.IsNullOrEmpty(obj.ModifierCode) || !string.IsNullOrEmpty(obj.ModifierCode))
            {
                List<string> resultModifier = GetPricingModifier(obj.ModifierCode);
                if (resultModifier.Count == 0)
                {
                    obj.ModifierCode = string.Empty;
                }
            }
            if (obj.FacilityCode == "undefined")
            {
                obj.FacilityCode = "";
            }
            List<GETEFSPRACTICES_Result> EFSPractice = GetEFSPractices(long.Parse(obj.PracticeCode));
            List<string> resultLocState = GetLocationState(locationCode);
            if (resultLocState.Count > 0)
            {
                var resultNew = (from x in resultLocState
                                 select x).FirstOrDefault();
                strState = resultNew.ToString();
            }
            string insPayerId = string.Empty;
            string insState = (objPractice.Prac_State != null ? objPractice.Prac_State : "");
            bool chkSelfPay;
            chkSelfPay = bool.Parse(obj.IsSelfPay == "" ? "false" : obj.IsSelfPay);
            if (!chkSelfPay)
            {
                GetPatientInsuranceInfo(obj.InsuranceID, out insPayerId, out insState); // to be done in javascript
                insState = obj.PracticeState.ToString();
                strInsurance = insPayerId == "0" ? "" : insPayerId;
                if (!strInsurance.Trim().Equals("")) //if patient have insurance
                {
                    strIsParticipating = "False";
                    if (EFSPractice.Count < 1)
                    {
                        string isParticipating = Is_Participating(long.Parse(insPayerId), obj.ProviderCode);
                        if (!string.IsNullOrEmpty(isParticipating))
                        {
                            var resultNew = (from x in isParticipating select x).FirstOrDefault();
                            strIsParticipating = isParticipating;
                        }
                    }
                    else
                    {
                        strIsParticipating = "True";
                    }
                }
            }
            if (chkSelfPay && strInsurance.Trim().Equals(""))
            {
                strIsParticipating = "True";
            }
            //strIsFacility = "True";   // Need to add Is Facility Column in Practice Table
            //Getting the Facility Criteria
            List<GET_ISFACILITY_Result> isFacility = IsFacility(long.Parse(obj.ProviderCode));
            if (isFacility.Count > 0)
            {
                var resultNew = (from x in isFacility select x).FirstOrDefault();
                strIsFacility = resultNew.IS_PRACTICE_FACILITY_BASED.ToString() == "" ? "False" : resultNew.IS_PRACTICE_FACILITY_BASED.ToString();
            }
            // Creating field for selection
            switch (strIsFacility)
            {
                case "True":
                    strField = "Facility_";
                    break;
                case "False":
                    strField = "Non_Facility_";
                    break;
            }
            //if patient have insurance
            if (strInsurance.Trim().Equals("") == false)
            {
                switch (strIsParticipating)
                {
                    case "True":
                        strField = strField + "Participating_Fee";
                        break;
                    case "False":
                        strField = strField + "Non_participating_Fee";
                        break;
                }
            }
            else
                strField = strField + "Non_participating_Fee";
            //Get standard cpt fee
            //var Charges ="";
            //strField = Charges;
            decimal? CPTStandardFee = GetStandardCptFee(strField, strState, obj.ProcedureCode, obj.ModifierCode,alternateCode);
            //For EFS practice system will directly pick the key from table as these practice have only two fee plan 
            if (EFSPractice.Count < 1)
            {
                long locationcode = long.Parse(obj.LocationCode);
                long facilityCode = 0;
                if (!string.IsNullOrEmpty(obj.FacilityCode))
                {
                    facilityCode = long.Parse(obj.FacilityCode);
                }
                strcode = GetCPTCode(obj.ProviderCode, strInsurance, strState, locationcode, obj.ProcedureCode, obj.ModifierCode, facilityCode, chkSelfPay ? true : false, obj.PracticeCode, alternateCode);
            }
            else
            {
                string strSelfPay = chkSelfPay ? "1" : "0";
                string strPractice = "";
                strPractice = obj.PracticeCode;
                string PlanID = GetProviderCPT_PlanID(obj.ProviderCode, strPractice, chkSelfPay, obj.ModifierCode, obj.ProcedureCode,alternateCode,obj.Dos_From, strInsurance, obj.FacilityCode,obj.LocationCode);
                var resultCPTID = PlanID;
                if (resultCPTID == null)
                {
                    strcode = "0";
                }
                else
                {
                    strcode = resultCPTID.ToString();
                }
            }
            if (strcode.Equals("0"))
            {
                if (CPTStandardFee == null || CPTStandardFee.ToString().Equals("") == true)
                {
                    objResponse.Response = "0.00" + "|" + description;
                }
                else
                    objResponse.Response = Convert.ToDouble(CPTStandardFee).ToString("0.00") + "|" + description;
            }


            List<SP_GetProviderCPTPlan_Result> ProviderCPTPlan = ProviderCPT_Plan(strcode);
            var Charges ="Charges";
            strField = Charges;
            if (ProviderCPTPlan.Count > 0)
            {
                var resultCPTPlan = (from x in ProviderCPTPlan select x).FirstOrDefault();
                strPlan = resultCPTPlan.CPT_PLAN.ToString();
            }
            if (strPlan.Trim().ToUpper().Equals("PRCNT"))
            {
                var resultCPTPlan = (from x in ProviderCPTPlan select x).FirstOrDefault();
                curPer = Convert.ToDouble(resultCPTPlan.PERCENTAGE_HIGHER.ToString());
                if (CPTStandardFee == null || CPTStandardFee.ToString().Equals("") == true)
                {
                    objResponse.Response = "0.00" + "|" + description;
                }
                else
                    objResponse.Response = (Convert.ToDouble(Convert.ToDouble(CPTStandardFee) + ((Convert.ToDouble(CPTStandardFee.ToString()) * curPer) / 100))).ToString("0.00") + "|" + description;
            }
            else if (strPlan.Trim().ToUpper().Equals("STAND"))
            {
                var resultCPTPlan = (from x in ProviderCPTPlan select x).FirstOrDefault();
                curPer = Convert.ToDouble(resultCPTPlan.PERCENTAGE_HIGHER.ToString());
                if (CPTStandardFee == null || CPTStandardFee.ToString().Equals("") == true)
                {
                    objResponse.Response = "0.00" + "|" + description;
                }
                else
                    objResponse.Response = Convert.ToDouble(CPTStandardFee).ToString("0.00") + "|" + description;
            }
            else if (strPlan.Trim().ToUpper().Equals("CUSTM"))
            {
                decimal? Plan_Detail = Plan_Details(strInsurance, strField, obj.ModifierCode, strcode, obj.ProcedureCode, alternateCode);
                if (Plan_Detail != null)
                {
                    if (Plan_Detail.ToString().Equals("") == true)
                    {
                        bool isModificationAllowed = CheckModificationAllowed(strcode.Trim());
                        if (isModificationAllowed)
                        {
                            if (CPTStandardFee == null || CPTStandardFee.ToString().Equals("") == true)
                            {
                                objResponse.Response = "0.00" + "|" + description;
                            }
                            else
                            {
                                objResponse.Response = Convert.ToDouble(CPTStandardFee).ToString("0.00") + "|" + description;
                            }
                        }
                    }
                    else
                    {
                        objResponse.Response = Convert.ToDouble(Plan_Detail).ToString("0.00") + "|" + description;
                    }
                }
                else
                {
                    bool isModificationAllowed = CheckModificationAllowed(strcode.Trim());
                    if (isModificationAllowed)
                    {
                        if (CPTStandardFee == null || CPTStandardFee.ToString().Equals("") == true)
                        {
                            objResponse.Response = "0.00" + "|" + description;
                        }
                        else
                        {
                            objResponse.Response = Convert.ToDouble(CPTStandardFee).ToString("0.00") + "|" + description;
                        }
                    }
                    else
                    {
                        objResponse.Response = "0.00" + "|" + description;
                    }
                }
            }
            else
                objResponse.Response = "0.00" + "|" + description;

            CPTChargesViewModel chargesModel = new CPTChargesViewModel();
            using (var ctx = new NPMDBEntities())
            {

                //..below changes are for anesthesia procdeures, written by Tamour Ali on 14/08/2023
                var procedureInfo = ctx.Procedures.FirstOrDefault(p => (p.ProcedureCode == obj.ProcedureCode && p.Alternate_Code == alternateCode) || p.ProcedureCode == obj.ProcedureCode);
                description = ((procedureInfo != null && procedureInfo.ProcedureDescription != null) ? procedureInfo.ProcedureDescription : "");
                var conversionRate = procedureInfo?.ProcedureDefaultCharge;
                var IsAnesthesiaCpt = CheckIfAnesthesiaCpt(obj.ProcedureCode);
                if (IsAnesthesiaCpt == true && procedureInfo != null)
                {
                    objResponse.Response = conversionRate + "|" + description;
                    chargesModel.DefaultUnits = procedureInfo.MxUnits != null ? Convert.ToInt32(procedureInfo.MxUnits) : 0;
                    chargesModel.IsAnesthesiaCpt = true;
                }
                //..above changes are for anesthesia procdeures, written by Tamour Ali on 14/08/2023

                var splittedValue = objResponse.Response.Split('|');
                chargesModel.Charges = splittedValue[0];
                chargesModel.Description = splittedValue[1];
                if (obj.ProcedureCode != null)
                {
                    var procModel = ctx.Procedures.Where(p => ((p.ProcedureCode == obj.ProcedureCode && p.Alternate_Code == alternateCode) || p.ProcedureCode == obj.ProcedureCode) && (p.Deleted == null || p.Deleted == false)).FirstOrDefault();
                    if (procModel != null)
                    {
                        var posDescription = ctx.Place_Of_Services.FirstOrDefault(pos => pos.posshortcode == procModel.ProcedurePosCode);
                        if (posDescription != null)
                            chargesModel.POS = posDescription.POS_Code;  //

                        //below changes are Added By Umer 
                        if (string.IsNullOrEmpty(strcode) || strcode.Equals("0"))
                        {
                            chargesModel.Revenue_Code = "";
                        }
                        

                        else
                        {
                            Provider_Cpt_Plan_Details List = new Provider_Cpt_Plan_Details();
                            List = ctx.Provider_Cpt_Plan_Details.Where(x => x.Provider_Cpt_Plan_Id == strcode && x.Cpt_Code == obj.ProcedureCode).FirstOrDefault();

                            if (List != null)
                                chargesModel.Revenue_Code = List.Revenue_Code ?? "";

                        }
                        
                      
                        
                        //Above
                        //
                        //changes are Added By Umer


                    }
                    var maxDate = Convert.ToDateTime("10/01/2015");
                    //mychanges
                    var ndcModel = ctx.NDC_CrossWalk.Where(scf => scf.HCPCS_Code == obj.ProcedureCode && scf.Practice_Code.ToString() == obj.PracticeCode && scf.Effective_Date_To >= maxDate).ToList();
                    //var ndcModel = ctx.NDC_CrossWalk.Where(scf => scf.HCPCS_Code == obj.ProcedureCode && scf.Effective_Date_To >= maxDate).ToList();
                    chargesModel.NDCCodeList = new List<SelectListViewModel>();
                    foreach (var item in ndcModel)
                    {
                        SelectListViewModel objmodel = new SelectListViewModel
                        {
                            Id = item.NDC_ID,
                            Name = item.NDC2,
                            Meta = new ExpandoObject()
                        };
                        objmodel.Meta.Qualifier = item.Qualifier;
                        chargesModel.NDCCodeList.Add(objmodel);
                    }
                }
                objResponse.Response = chargesModel;
            }
            if (!string.IsNullOrEmpty(description))
            {
                objResponse.Status = "Sucess";
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        //after changes for get chargers on the basis of alternate code or cpt code
        //public ResponseModel GetProcedureCharges([FromBody] CPTWiseCharges obj)
        //    {
        //    ResponseModel objResponse = new ResponseModel();
        //    string strState = "";
        //    string strInsurance = "";
        //    string strIsParticipating = "False";
        //    string strIsFacility = "False";
        //    string strField = "";
        //    string strcode = "";
        //    string strPlan = "";
        //    double curPer = 0.0;
        //    string description = string.Empty;
        //    Practice objPractice = null;

        //    using (var db = new NPMDBEntities())
        //    {
        //        // First check if ProcedureCode exists in the Procedures table
        //        var procedureInfo = db.Procedures
        //                              .FirstOrDefault(p => p.ProcedureCode == obj.ProcedureCode);

        //        // If not found, check with Alternate_Code
        //        if (procedureInfo == null)
        //        {
        //            procedureInfo = db.Procedures
        //                              .FirstOrDefault(p => p.Alternate_Code == obj.ProcedureCode);

        //            // If found by Alternate_Code, set the obj.ProcedureCode to the ProcedureCode from the database
        //                if (procedureInfo != null)
        //            {
        //                obj.ProcedureCode = procedureInfo.ProcedureCode;
        //            }
        //        }

        //        // Set the description if procedureInfo is found
        //        description = ((procedureInfo != null && procedureInfo.ProcedureDescription != null) ? procedureInfo.ProcedureDescription : "");

        //        long pracCode = Convert.ToInt64(obj.PracticeCode);
        //        objPractice = db.Practices.FirstOrDefault(p => p.Practice_Code == pracCode);
        //    }

        //    if (string.IsNullOrEmpty(obj.ProviderCode))
        //    {
        //        obj.ProviderCode = "0";
        //    }

        //    string locationCode = "0";
        //    if (!string.IsNullOrEmpty(obj.LocationCode))
        //    {
        //        locationCode = obj.LocationCode;
        //    }

        //    if (string.IsNullOrEmpty(obj.ModifierCode) || !string.IsNullOrEmpty(obj.ModifierCode))
        //    {
        //        List<string> resultModifier = GetPricingModifier(obj.ModifierCode);
        //        if (resultModifier.Count == 0)
        //        {
        //            obj.ModifierCode = string.Empty;
        //        }
        //    }

        //    if (obj.FacilityCode == "undefined")
        //    {
        //        obj.FacilityCode = "";
        //    }

        //    List<GETEFSPRACTICES_Result> EFSPractice = GetEFSPractices(long.Parse(obj.PracticeCode));
        //    List<string> resultLocState = GetLocationState(locationCode);
        //    if (resultLocState.Count > 0)
        //    {
        //        var resultNew = (from x in resultLocState select x).FirstOrDefault();
        //        strState = resultNew.ToString();
        //    }

        //    string insPayerId = string.Empty;
        //    string insState = (objPractice.Prac_State != null ? objPractice.Prac_State : "");
        //    bool chkSelfPay = bool.Parse(obj.IsSelfPay == "" ? "false" : obj.IsSelfPay);

        //    if (!chkSelfPay)
        //    {
        //        GetPatientInsuranceInfo(obj.InsuranceID, out insPayerId, out insState);
        //        insState = obj.PracticeState.ToString();
        //        strInsurance = insPayerId == "0" ? "" : insPayerId;

        //        if (!strInsurance.Trim().Equals(""))
        //        {
        //            strIsParticipating = "False";
        //            if (EFSPractice.Count < 1)
        //            {
        //                string isParticipating = Is_Participating(long.Parse(insPayerId), obj.ProviderCode);
        //                if (!string.IsNullOrEmpty(isParticipating))
        //                {
        //                    var resultNew = (from x in isParticipating select x).FirstOrDefault();
        //                    strIsParticipating = isParticipating;
        //                }
        //            }
        //            else
        //            {
        //                strIsParticipating = "True";
        //            }
        //        }
        //    }

        //    if (chkSelfPay && strInsurance.Trim().Equals(""))
        //    {
        //        strIsParticipating = "True";
        //    }

        //    List<GET_ISFACILITY_Result> isFacility = IsFacility(long.Parse(obj.ProviderCode));
        //    if (isFacility.Count > 0)
        //    {
        //        var resultNew = (from x in isFacility select x).FirstOrDefault();
        //        strIsFacility = resultNew.IS_PRACTICE_FACILITY_BASED.ToString() == "" ? "False" : resultNew.IS_PRACTICE_FACILITY_BASED.ToString();
        //    }

        //    switch (strIsFacility)
        //    {
        //        case "True":
        //            strField = "Facility_";
        //            break;
        //        case "False":
        //            strField = "Non_Facility_";
        //            break;
        //    }

        //    if (!strInsurance.Trim().Equals(""))
        //    {
        //        switch (strIsParticipating)
        //        {
        //            case "True":
        //                strField = strField + "Participating_Fee";
        //                break;
        //            case "False":
        //                strField = strField + "Non_participating_Fee";
        //                break;
        //        }
        //    }
        //    else
        //        strField = strField + "Non_participating_Fee";

        //    decimal? CPTStandardFee = GetStandardCptFee(strField, strState, obj.ProcedureCode, obj.ModifierCode);

        //    if (EFSPractice.Count < 1)
        //    {
        //        long locationcode = long.Parse(obj.LocationCode);
        //        long facilityCode = 0;
        //        if (!string.IsNullOrEmpty(obj.FacilityCode))
        //        {
        //            facilityCode = long.Parse(obj.FacilityCode);
        //        }
        //        strcode = GetCPTCode(obj.ProviderCode, strInsurance, strState, locationcode, obj.ProcedureCode, obj.ModifierCode, facilityCode, chkSelfPay ? true : false, obj.PracticeCode);
        //    }
        //    else
        //    {
        //        string strSelfPay = chkSelfPay ? "1" : "0";
        //        string strPractice = obj.PracticeCode;
        //        string PlanID = GetProviderCPT_PlanID(obj.ProviderCode, strPractice, chkSelfPay, obj.ModifierCode, obj.ProcedureCode);
        //        var resultCPTID = PlanID;
        //        if (resultCPTID == null)
        //        {
        //            strcode = "0";
        //        }
        //        else
        //        {
        //            strcode = resultCPTID.ToString();
        //        }
        //    }

        //    if (strcode.Equals("0"))
        //    {
        //        if (CPTStandardFee == null || CPTStandardFee.ToString().Equals("") == true)
        //        {
        //            objResponse.Response = "0.00" + "|" + description;
        //        }
        //        else
        //            objResponse.Response = Convert.ToDouble(CPTStandardFee).ToString("0.00") + "|" + description;
        //    }

        //    List<SP_GetProviderCPTPlan_Result> ProviderCPTPlan = ProviderCPT_Plan(strcode);
        //    if (ProviderCPTPlan.Count > 0)
        //    {
        //        var resultCPTPlan = (from x in ProviderCPTPlan select x).FirstOrDefault();
        //        strPlan = resultCPTPlan.CPT_PLAN.ToString();
        //    }

        //    if (strPlan.Trim().ToUpper().Equals("PRCNT"))
        //    {
        //        var resultCPTPlan = (from x in ProviderCPTPlan select x).FirstOrDefault();
        //        curPer = Convert.ToDouble(resultCPTPlan.PERCENTAGE_HIGHER.ToString());
        //        if (CPTStandardFee == null || CPTStandardFee.ToString().Equals("") == true)
        //        {
        //            objResponse.Response = "0.00" + "|" + description;
        //        }
        //        else
        //            objResponse.Response = (Convert.ToDouble(Convert.ToDouble(CPTStandardFee) + ((Convert.ToDouble(CPTStandardFee.ToString()) * curPer) / 100))).ToString("0.00") + "|" + description;
        //    }
        //    else if (strPlan.Trim().ToUpper().Equals("STAND"))
        //    {
        //        var resultCPTPlan = (from x in ProviderCPTPlan select x).FirstOrDefault();
        //        curPer = Convert.ToDouble(resultCPTPlan.PERCENTAGE_HIGHER.ToString());
        //        if (CPTStandardFee == null || CPTStandardFee.ToString().Equals("") == true)
        //        {
        //            objResponse.Response = "0.00" + "|" + description;
        //        }
        //        else
        //            objResponse.Response = Convert.ToDouble(CPTStandardFee).ToString("0.00") + "|" + description;
        //    }
        //    else if (strPlan.Trim().ToUpper().Equals("CUSTM"))
        //    {
        //        decimal? Plan_Detail = Plan_Details(strInsurance, strField, obj.ModifierCode, strcode, obj.ProcedureCode);
        //        if (Plan_Detail != null)
        //        {
        //            if (Plan_Detail.ToString().Equals("") == true)
        //            {
        //                bool isModificationAllowed = CheckModificationAllowed(strcode.Trim());
        //                if (isModificationAllowed)
        //                {
        //                    if (CPTStandardFee == null || CPTStandardFee.ToString().Equals("") == true)
        //                    {
        //                        objResponse.Response = "0.00" + "|" + description;
        //                    }
        //                    else
        //                    {
        //                        objResponse.Response = Convert.ToDouble(CPTStandardFee).ToString("0.00") + "|" + description;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                objResponse.Response = Convert.ToDouble(Plan_Detail).ToString("0.00") + "|" + description;
        //            }
        //        }
        //        else
        //        {
        //            bool isModificationAllowed = CheckModificationAllowed(strcode.Trim());
        //            if (isModificationAllowed)
        //            {
        //                if (CPTStandardFee == null || CPTStandardFee.ToString().Equals("") == true)
        //                {
        //                    objResponse.Response = "0.00" + "|" + description;
        //                }
        //                else
        //                    objResponse.Response = Convert.ToDouble(CPTStandardFee).ToString("0.00") + "|" + description;
        //            }
        //        }
        //    }

        //    return objResponse;
        //}

        //

        public bool CheckModificationAllowed(string Plan_ID)
        {
            string Query = string.Empty;
            Query = "Select isnull(modification_allowed,0) as modification_allowed from provider_cpt_plan where provider_cpt_plan_id=@plan_id and isnull(deleted,0)<>1";
            using (var db = new NPMDBEntities())
            {
                var results = db.Database.SqlQuery<bool>(Query, new SqlParameter("plan_id", Plan_ID));
                return results.FirstOrDefault();
            }
        }
        public List<string> GetPricingModifier(string ModifierCode)
        {
            string Query = string.Empty;
            Query = "Select pricing_modifier from cpt_pricing_modifier  where isnull(pricing_modifier,'')=@ModifierCode and isnull(deleted,0)=0";
            using (var db = new NPMDBEntities())
            {
                var results = db.Database.SqlQuery<string>(Query, new SqlParameter("ModifierCode", ModifierCode)).ToList();
                return results;
            }
        }
        public List<GETEFSPRACTICES_Result> GetEFSPractices(long PracticeCode)
        {
            //GeneralClasses.GeneralClasses generalObj = new GeneralClasses.GeneralClasses();
            using (var db = new NPMDBEntities())
            {
                var result = (from x in db.GETEFSPRACTICES(PracticeCode) select x).ToList<GETEFSPRACTICES_Result>();
                return result;
            }
        }
        public List<string> GetLocationState(string LocationCode)
        {
            using (var db = new NPMDBEntities())
            {
                var result = db.SP_GetLocationState(long.Parse(LocationCode)).ToList();
                return result;
            }
        }
        public void GetPatientInsuranceInfo(string InsId, out string insPayerId, out string insState)
        {
            insPayerId = "0";
            insState = "0";
            if (!string.IsNullOrEmpty(InsId))
            {
                long Ins_Id = long.Parse(InsId);
                List<SP_GetInsuranceInfo_Result> _resultInsInfo = GetInsuranceInfo(Ins_Id);
                var InsPayerID = (from x in _resultInsInfo select x).FirstOrDefault();
                if (InsPayerID != null)
                {
                    insPayerId = Convert.ToString(InsPayerID.INSPAYER_ID);
                }
                else
                { insPayerId = "0"; }
            }
        }
        public List<SP_GetInsuranceInfo_Result> GetInsuranceInfo(long InsID)
        {
            //GeneralClasses.GeneralClasses generalObj = new GeneralClasses.GeneralClasses();
            using (var db = new NPMDBEntities())
            {
                var result = (from x in db.SP_GetInsuranceInfo(InsID) select x).ToList<SP_GetInsuranceInfo_Result>();
                return result;
            }
        }
        public string Is_Participating(long InsPayerID, string providerCode)
        {
            using (var db = new NPMDBEntities())
            {
                var isParticipating = db.SP_GetParticipating(providerCode, InsPayerID).FirstOrDefault();
                var result = (isParticipating != null ? isParticipating.Value.ToString() : "");
                return result;
            }
        }
        public List<GET_ISFACILITY_Result> IsFacility(long providerCode)
        {
            using (var db = new NPMDBEntities())
            {
                var result = (from x in db.GET_ISFACILITY(500) select x).ToList<GET_ISFACILITY_Result>();
                return result;
            }
        }
        public decimal? GetStandardCptFee(string Column, string state, string ProcCode, string Modifier, string alternate_Code)
        {
            using (var db = new NPMDBEntities())
            {
                IEnumerable<decimal?> results = db.Database.SqlQuery<decimal?>("SELECT " + Column + " FROM standard_cpt_fee WHERE state =@state and cpt_code=@ProcCode and isnull(cpt_modifier,'')=@Modifier and isnull(deleted,0)<>1", new SqlParameter("state", state), new SqlParameter("ProcCode", ProcCode), new SqlParameter("Modifier", Modifier));
                return results.FirstOrDefault();
            }
        }
        public string GetCPTCode(string provider, string ins, string state, long location, string procedure, string modifier, long facility_code, bool bSelfPay, string practice, string alternate_Code)
        {
            string strPractice = "";
            string strPreCode = "";
            string strCode = "";
            strPractice = practice;
            strCode = strPractice;
            string isSelfPay = bSelfPay.ToString();
            List<SP_ProviderPlan_Result> ProviderInfo = SP_ProviderPlan(provider, strPractice, isSelfPay, ins, state, location.ToString(), facility_code.ToString(), modifier, procedure, alternate_Code);
            if (ProviderInfo.Count > 0)
            {
                strPreCode = provider;
                foreach (SP_ProviderPlan_Result pInfo in ProviderInfo)
                {
                    if (bSelfPay)
                    {
                        if (pInfo.PROVIDER_CODE.ToString().Trim() == strPreCode && pInfo.SELF_PAY == true)
                        {
                            provider = pInfo.PROVIDER_CODE.ToString();
                            break;
                        }
                        else if (pInfo.PROVIDER_CODE.ToString().Trim() == strPreCode)
                        {
                            provider = pInfo.PROVIDER_CODE.ToString();
                            break;
                        }
                        else if (pInfo.PROVIDER_CODE.ToString().Trim() == "0")
                        {
                            provider = pInfo.PROVIDER_CODE.ToString();
                        }
                    }
                    else
                    {
                        if (pInfo.PROVIDER_CODE.ToString().Trim() == strPreCode)
                        {
                            provider = pInfo.PROVIDER_CODE.ToString();
                            break;
                        }
                        else if (pInfo.PROVIDER_CODE.ToString().Trim() == "0")
                        {
                            provider = pInfo.PROVIDER_CODE.ToString();
                        }
                    }
                }
                strCode = strCode + ((provider.Equals("0")) ? "All" : provider);
            }
            else
                strCode = "0";
            List<SP_InsPayer_Result> PayerInfo = null;
            if (!(bSelfPay))
            {
                PayerInfo = SP_InsPayer(provider, strPractice, ins, state, location.ToString(), facility_code.ToString(), modifier, procedure, alternate_Code);
            }
            if ((ins == "0" || ins == "") && bSelfPay)
            {
                if (ProviderInfo.Count > 0)
                {
                    bool isFoundSelfPayTrue = false;
                    foreach (SP_ProviderPlan_Result pInfo in ProviderInfo)
                    {
                        if (pInfo.SELF_PAY == true)
                        {
                            isFoundSelfPayTrue = true;
                        }
                    }
                    if (isFoundSelfPayTrue == true)
                    {
                        strCode = strCode + "SELF";
                    }
                    else { strCode = strCode + "All"; }
                }
            }
            else
            {
                if (PayerInfo.Count > 0)
                {
                    strPreCode = ins;
                    foreach (SP_InsPayer_Result insPayerInfo in PayerInfo)
                    {
                        if (insPayerInfo.INSPAYER_ID.ToString() == strPreCode)
                        {
                            ins = insPayerInfo.INSPAYER_ID.ToString();
                            break;
                        }
                        else if (insPayerInfo.INSPAYER_ID.ToString().Trim() == "0")
                        {
                            ins = insPayerInfo.INSPAYER_ID.ToString();
                        }
                    }
                    strCode = strCode + ((ins.Trim() == "0") ? "All" : ins);
                }
                else
                    strCode = "0";
            }
            List<SP_InsStatePlan_Result> insStateInfo = GetInsState(provider, strPractice, isSelfPay, ins, state, location.ToString(), facility_code.ToString(), modifier, procedure, alternate_Code);
            if (insStateInfo.Count > 0)
            {
                strPreCode = state;
                foreach (SP_InsStatePlan_Result stateInfo in insStateInfo)
                {
                    if (bSelfPay)
                    {
                        if (stateInfo.INSURANCE_STATE.ToString().Trim() == strPreCode && stateInfo.SELF_PAY == true)
                        {
                            state = stateInfo.INSURANCE_STATE.ToString();
                            break;
                        }
                        else if (stateInfo.INSURANCE_STATE.ToString().Trim() == strPreCode)
                        {
                            state = stateInfo.INSURANCE_STATE.ToString();
                            break;
                        }
                        else if (stateInfo.INSURANCE_STATE.ToString().Trim() == "0")
                        {
                            state = stateInfo.INSURANCE_STATE.ToString();
                        }
                    }
                    else
                    {
                        if (stateInfo.INSURANCE_STATE.ToString().Trim() == strPreCode)
                        {
                            state = stateInfo.INSURANCE_STATE.ToString();
                            break;
                        }
                        else if (stateInfo.INSURANCE_STATE.ToString().Trim() == "0") //Trim chk added by 1596
                        {
                            state = stateInfo.INSURANCE_STATE.ToString();
                        }
                    }
                }
                strCode = strCode + ((state.Trim() == "0") ? "All" : state);
            }
            else
                strCode = "0";
            List<SP_InsLocationPlan_Result> insLocationsInfo = GetInsLocation(provider, strPractice, isSelfPay, ins, state, location.ToString(), facility_code.ToString(), modifier, procedure, alternate_Code);
            if (insLocationsInfo.Count > 0)
            {
                strPreCode = location.ToString();
                foreach (SP_InsLocationPlan_Result LocationInfo in insLocationsInfo)
                {
                    if (bSelfPay)
                    {
                        if (LocationInfo.LOCATION_CODE.ToString().Trim() == strPreCode && LocationInfo.SELF_PAY == true)
                        {
                            location = Convert.ToInt64(LocationInfo.LOCATION_CODE);
                            break;
                        }
                        else if (LocationInfo.LOCATION_CODE.ToString().Trim() == strPreCode)
                        {
                            location = Convert.ToInt64(LocationInfo.LOCATION_CODE);
                            break;
                        }
                        else if (LocationInfo.LOCATION_CODE.ToString().Trim() == "0")
                        {
                            location = Convert.ToInt64(LocationInfo.LOCATION_CODE);
                        }
                    }
                    else
                    {
                        if (LocationInfo.LOCATION_CODE.ToString().Trim() == strPreCode)
                        {
                            location = Convert.ToInt64(LocationInfo.LOCATION_CODE);
                            break;
                        }
                        else if (LocationInfo.LOCATION_CODE.ToString().Trim() == "0")
                        {
                            location = Convert.ToInt64(LocationInfo.LOCATION_CODE);
                        }
                    }
                }
                strCode = strCode + ((location.ToString().Trim() == "0") ? "All" : location.ToString());
            }
            else
                strCode = "0";
            List<SP_InsFacilityPlan_Result> insFacilityInfo = GetInsFacility(provider, strPractice, isSelfPay, ins, state, location.ToString(), facility_code.ToString(), modifier, procedure, alternate_Code);
            if (insFacilityInfo.Count > 0)
            {
                strPreCode = facility_code.ToString();
                foreach (SP_InsFacilityPlan_Result FacilityInfo in insFacilityInfo)
                {
                    if (bSelfPay)
                    {
                        if (FacilityInfo.FACILITY_CODE.ToString() == strPreCode && FacilityInfo.SELF_PAY == true)
                        {
                            facility_code = Convert.ToInt64(FacilityInfo.FACILITY_CODE);
                            break;
                        }
                        else if (FacilityInfo.FACILITY_CODE.ToString().Trim() == strPreCode)
                        {
                            facility_code = Convert.ToInt64(FacilityInfo.FACILITY_CODE);
                            break;
                        }
                        else if (FacilityInfo.FACILITY_CODE.ToString().Trim() == "0")
                        {
                            facility_code = Convert.ToInt64(FacilityInfo.FACILITY_CODE);
                        }
                    }
                    else
                    {
                        if (FacilityInfo.FACILITY_CODE.ToString().Trim() == strPreCode)
                        {
                            facility_code = Convert.ToInt64(FacilityInfo.FACILITY_CODE);
                            break;
                        }
                        else if (FacilityInfo.FACILITY_CODE.ToString().Trim() == "0")
                        {
                            facility_code = Convert.ToInt64(FacilityInfo.FACILITY_CODE);
                        }
                    }
                }
                strCode = strCode + ((facility_code.ToString().Trim() == "0") ? "All" : facility_code.ToString());
            }
            else
                strCode = "0";
            if (strCode.Equals("0"))
                return GetCPTCode(provider, ins, state, location, facility_code, bSelfPay ? true : false, practice);
            return strCode;
        }
        public string GetCPTCode(string provider, string ins, string state, long location, long facility_code, bool bSelfPay, string practiceCode)
        {
            string strPractice = practiceCode;
            string strPreCode = "";
            string strCode = "";
            string isSelfPay = bSelfPay.ToString();
            //List<MTBCSOFT_WEB_PRACTICE_PROVIDER_Result> practiceInfo = GetPracticeCodeFromProviderCode(provider);
            //if (practiceInfo.Count > 0)
            //{
            //    var resultPracticeCode = (from x in practiceInfo select x).FirstOrDefault();
            //    strPractice = resultPracticeCode.PRACTICE_CODE.ToString();
            //}
            //else
            //{
            //    return "0";
            //}
            strCode = strPractice;
            List<SP_ProviderWithoutModifierAndProcedure_Result> providerInfo = GetProviderInfoWithOutModifierAndProcedure(provider, strPractice, isSelfPay, ins, state, location.ToString(), facility_code.ToString());
            if (providerInfo.Count > 0)
            {
                strPreCode = provider;
                foreach (SP_ProviderWithoutModifierAndProcedure_Result pInfo in providerInfo)
                {
                    if (pInfo.PROVIDER_CODE.ToString().Trim() == strPreCode)
                    {
                        provider = pInfo.PROVIDER_CODE.ToString();
                        break;
                    }
                    else if (pInfo.PROVIDER_CODE.ToString().Trim() == "0")
                    {
                        provider = pInfo.PROVIDER_CODE.ToString();
                    }
                }
                strCode = strCode + ((provider.Trim().Equals("0")) ? "All" : provider);
            }
            else
                return "0";
            List<SP_InsPayerWithoutModifierAndProcedure_Result> PayerInfo = null;
            if (!(bSelfPay))
            {
                PayerInfo = GetInsPayerWithOutModifierAndProcedure(provider, strPractice, ins, state, location.ToString(), facility_code.ToString());
            }
            if ((ins == "0" || ins == "") && bSelfPay)
            {
                var selfpayresult = ListofProviderInfo(bSelfPay, strPractice, provider, location.ToString(), facility_code.ToString(), state, ins);
                if ((from x in selfpayresult where x.SELF_PAY == true select x).Count() > 1)
                {
                    strCode = strCode + "SELF";
                }
                else
                {
                    strCode = strCode + "All";
                }
            }
            else
            {
                if (PayerInfo.Count > 0)
                {
                    strPreCode = ins;
                    foreach (SP_InsPayerWithoutModifierAndProcedure_Result insPayerInfo in PayerInfo)
                    {
                        if (insPayerInfo.INSPAYER_ID.ToString().Trim() == strPreCode)
                        {
                            ins = insPayerInfo.INSPAYER_ID.ToString();
                            break;
                        }
                        else if (insPayerInfo.INSPAYER_ID.ToString().Trim() == "0")
                        {
                            ins = insPayerInfo.INSPAYER_ID.ToString();
                        }
                    }
                    strCode = strCode + ((ins.Trim() == "0") ? "All" : ins);
                }
                else
                    return "0";
            }
            List<SP_InsStateWithoutModifierAndProcedure_Result> insStateInfo = GetInsStateWithOutModifierAndProcedure(provider, strPractice, isSelfPay, ins, state, location.ToString(), facility_code.ToString());
            if (insStateInfo.Count > 0)
            {
                strPreCode = state;
                foreach (SP_InsStateWithoutModifierAndProcedure_Result stateInfo in insStateInfo)
                {
                    if (stateInfo.INSURANCE_STATE.ToString().Trim() == strPreCode)
                    {
                        state = stateInfo.INSURANCE_STATE.ToString();
                        break;
                    }
                    else if (stateInfo.INSURANCE_STATE.ToString().Trim() == "0")
                    {
                        state = stateInfo.INSURANCE_STATE.ToString();
                    }
                }
                strCode = strCode + ((state.Trim() == "0") ? "All" : state);
            }
            else
                return "0";
            List<SP_InsLocationWithoutModifierAndProcedure_Result> insLocationsInfo = GetInsLocationWithOutModifierAndProcedure(provider, strPractice, isSelfPay, ins, state, location.ToString(), facility_code.ToString());
            if (insLocationsInfo.Count > 0)
            {
                strPreCode = location.ToString();
                foreach (SP_InsLocationWithoutModifierAndProcedure_Result LocationInfo in insLocationsInfo)
                {
                    if (LocationInfo.LOCATION_CODE.ToString().Trim() == strPreCode)
                    {
                        location = Convert.ToInt64(LocationInfo.LOCATION_CODE);
                        break;
                    }
                    else if (LocationInfo.LOCATION_CODE.ToString().Trim() == "0")
                    {
                        location = Convert.ToInt64(LocationInfo.LOCATION_CODE);
                    }
                }
                strCode = strCode + ((location.ToString().Trim() == "0") ? "All" : location.ToString());
            }
            else
                return "0";
            List<SP_InsFacilityInfoWithoutModifierAndProcedure_Result> insFacilityInfo = GetInsFacilityWithOutModifierAndProcedure(provider, strPractice, isSelfPay, ins, state, location.ToString(), facility_code.ToString());
            if (insFacilityInfo.Count > 0)
            {
                strPreCode = facility_code.ToString();
                foreach (SP_InsFacilityInfoWithoutModifierAndProcedure_Result FacilityInfo in insFacilityInfo)
                {
                    if (FacilityInfo.FACILITY_CODE.ToString().Trim() == strPreCode)
                    {
                        facility_code = Convert.ToInt64(FacilityInfo.FACILITY_CODE);
                        break;
                    }
                    else if (FacilityInfo.FACILITY_CODE.ToString().Trim() == "0")
                    {
                        facility_code = Convert.ToInt64(FacilityInfo.FACILITY_CODE);
                    }
                }
                strCode = strCode + ((facility_code.ToString().Trim() == "0") ? "All" : facility_code.ToString());
            }
            else
                return "0";
            return strCode;
        }
        public static List<SP_ProviderPlan_Result> SP_ProviderPlan(string provider, string practiceCode, string bSelfPay, string insPayerID, string state, string location, string facilityCode, string modifier, string procCode, string alternate_Code)
        {
            using (var db = new NPMDBEntities())
            {
                var result = db.SP_ProviderPlan(provider, practiceCode, bool.Parse(bSelfPay), insPayerID, state, location, facilityCode, modifier, procCode, alternate_Code).ToList();
                return result;
            }
        }
        public static List<SP_InsPayer_Result> SP_InsPayer(string provider, string practiceCode, string insPayerID, string state,
        string location, string facilityCode, string modifier, string procCode, string alternate_Code)
        {
            using (var db = new NPMDBEntities())
            {
                var result = db.SP_InsPayer(provider, insPayerID, state, location, facilityCode, procCode, modifier, practiceCode, alternate_Code).ToList();
                return result;
            }
        }
        public static List<SP_InsStatePlan_Result> GetInsState(string provider, string practiceCode, string bSelfPay, string insPayerID, string state,
         string location, string facilityCode, string modifier, string procCode, string alternate_Code)
        {
            using (var db = new NPMDBEntities())
            {
                var result = db.SP_InsStatePlan(practiceCode, provider, bool.Parse(bSelfPay), insPayerID, state, location, facilityCode, procCode, modifier, alternate_Code).ToList();
                return result;
            }
        }
        public static List<SP_InsLocationPlan_Result> GetInsLocation(string provider, string practiceCode, string bSelfPay, string insPayerID, string state,
            string location, string facilityCode, string modifier, string procCode, string alternate_Code)
        {
            using (var db = new NPMDBEntities())
            {
                var result = db.SP_InsLocationPlan(practiceCode, provider, bool.Parse(bSelfPay), insPayerID, state, location, facilityCode, procCode, modifier, alternate_Code).ToList();
                return result;
            }
        }
        public static List<SP_InsFacilityPlan_Result> GetInsFacility(string provider, string practiceCode, string bSelfPay, string insPayerID, string state,
            string location, string facilityCode, string modifier, string procCode, string alternate_Code)
        {
            using (var db = new NPMDBEntities())
            {
                var result = db.SP_InsFacilityPlan(practiceCode, provider, bool.Parse(bSelfPay), insPayerID, state, location, facilityCode, procCode, modifier, alternate_Code).ToList();
                return result;
            }
        }
        public static List<SP_InsFacilityInfoWithoutModifierAndProcedure_Result> GetInsFacilityWithOutModifierAndProcedure(string provider, string practiceCode, string bSelfPay, string insPayerID, string state,
        string location, string facilityCode)
        {
            using (var db = new NPMDBEntities())
            {
                var result = db.SP_InsFacilityInfoWithoutModifierAndProcedure(practiceCode, provider, bool.Parse(bSelfPay), insPayerID, state, location, facilityCode).ToList();
                return result;
            }
        }
        public static List<SP_InsLocationWithoutModifierAndProcedure_Result> GetInsLocationWithOutModifierAndProcedure(string provider, string practiceCode, string bSelfPay, string insPayerID, string state,
            string location, string facilityCode)
        {
            using (var db = new NPMDBEntities())
            {
                var result = db.SP_InsLocationWithoutModifierAndProcedure(practiceCode, provider, bool.Parse(bSelfPay), insPayerID, state, location, facilityCode).ToList();
                return result;
            }
        }
        public static List<SP_ProviderWithoutModifierAndProcedure_Result> GetProviderInfoWithOutModifierAndProcedure(string provider, string practiceCode, string bSelfPay, string insPayerID, string state,
            string location, string facilityCode)
        {
            using (var db = new NPMDBEntities())
            {
                var result = db.SP_ProviderWithoutModifierAndProcedure(provider, practiceCode, bool.Parse(bSelfPay), insPayerID, state, location, facilityCode).ToList();
                return result;
            }
        }
        public static List<SP_InsPayerWithoutModifierAndProcedure_Result> GetInsPayerWithOutModifierAndProcedure(string provider, string practiceCode, string insPayerID, string state,
        string location, string facilityCode)
        {
            using (var db = new NPMDBEntities())
            {
                var result = db.SP_InsPayerWithoutModifierAndProcedure(provider, insPayerID, state, location, facilityCode, practiceCode).ToList();
                return result;
            }
        }
        public static List<SP_InsStateWithoutModifierAndProcedure_Result> GetInsStateWithOutModifierAndProcedure(string provider, string practiceCode, string bSelfPay, string insPayerID, string state,
         string location, string facilityCode)
        {
            using (var db = new NPMDBEntities())
            {
                var result = db.SP_InsStateWithoutModifierAndProcedure(practiceCode, provider, bool.Parse(bSelfPay), insPayerID, state, location, facilityCode).ToList();
                return result;
            }
        }
        public static List<SP_ProviderInfo_Result> ListofProviderInfo(bool selfpay, string practice_code, string provider_code, string location_code, string facility_code, string state, string insid)
        {
            using (var db = new NPMDBEntities())
            {
                var ProviderInfo = db.SP_ProviderInfo(selfpay, practice_code, provider_code, location_code, facility_code, state, insid).ToList();
                return ProviderInfo;
            }
        }

        // Added BY hamza akhlaq fee schedule change check procedure 
        public string GetProviderCPT_PlanID(string provider, string PracticeCode, bool bSelfPay, string modifier, string procCode, string alternate_Code, DateTime Dos, string inspayer, string facilitycode, string Locationcode)
        {
            string providerCptPlanId = "";

            try
            {
                using (var db = new NPMDBEntities())
                {
                    var result = db.SP_GetProviderCPTPlanID(PracticeCode, provider, inspayer, Locationcode, facilitycode, bSelfPay, procCode, modifier, alternate_Code, Dos).ToList();

                    if (result != null && result.Count > 0)
                    {
                        providerCptPlanId = result.FirstOrDefault().PROVIDER_CPT_PLAN_ID != null
                            ? result.FirstOrDefault().PROVIDER_CPT_PLAN_ID
                            : "";
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (log it, rethrow it, or return a specific message)
                // Example: Log the error message
                // Log.Error("Error in GetProviderCPT_PlanID: " + ex.Message);

                // Optionally, you could return a specific message or rethrow the exception
                providerCptPlanId = "Error occurred: " + ex.Message;
            }

            return providerCptPlanId;
        }

        //   public string GetProviderCPT_PlanID(string provider, string PracticeCode, bool bSelfPay, string modifier, string procCode, string alternate_Code,DateTime Dos,string inspayer, string facilitycode,string Locationcode )
        //{
        //       string providerCptPlanId = "";
        //       //GeneralClasses.GeneralClasses generalObj = new GeneralClasses.GeneralClasses();
        //       using (var db = new NPMDBEntities())
        //           {
        //           var result = db.SP_GetProviderCPTPlanID_Hamza(PracticeCode, provider, inspayer, Locationcode, facilitycode, bSelfPay, procCode, modifier, alternate_Code, Dos ).ToList();
        //           if (result != null && result.Count > 0)
        //           {
        //               //if(result.FirstOrDefault().PROVIDER_CPT_PLAN_ID != null)
        //               providerCptPlanId = (result.FirstOrDefault().PROVIDER_CPT_PLAN_ID != null ? result.FirstOrDefault().PROVIDER_CPT_PLAN_ID : "");
        //               return providerCptPlanId;
        //           }
        //           else
        //               return providerCptPlanId;
        //       }
        //   }
        public List<SP_GetProviderCPTPlan_Result> ProviderCPT_Plan(string strPlanID)
        {
            //GeneralClasses.GeneralClasses generalObj = new GeneralClasses.GeneralClasses();
            using (var db = new NPMDBEntities())
            {
                var result = (from x in db.SP_GetProviderCPTPlan(strPlanID) select x).ToList<SP_GetProviderCPTPlan_Result>();
                return result;
            }
        }
        public decimal? Plan_Details(string strInsurance, string strField, string modifierCode, string PlanId, string procCode, string Alternate_Code)
        {
            string Query = string.Empty;
            try
            {
                if (strInsurance.Trim().Equals(""))
                {
                    Query = "Select CAST(pcpd." + strField + " AS DECIMAL(18, 2)) AS " + strField + " " +
                         " from provider_cpt_plan_details pcpd,provider_cpt_plan pcp " +
                         " where pcp.provider_cpt_plan_id=@PlanId " +
                         "  and pcpd.cpt_code =@procCode " +
                         "  and isnull(pcpd.Alternate_code,'')= @Alternate_Code " +
                         "  and isnull(pcpd.cpt_modifier,'')= @modifierCode " +
                         "  and pcpd.PROVIDER_CPT_PLAN_ID = PCP.PROVIDER_CPT_PLAN_ID " +
                         "  and isnull(pcp.deleted,0)<>1 and isnull(pcpd.deleted,0)<>1";
                }
                else if (!modifierCode.Trim().Equals(""))
                {
                    Query = "Select CAST(pcpd." + strField + " AS DECIMAL(18, 2)) AS " + strField + " " +
                          " from provider_cpt_plan_details pcpd  " +
                          " where pcpd.provider_cpt_plan_id =@PlanId " +
                          " and pcpd.cpt_code =@procCode  " +
                           "  and isnull(pcpd.Alternate_code,'')= @Alternate_Code " +
                          " and isnull(pcpd.cpt_modifier,'')= @modifierCode " +
                          " and isnull(pcpd.deleted,0)<>1    ";
                }
                else
                {
                    Query = "Select CAST(pcpd." + strField + " AS DECIMAL(18, 2)) AS " + strField + " " +
                          "   from provider_cpt_plan_details pcpd " +
                          "  where pcpd.provider_cpt_plan_id =@PlanId " +
                          "   and pcpd.cpt_code =@procCode  " +
                           "  and isnull(pcpd.Alternate_code,'')= @Alternate_Code " +
                          "   and isnull(pcpd.cpt_modifier,'') =  isnull(nullif(@modifierCode,''),pcpd.cpt_modifier)  " +
                          "   and isnull(pcpd.deleted,0)<>1    ";
                }
                using (var db = new NPMDBEntities())
                {
                    //IEnumerable<decimal?> results = db.ExecuteQuery<decimal?>(Query);
                    var results = db.Database.SqlQuery<decimal?>(Query, new SqlParameter("PlanId", PlanId), new SqlParameter("procCode", procCode), new SqlParameter("modifierCode", modifierCode), new SqlParameter("Alternate_Code", Alternate_Code));
                    return results.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }
        //public decimal? Plan_Details(string strInsurance, string strField, string modifierCode, string PlanId, string procCode, string alternate_Code)
        //{
        //    string Query = string.Empty;
        //    if (strInsurance.Trim().Equals(""))
        //    {
        //        Query = "Select max(pcpd." + strField + ")  " + strField + "" +
        //             " from provider_cpt_plan_details pcpd,provider_cpt_plan pcp " +
        //             " where pcp.provider_cpt_plan_id=@PlanId " +
        //             "  and pcpd.cpt_code =@procCode " +
        //             "  and isnull(pcpd.cpt_modifier,'')= @modifierCode " +
        //             "  and pcpd.PROVIDER_CPT_PLAN_ID = PCP.PROVIDER_CPT_PLAN_ID " +
        //             "  and isnull(pcp.deleted,0)<>1 and isnull(pcpd.deleted,0)<>1";
        //    }
        //    else if (!modifierCode.Trim().Equals(""))
        //    {
        //        Query = "Select pcpd." + strField + "  " + strField + " " +
        //              " from provider_cpt_plan_details pcpd  " +
        //              " where pcpd.provider_cpt_plan_id =@PlanId " +
        //              " and pcpd.cpt_code =@procCode  " +
        //              " and isnull(pcpd.cpt_modifier,'')= @modifierCode " +
        //              " and isnull(pcpd.deleted,0)<>1    ";
        //    }
        //    else
        //    {
        //        Query = "Select pcpd." + strField + " " + strField + "  " +
        //              "   from provider_cpt_plan_details pcpd " +
        //              "  where pcpd.provider_cpt_plan_id =@PlanId " +
        //              "   and pcpd.cpt_code =@procCode  " +
        //              "   and isnull(pcpd.cpt_modifier,'') =  isnull(nullif(@modifierCode,''),pcpd.cpt_modifier)  " +
        //              "   and isnull(pcpd.deleted,0)<>1    ";
        //    }
        //    using (var db = new NPMDBEntities())
        //    {
        //        //IEnumerable<decimal?> results = db.ExecuteQuery<decimal?>(Query);
        //        var results = db.Database.SqlQuery<decimal?>(Query, new SqlParameter("PlanId", PlanId), new SqlParameter("procCode", procCode), new SqlParameter("modifierCode", modifierCode));
        //        return results.FirstOrDefault();
        //    }
        //}
        public void UpdateClaimStatus(List<long> claimIds, string status, string typeOfStatus)
        {
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var claims = ctx.Claims.Where(c => claimIds.Contains(c.Claim_No));
                    if (typeOfStatus == "Pat_Status")
                    {
                        claims.ForEach(c => c.Pat_Status = status);
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        //Added  By Hamza akhlaq inbox Delete API
        public BaseResponse DeleteInboxInvoice(long invoiceId)
        {
            BaseResponse res = new BaseResponse();
            if (invoiceId != null)
            {
                var invoicePayments = _deltaSyncRepository.GetPyments_Invoice(invoiceId);
                bool allPaymentsDeleted = true;

                for (int i = 0; i < (invoicePayments?.invoice_payments?.Count ?? 0); i++)
                {
                    if (invoicePayments.invoice_payments[i].payment_id != null)
                    {
                        var deleteInvoicepaymentsRequest = new DeleteInvoicepaymentsRequest();
                        var paymentId = invoicePayments.invoice_payments[i].payment_id;

                        deleteInvoicepaymentsRequest.id = invoicePayments.invoice_payments[i].id;
                        deleteInvoicepaymentsRequest.payment.voided = "true";

                        var deleteInvoicePaymentsResponse = _deltaSyncRepository.DeletePayments(deleteInvoicepaymentsRequest, paymentId);
                        if (!deleteInvoicePaymentsResponse.IsSuccessful)
                        {
                            allPaymentsDeleted = false;
                            break;
                        }
                    }
                }

                var deleteinvoice = _deltaSyncRepository.DeleteInvoice(invoiceId);
                res.IsSuccessful = deleteinvoice.IsSuccessful;
            }
            return res;
        }

        private void DeleteSyncedClaim(long ClaimNumber)
        {
            try
            {
                using (var db = new NPMDBEntities())
                {
                    var query = "DELETE FROM SyncedClaims WHERE Claim_no = @ClaimNumber";
                    var result = db.Database.ExecuteSqlCommand(query, new SqlParameter("@ClaimNumber", ClaimNumber));

                    if (result >= 0)
                    {
                        // Deleting from syncedclaimcharges
                        var claimCharges = db.Claim_Charges.Where(x => x.Claim_No == ClaimNumber).ToList();

                        if (claimCharges != null && claimCharges.Count > 0)
                        {
                            foreach (var charge in claimCharges)
                            {
                                if (charge.claim_charges_id != null)
                                {
                                    var q = "DELETE FROM syncedclaimcharges WHERE claim_charges_id = @id";
                                    db.Database.ExecuteSqlCommand(q, new SqlParameter("@id", charge.claim_charges_id));
                                }
                            }
                        }
                        // Deleting from syncedclaimpayments
                        var claimPayments = db.Claim_Payments.Where(x => x.Claim_No == ClaimNumber).ToList();
                        foreach (var payment in claimPayments)
                        {
                            if (payment.claim_payments_id != null)
                            {
                                var queryPayments = "DELETE FROM syncedclaimpayments WHERE claim_payments_id = @Payment_id";
                                db.Database.ExecuteSqlCommand(queryPayments, new SqlParameter("@Payment_id", payment.claim_payments_id));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }



        //.. Added by Pir Ubaid - CPT With Multiple Description

        //for check the code wethter it is CPT code or Alternate Code 
        public ResponseModel CheckCode(string Code)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                string Procedure_Code = null;
                string Alternate_Code = null;
                //  string Description = null;
                //string Charges = null;

                using (var ctx = new NPMDBEntities())
                {
                    Console.WriteLine($"Received Code: {Code}");


                    //                 var providerCptDetails = ctx.Provider_Cpt_Plan_Details
                    //.Where(p => p.Cpt_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase)
                    //         || p.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase))
                    //.OrderByDescending(p => p.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase)) // Prioritize exact Alternate_Code match
                    //.ThenByDescending(p => p.Alternate_Code == null && p.Cpt_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase)) // Prioritize matching Cpt_Code with null Alternate_Code
                    //.ThenBy(p => p.Alternate_Code != null) // Lastly, prioritize non-null Alternate_Code
                    //.FirstOrDefault();

                    var providerCptDetails = ctx.Provider_Cpt_Plan_Details
               .FirstOrDefault(p => p.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase));

                    if (providerCptDetails == null)
                    {
                        // 2. If no result, check for a match with Cpt_Code and null Alternate_Code
                        providerCptDetails = ctx.Provider_Cpt_Plan_Details
                            .FirstOrDefault(p => p.Cpt_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase)
                                              && p.Alternate_Code == null);

                        if (providerCptDetails == null)
                        {
                            // 3. If no result, return the first match for Cpt_Code, regardless of Alternate_Code
                            providerCptDetails = ctx.Provider_Cpt_Plan_Details
                                .FirstOrDefault(p => p.Cpt_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase));
                        }
                    }
                    if (providerCptDetails != null)
                    {
                        Console.WriteLine($"Found in Provider_Cpt_Plan_Details: {providerCptDetails.Cpt_Code}, {providerCptDetails.Alternate_Code}");

                        // If code matches Cpt_Code, return its Alternate_Code
                        if (providerCptDetails.Cpt_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            Procedure_Code = providerCptDetails.Cpt_Code?.Trim();
                            Alternate_Code = providerCptDetails.Alternate_Code?.Trim();
                            //  Description = providerCptDetails.Cpt_Description?.Trim();


                        }
                        // If code matches Alternate_Code, return its Cpt_Code
                        else if (providerCptDetails.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            Procedure_Code = providerCptDetails.Cpt_Code?.Trim();
                            Alternate_Code = providerCptDetails.Alternate_Code;
                            //Description = providerCptDetails.Cpt_Description?.Trim();
                        }
                    }

                    if (providerCptDetails != null)
                    {
                        Console.WriteLine($"Found in Provider_Cpt_Plan_Details: {providerCptDetails.Cpt_Code}, {providerCptDetails.Alternate_Code}");

                        // If code matches Cpt_Code, return its Alternate_Code
                        if (providerCptDetails.Cpt_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            Procedure_Code = providerCptDetails.Cpt_Code?.Trim();
                            Alternate_Code = providerCptDetails.Alternate_Code?.Trim();
                            //  Description = providerCptDetails.Cpt_Description?.Trim();


                        }
                        // If code matches Alternate_Code, return its Cpt_Code
                        else if (providerCptDetails.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            Procedure_Code = providerCptDetails.Cpt_Code?.Trim();
                            Alternate_Code = providerCptDetails.Alternate_Code;
                            //Description = providerCptDetails.Cpt_Description?.Trim();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Not found in Provider_Cpt_Plan_Details, checking Procedures table.");

                        //                     var procedure = ctx.Procedures
                        //.Where(p => p.ProcedureCode.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase)
                        //         || p.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase))
                        //.OrderByDescending(p => p.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase)) // Prioritize exact Alternate_Code match
                        //.ThenByDescending(p => p.Alternate_Code == null && p.ProcedureCode.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase)) // Prioritize matching ProcedureCode with null Alternate_Code
                        //.ThenBy(p => p.Alternate_Code != null) // Lastly, pick non-null Alternate_Code for remaining ProcedureCode matches
                        //.FirstOrDefault();

                        // 1. First, check for a match with ProcedureCode and null Alternate_Code
                        var procedure = ctx.Procedures
                            .FirstOrDefault(p => p.ProcedureCode.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase)
                                              && p.Alternate_Code == null);

                        if (procedure == null)
                        {
                            // 2. If no result, check for an exact match with Alternate_Code
                            procedure = ctx.Procedures
                                .FirstOrDefault(p => p.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase));

                            if (procedure == null)
                            {
                                // 3. If no result, return the first match for ProcedureCode, regardless of Alternate_Code
                                procedure = ctx.Procedures
                                    .FirstOrDefault(p => p.ProcedureCode.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase));
                            }
                        }

                        if (procedure != null)
                        {
                            Console.WriteLine($"Found in Procedures: {procedure.ProcedureCode}, {procedure.Alternate_Code}");

                            // If code matches ProcedureCode, return its Alternate_Code
                            if (procedure.ProcedureCode.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                Procedure_Code = procedure.ProcedureCode;
                                Alternate_Code = procedure.Alternate_Code;
                                //Description = procedure.ProcedureDescription;
                            }
                            // If code matches Alternate_Code, return its ProcedureCode
                            else if (procedure.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                Procedure_Code = procedure.ProcedureCode;
                                Alternate_Code = procedure.Alternate_Code;
                                // Description = procedure.ProcedureDescription;
                            }
                        }
                    }
                }

                // If Procedure_Code is not null, it means we found a match
                if (Procedure_Code != null)
                {
                    objResponse.Status = "Success";
                    objResponse.Response = new
                    {
                        Procedure_Code = Procedure_Code,
                        Alternate_Code = Alternate_Code,
                        //Description = Description
                    };
                }
                else
                {
                    // If no match was found in either table
                    objResponse.Status = "Invalid Code";
                }
                return objResponse;
            }
            catch (Exception ex)
            {
                // You may want to log the exception or handle it as per your error handling strategy
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
        public ResponseModel InsertOrUpdateClaimPayments(List<Claim_Payments> paymentList, long userId)
        {
            ResponseModel objResponse = new ResponseModel();

            using (var db = new NPMDBEntities())
            {
                try
                {
                    foreach (var payment in paymentList)
                    {
                        if (payment == null || payment.Deleted == true)
                            continue;

                        var existingPayment = db.Claim_Payments.FirstOrDefault(cp => cp.claim_payments_id == payment.claim_payments_id);
                        var claim = db.Claims.FirstOrDefault(c => c.Claim_No == payment.Claim_No);
                        if (existingPayment != null)
                        {
                            // Update existing
                            existingPayment.Deleted = false;
                            existingPayment.Modified_By = userId;
                            existingPayment.Modified_Date = DateTime.Now;

                            existingPayment.Insurance_Id = payment.Insurance_Id;
                            existingPayment.Payment_Source = payment.Payment_Source;
                            existingPayment.ENTERED_FROM =payment.ENTERED_FROM;
                            existingPayment.Payment_Type =payment.Payment_Type;
                            existingPayment.Amount_Paid = payment.Amount_Paid;
                            existingPayment.Date_Entry = payment.Date_Entry;
                            existingPayment.Date_Filing = payment.Date_Filing;
                            existingPayment.Sequence_No = payment.Sequence_No;
                            existingPayment.Charged_Proc_Code = payment.Charged_Proc_Code;
                            existingPayment.DOS_From = payment.DOS_From;
                            existingPayment.Dos_To = payment.Dos_To;
                            existingPayment.Paid_Proc_Code = payment.Paid_Proc_Code;
                            existingPayment.Units = payment.Units;
                            existingPayment.MODI_CODE1 = payment.MODI_CODE1;
                            existingPayment.MODI_CODE2 = payment.MODI_CODE2;
                            existingPayment.Amount_Approved = payment.Amount_Approved;
                            existingPayment.Amount_Adjusted = payment.Amount_Adjusted;
                            existingPayment.Claim_No = payment.Claim_No;
                            existingPayment.ERA_ADJUSTMENT_CODE = payment.ERA_ADJUSTMENT_CODE;
                            existingPayment.ERA_Rejection_CATEGORY_CODE = payment.ERA_Rejection_CATEGORY_CODE;
                            existingPayment.Payment_No = 123;
                            existingPayment.DEPOSITSLIP_ID = payment.DEPOSITSLIP_ID;
                            existingPayment.BATCH_NO = payment.BATCH_NO;
                            existingPayment.BATCH_DATE = payment.BATCH_DATE;
                            existingPayment.Details = payment.Details;
                            existingPayment.Check_No = payment.Check_No;
                            existingPayment.Reject_Type = payment.Reject_Type;
                            existingPayment.Reject_Amount = payment.Reject_Amount;
                            existingPayment.DepositDate = payment.DepositDate;
                            existingPayment.T_payment_id = payment.T_payment_id;
                        }
                        else
                        {
                            // Insert new
                            var cp = new Claim_Payments
                            {
                                claim_payments_id = Convert.ToInt64(db.SP_TableIdGenerator("Claim_Payments_Id").FirstOrDefault()?.ToString()),
                                Deleted = false,
                                Created_By = userId,
                                Created_Date = DateTime.Now,
                                Date_Adj_Payment = DateTime.Now,
                                Insurance_Id = payment.Insurance_Id,
                                Payment_Source = payment.Payment_Source,
                                ENTERED_FROM = payment.ENTERED_FROM,
                                Payment_Type = payment.Payment_Type,
                                Amount_Paid = payment.Amount_Paid,
                                Date_Entry = payment.Date_Entry,
                                Date_Filing = payment.Date_Filing,
                                Sequence_No = payment.Sequence_No,
                                Charged_Proc_Code = payment.Charged_Proc_Code,
                                DOS_From = payment.DOS_From,
                                Dos_To = payment.Dos_To,
                                Paid_Proc_Code = payment.Paid_Proc_Code,
                                Units = payment.Units,
                                MODI_CODE1 = payment.MODI_CODE1,
                                MODI_CODE2 = payment.MODI_CODE2,
                                Amount_Approved = payment.Amount_Approved,
                                Amount_Adjusted = payment.Amount_Adjusted,
                                Claim_No = payment.Claim_No,
                                ERA_ADJUSTMENT_CODE = payment.ERA_ADJUSTMENT_CODE,
                                ERA_Rejection_CATEGORY_CODE = payment.ERA_Rejection_CATEGORY_CODE,
                                Payment_No = 123,
                                DEPOSITSLIP_ID = payment.DEPOSITSLIP_ID,
                                BATCH_NO = payment.BATCH_NO,
                                BATCH_DATE = payment.BATCH_DATE,
                                Details = payment.Details,
                                Check_No = payment.Check_No,
                                Reject_Type = payment.Reject_Type,
                                Reject_Amount = payment.Reject_Amount,
                                DepositDate = payment.DepositDate,
                                T_payment_id=payment.T_payment_id
                            };
                            if (payment.Payment_Source == "T")
                            {
                                claim.Amt_Due = claim.Amt_Due - payment.Amount_Paid;
                                claim.Amt_Paid = claim.Amt_Paid + payment.Amount_Paid;
                                claim.Patient_Payment = claim.Patient_Payment + payment.Amount_Paid;
                                var tp = db.Claim_Payments.FirstOrDefault(p => p.claim_payments_id == payment.T_payment_id);
                                if (tp != null)
                                {
                                    List<CLAIM_NOTES> cLAIM_NOTEs = new List<CLAIM_NOTES>();
                                    cLAIM_NOTEs.Add(new CLAIM_NOTES
                                    {
                                        Claim_Notes_Id = Convert.ToInt64(db.SP_TableIdGenerator("Claim_Notes_Id").FirstOrDefault().ToString()),
                                        Claim_No = tp.Claim_No,
                                        IsAuto_Note = true,
                                        Note_Detail = $"Patient Credit amount $-{payment.Amount_Paid} transferred to Claim Number: {payment.Claim_No}",
                                        Created_By = userId,
                                        Created_Date = DateTime.Now
                                    });

                                    cLAIM_NOTEs.Add(new CLAIM_NOTES
                                    {
                                        Claim_Notes_Id = Convert.ToInt64(db.SP_TableIdGenerator("Claim_Notes_Id").FirstOrDefault().ToString()),
                                        Claim_No = payment.Claim_No,
                                        IsAuto_Note = true,
                                        Note_Detail = $"Patient Credit amount ${payment.Amount_Paid} received from Claim Number: {tp.Claim_No}",
                                        Created_By = userId,
                                        Created_Date = DateTime.Now
                                    });
                                    db.CLAIM_NOTES.AddRange(cLAIM_NOTEs);
                                }
                               
                            }
                          
                            db.Claim_Payments.Add(cp);
                        }
                    }

                    db.SaveChanges();

                    objResponse.Status = "Success";
                    objResponse.Response = new { Message = "Claim payments inserted/updated successfully." };
                }
                catch (Exception ex)
                {
                    objResponse.Status = "Failed";
                    objResponse.Response = new { Message = ex.Message };
                }
            }

            return objResponse;
        }









    }
}
