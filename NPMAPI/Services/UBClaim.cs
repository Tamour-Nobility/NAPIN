using Microsoft.AspNetCore.Mvc;
using NPMAPI.Models;
using NPMAPI.Models.InboxHealth;
using NPMAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Data.SqlClient;
using System.Dynamic;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Ajax.Utilities;
using System.Reflection.Emit;
using System.Security;
using System.Security.Claims;
using Claim = NPMAPI.Models.Claim;
using EdiFabric.Core.Model.Edi.X12;

namespace NPMAPI.Services
{
    public class UBClaim : IUBClaim
    {

        private readonly IFTP ftp;
        private readonly IPracticeRepository practiceRepository;
        private IEligibility _iEligibility;
        private IDeltaSyncRepository _deltaSyncRepository;
        private readonly IScrubberRepository _scrubberService;
        private readonly DemographicService _demographicService;


        public UBClaim(DemographicService demographicService, IFTP _ftp, IPracticeRepository _practiceRepository, IEligibility iEligibility, IDeltaSyncRepository deltaSyncRepository, IScrubberRepository scrubberRepository, IScrubberRepository scrubberService)
        {
            ftp = _ftp;
            practiceRepository = _practiceRepository;
            _iEligibility = iEligibility;
            _deltaSyncRepository = deltaSyncRepository;
            _scrubberService = scrubberService;
            _demographicService = demographicService;


        }

       

        public async Task<ResponseModel> GetAllCodesData()
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {

                CombinedCodeModel combinedData = new CombinedCodeModel();
                combinedData.occurrence_Span_Codes_List = new List<Occurrence_Span_Codes>();
                combinedData.condition_Codes_List = new List<Condition_Codes>();
                combinedData.occurrence_Codes_List = new List<Occurrence_Codes>();
                combinedData.Value_CodesList = new List<Value_Codes>();
                using (var ctx = new NPMDBEntities())
                {

                    combinedData.condition_Codes_List = await ctx.Condition_Codes.ToListAsync();
                    combinedData.occurrence_Codes_List = await ctx.Occurrence_Codes.ToListAsync();
                    combinedData.occurrence_Span_Codes_List = await ctx.Occurrence_Span_Codes.ToListAsync();
                    combinedData.Value_CodesList = await ctx.Value_Codes.ToListAsync();

                }

                if (combinedData != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = combinedData;
                }

            }

            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;

        }

        //public async Task<ResponseModel> ManageOccurenceSpan(List<ClaimsOccurrenceCode> OSClist)
        //{
        //    ResponseModel objResponse = new ResponseModel();
        //    try

        //    {
        //        using (var ctx = new NPMDBEntities())
        //        {


        //        }


        //    }
        //    catch
        //    {

        //    }

        //    }

        public async Task<ResponseModel> ManageOccurenceSpan(List<Claims_Occurrence_Code> OSClist, long userid)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    foreach (var occCode in OSClist)
                    {
                        if (occCode.Claims_Occurrence_Code_Id == 0)
                        {
                            occCode.Created_By = userid;
                            occCode.Modified_By = null;
                            occCode.Modified_Date = DateTime.Now;
                            occCode.Created_Date = DateTime.Now;


                            ctx.Claims_Occurrence_Code.Add(occCode);
                        }
                        else
                        {
                            var existingRecord = await ctx.Claims_Occurrence_Code.FirstOrDefaultAsync(o => o.Claims_Occurrence_Code_Id == occCode.Claims_Occurrence_Code_Id);

                            if (existingRecord != null)
                            {
                                existingRecord.OccCode = occCode.OccCode;
                                existingRecord.Descriptions = occCode.Descriptions;
                                existingRecord.Created_By = userid;
                                existingRecord.Modified_By = null;
                                existingRecord.Modified_Date = DateTime.Now;
                                existingRecord.Created_Date = DateTime.Now;
                            }
                        }
                    }

                    await ctx.SaveChangesAsync();
                }

                objResponse.Response = "Data added/edited successfully.";
                objResponse.Status = "Success";
            }
            catch (Exception ex)
            {
                objResponse.Response = "Error: " + ex.Message;
                objResponse.Status = "Error";
            }

            return objResponse;
        }

        public async Task<ResponseModel> GetallDropdowns()
        {



            ResponseModel objResponse = new ResponseModel();
            try
            {

                UbDrodowns ubdrodowns = new UbDrodowns();

                ubdrodowns.Type_Of_AdmissionList = new List<Type_Of_Admission>();
                ubdrodowns.Type_of_facilityList = new List<Type_of_facility>();
                ubdrodowns.Type_of_CareList = new List<Type_of_Care>();
                ubdrodowns.sequence_of_careList = new List<sequence_of_care>();
                ubdrodowns.Source_of_AdmissionList = new List<Source_of_Admission>();
                ubdrodowns.Discharge_StatusList = new List<Discharge_Status>();

                ubdrodowns = new UbDrodowns();
                using (var ctx = new NPMDBEntities())
                {
                    ubdrodowns.Type_Of_AdmissionList = await ctx.Type_Of_Admission.ToListAsync();
                    ubdrodowns.Type_of_facilityList = await ctx.Type_of_facility.ToListAsync();
                    ubdrodowns.Type_of_CareList = await ctx.Type_of_Care.ToListAsync();
                    ubdrodowns.sequence_of_careList = await ctx.sequence_of_care.ToListAsync();
                    ubdrodowns.Source_of_AdmissionList = await ctx.Source_of_Admission.ToListAsync();
                    ubdrodowns.Discharge_StatusList = await ctx.Discharge_Status.ToListAsync();

                }
                if (ubdrodowns != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = ubdrodowns;
                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
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

        public async Task<ResponseModel> SaveUBClaim(ClaimsViewModel cr, long userId)
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
                    //auto update status 'pending' to 'completed'
             
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

                    //...
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
                            c.Deleted = false;
                            c.Claim_Type = cr.ClaimModel.Claim_Type;
                            if (!string.IsNullOrEmpty(cr.ClaimModel.BATCH_NO))
                                c.BATCH_NO = cr.ClaimModel.BATCH_NO;
                            if (!string.IsNullOrEmpty(cr.ClaimModel.Batch_Date.ToString()))
                                c.Batch_Date = DateTime.Parse(cr.ClaimModel.Batch_Date.ToString());
                            if (!string.IsNullOrEmpty(cr.ClaimModel.DOS.ToString()))
                                c.DOS = DateTime.Parse(cr.ClaimModel.DOS.ToString());
                            c.Bill_Date = DateTime.Now;
                            c.Location_Code = cr.ClaimModel.Location_Code;
                            c.Attending_Physician = cr.ClaimModel.Attending_Physician;
                            c.Created_By = userId;
                            c.Created_Date = DateTime.Now;
                            c.Scan_Date = cr.ClaimModel.Scan_Date;
                            c.Billing_Physician = cr.ClaimModel.Billing_Physician;
                            c.Referring_Physician = cr.ClaimModel.Referring_Physician;
                            c.Supervising_Physician = cr.ClaimModel.Supervising_Physician;
                            c.Referral_Number = cr.ClaimModel.Referral_Number;
                            c.PA_Number = cr.ClaimModel.PA_Number;
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


                            #region ub claim



                            if (cr.Admission_Details != null)
                            {
                                long Id = Convert.ToInt64(db.SP_TableIdGenerator("Admission_Details_Id").FirstOrDefault());
                                db.Admission_Details.Add(new Admission_Details()
                                {
                                    Admission_Details_Id = Id,
                                    Type_Of_Admission_Id = cr.Admission_Details.Type_Of_Admission_Id,
                                    Admhour = cr.Admission_Details.Admhour,
                                    AdmSource = cr.Admission_Details.AdmSource,
                                    Type_of_Bill = cr.Admission_Details.Type_of_Bill,
                                    Dischargehour = cr.Admission_Details.Dischargehour,
                                    Discharge_status_Id = cr.Admission_Details.Discharge_status_Id,
                                    Practice_code = c.practice_code,
                                    Modified_By = null,
                                    Modified_Date = null,
                                    Created_Date = DateTime.Now,
                                    Claim_No = c.Claim_No,
                                    Created_By = userId,


                                });

                            }

                            if (cr.UbClaimDropdown == null)
                                {
                                    Console.WriteLine("hello");
                                }
                                else
                            {
                                    if (cr.UbClaimDropdown.OccCode.Count > 0)
                                    {
                                        foreach (var occurrenceCode in cr.UbClaimDropdown.OccCode)
                                        {
                                            //ResponseModel objResponse = new ResponseModel();
                                            long OCID = Convert.ToInt64(db.SP_TableIdGenerator("Claims_Occurrence_Code_Id").FirstOrDefault());
                                        db.Claims_Occurrence_Code.Add(new Claims_Occurrence_Code()
                                            {
                                                Claims_Occurrence_Code_Id = OCID,
                                                Claim_no = c.Claim_No,
                                                OccCode = occurrenceCode.OccCode,
                                                Date = DateTime.Parse(occurrenceCode.Date2),
                                                Created_By = userId,
                                                Descriptions = "",
                                                Practice_Code = c.practice_code,
                                                Modified_By = null,
                                                Modified_Date = null,
                                                Created_Date = DateTime.Now,
                                                Isdeleted = false, //occurrenceCode.Isdeleted,

                                            });

                                            //if (ctx.SaveChanges() > 0)
                                            //{
                                            //    objResponse.Status = "Success";
                                            //}
                                            //else
                                            //{
                                            //    objResponse.Status = "Occ Code Error";
                                            //}

                                        }
                                    }
                                    if (cr.UbClaimDropdown.CcOde.Count > 0)
                                    {
                                        foreach (var conditionCode in cr.UbClaimDropdown.CcOde)
                                        {

                                            //ResponseModel objResponse2 = new ResponseModel();
                                            var CCID = Convert.ToInt64(db.SP_TableIdGenerator("Claims_Condition_Code_Id").FirstOrDefault().ToString());
                                        db.Claims_Condition_Code.Add(new Claims_Condition_Code()
                                            {
                                                Claims_Condition_Code_Id = CCID,
                                                Claim_No = c.Claim_No,
                                                ConditionCode = conditionCode.ConditionCode,
                                                Date = DateTime.UtcNow,
                                                 Created_By = userId,
                                                Practice_Code = c.practice_code,
                                                Modified_By = null,
                                                Modified_Date = null,
                                                Created_Date = DateTime.Now,
                                                Isdeleted = false,

                                            });
                                        }

                                    }
                                    if (cr.UbClaimDropdown.OccSpanCode.Count > 0)
                                    {
                                        foreach (var occurenceSpanCode in cr.UbClaimDropdown.OccSpanCode)
                                        {

                                            // ResponseModel objResponse3 = new ResponseModel();
                                            var OSCID = Convert.ToInt64(db.SP_TableIdGenerator("Claims_Occurence_Span_Code_Id").FirstOrDefault().ToString());

                                        db.Claims_Occurence_Span_Code.Add(new Claims_Occurence_Span_Code()
                                            {
                                                Claims_Occurence_Span_Code_Id = OSCID,
                                                ClaimNo = c.Claim_No,
                                                Descriptions = "",
                                                 Created_By = userId,
                                                Practice_Code = c.practice_code,
                                                OccSpanCode = occurenceSpanCode.OccSpanCode,
                                             DateFrom = DateTime.Parse(occurenceSpanCode.DateFrom),
                                             DateThrough = DateTime.Parse(occurenceSpanCode.DateThrough),
                                            Modified_By = null,
                                                Modified_Date = null,
                                                Created_Date = DateTime.Now,
                                                Isdeleted=false,

                                            });
                                        }
                                    }
                                    if (cr.UbClaimDropdown.ValueCode.Count > 0)
                                    {
                                        foreach (var valueCode in cr.UbClaimDropdown.ValueCode)
                                        {

                                            // ResponseModel objResponse3 = new ResponseModel();
                                            var VCID = Convert.ToInt64(db.SP_TableIdGenerator("Claims_Value_Code_Id").FirstOrDefault().ToString());

                                        db.Claims_Value_Code.Add(new Claims_Value_Code()
                                            {
                                                Claims_Value_Code_Id = VCID,
                                                Claim_No = c.Claim_No,
                                                 Created_By = userId ,
                                                Practice_Code = c.practice_code,
                                                Value_Codes_Id = valueCode.Value_Codes_Id,
                                                Amount = valueCode.Amount,
                                                Modified_By = null,
                                                Modified_Date = null,
                                                Created_Date = DateTime.Now,
                                                Isdeleted=false,

                                            });

                                        }
                                    }
                                }


                            

                              

                            
                          

                            #endregion

                          //  await SaveDropdownsData(cr.UbClaimDropdown, c.Claim_No, cr.PracticeCode);

                            //if (claimType == "I")
                            //{
                            //    for (var i = 0; i < cr.UbClaimDropdown.CcOde.Count; i++)
                            //    {
                            //        if (cr.UbClaimDropdown.CcOde[i].Date == null)
                            //        {
                            //            cr.UbClaimDropdown.CcOde[i].Date = DateTime.UtcNow;
                            //        }
                            //    }

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
                        //auto update status 'pending' to 'completed'
                       
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
                        //...
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
                                    cn.Created_By = userId;
                                    cn.Created_Date = DateTime.Now;
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
                                if (UpdateClaim.ClaimModel.Bill_Date != null)
                                    originalclaim.Bill_Date = UpdateClaim.ClaimModel.Bill_Date;
                                if (UpdateClaim.ClaimModel.Location_Code != null)
                                    originalclaim.Location_Code = UpdateClaim.ClaimModel.Location_Code;
                                if (UpdateClaim.ClaimModel.Attending_Physician != null)
                                    originalclaim.Attending_Physician = UpdateClaim.ClaimModel.Attending_Physician;
                                if (UpdateClaim.ClaimModel.Billing_Physician != null)
                                    originalclaim.Billing_Physician = UpdateClaim.ClaimModel.Billing_Physician;
                                originalclaim.Referring_Physician = UpdateClaim.ClaimModel.Referring_Physician;
                                originalclaim.Supervising_Physician = UpdateClaim.ClaimModel.Supervising_Physician;
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
                                            if (UpdateClaim.claimPayments[i].claimPayments.Payment_Source == "P" || UpdateClaim.claimPayments[i].claimPayments.Payment_Source == "C" || UpdateClaim.claimPayments[i].claimPayments.Payment_Source == "I")
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
                                        SyncedClaimCharge syncedClaimCharge = db.SyncedClaimCharges.Where(c => c.claim_charges_id == item.claim_charges_id).FirstOrDefault<SyncedClaimCharge>();
                                        if (syncedClaimCharge != null)
                                        {
                                            if (UpdateClaim.claimPayments != null)
                                            {
                                                foreach (var Payment in UpdateClaim.claimPayments)
                                                {
                                                    if (Payment.claimPayments.Charged_Proc_Code != null && cc.Procedure_Code != null)
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
                                            UpdateSyncedClaimCharge(syncedClaimCharge, (long)invoiceId, Covered, ins_Covered, db);
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
                                foreach (var cPaymentItem in UpdateClaim.claimPayments)
                                {
                                    var item = cPaymentItem.claimPayments;
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
                                            cp.Date_Entry = DateTime.Now;
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
                                            if (item.Date_Entry == null)
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
                                        SyncedClaimPayment syncedClaimPayment = db.SyncedClaimPayments.Where(c => c.claim_payments_id == cp.claim_payments_id).FirstOrDefault();
                                        if (syncedClaimPayment != null)
                                        {
                                            syncedClaimPayment.UpdatedDate = DateTime.Now;
                                            //db.SaveChanges();
                                            //Update the Payment in Inbox Health if the Practice is Synced 
                                            UpdateSyncedClaimPayment(syncedClaimPayment, db);
                                        }
                                    }
                                    db.SaveChanges();
                                }
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


                            #region ub claim

                            if (cr.Admission_Details != null)
                            {


                                var existingAdmissionDetails = db.Admission_Details.Find(cr.Admission_Details.Id);

                                if (existingAdmissionDetails != null)
                                {
                                    // Update the existing record with new data
                                    existingAdmissionDetails.Type_Of_Admission_Id = cr.Admission_Details.Type_Of_Admission_Id;
                                    existingAdmissionDetails.Admhour = cr.Admission_Details.Admhour;
                                    existingAdmissionDetails.AdmSource = cr.Admission_Details.AdmSource;
                                    existingAdmissionDetails.Type_of_Bill = cr.Admission_Details.Type_of_Bill;
                                    existingAdmissionDetails.Dischargehour = cr.Admission_Details.Dischargehour;
                                    existingAdmissionDetails.Discharge_status_Id = cr.Admission_Details.Discharge_status_Id;
                                    existingAdmissionDetails.Practice_code = cr.PracticeCode;
                                    existingAdmissionDetails.Modified_By = null;
                                   existingAdmissionDetails.Modified_Date = null;
                                   existingAdmissionDetails.Created_Date = DateTime.Now;
                                    existingAdmissionDetails.Claim_No = cr.ClaimModel.Claim_No;
                                   existingAdmissionDetails.Created_By = userId;

                                  

                                    db.SaveChanges(); // Commit changes to the database
                                }
                            }


                            if (UpdateClaim.UbClaimDropdown == null)
                            {
                                Console.WriteLine("hello");
                            }
                            else
                            {
                                if (UpdateClaim.UbClaimDropdown.OccCode.Count > 0)
                                {
                                    //...delete the occurrence codes from db 
                                    deleteOccurenceCode(cr.ClaimModel.Claim_No, UpdateClaim.UbClaimDropdown.OccCode, userId, db);
                                    foreach (var occurrenceCode in UpdateClaim.UbClaimDropdown.OccCode)
                                    {
                                        long OCID = 0;
                                        if (occurrenceCode.OCID == 0)
                                        {
                                            OCID = Convert.ToInt64(db.SP_TableIdGenerator("Claims_Occurrence_Code_Id").FirstOrDefault());

                                            db.Claims_Occurrence_Code.Add(new Claims_Occurrence_Code()
                                            {
                                                Claims_Occurrence_Code_Id = OCID,
                                                Claim_no = cr.ClaimModel.Claim_No,
                                                OccCode = occurrenceCode.OccCode,
                                                Date =  DateTime.Parse(occurrenceCode.Date2),
                                                Created_By = userId,
                                                Descriptions = "",
                                                Practice_Code = cr.PracticeCode,
                                                Modified_By = null,
                                                Modified_Date = null,
                                                Created_Date = DateTime.Now,
                                                Isdeleted = false, //occurrenceCode.Isdeleted,
                                            });
                                        }
                                        else
                                        {
                                            OCID = Convert.ToInt64(occurrenceCode.OCID);

                                            var existingOccurrenceCode = db.Claims_Occurrence_Code.Find(OCID);

                                            if (existingOccurrenceCode != null)
                                            {
                                                // Update the existing record's properties
                                                existingOccurrenceCode.Claim_no = cr.ClaimModel.Claim_No;
                                                existingOccurrenceCode.OccCode = occurrenceCode.OccCode;
                                                existingOccurrenceCode.Date = DateTime.Parse(occurrenceCode.Date2);
                                                //existingOccurrenceCode.Created_By = userId;
                                                existingOccurrenceCode.Descriptions = "";
                                                existingOccurrenceCode.Practice_Code = cr.PracticeCode;
                                                existingOccurrenceCode.Modified_By = userId;
                                                existingOccurrenceCode.Modified_Date = DateTime.Now;
                                                //existingOccurrenceCode.Created_Date = DateTime.Now;
                                                existingOccurrenceCode.Isdeleted = false;  //occurrenceCode.Isdeleted;
                                            }
                                        }
                                    }
                                    db.SaveChanges(); // Commit changes to the database
                                }else
                                {
                                    //...delete the occurrence codes from db 
                                    deleteOccurenceCode(cr.ClaimModel.Claim_No, UpdateClaim.UbClaimDropdown.OccCode, userId, db);
                                }

                                if (UpdateClaim.UbClaimDropdown.CcOde.Count > 0)
                                {
                                    //...delete the Condition codes from db 
                                    deleteConditionCode(cr.ClaimModel.Claim_No, UpdateClaim.UbClaimDropdown.CcOde, userId, db);

                                    foreach (var conditionCode in UpdateClaim.UbClaimDropdown.CcOde)
                                    {
                                        long CCID = 0;
                                        //ResponseModel objResponse2 = new ResponseModel();
                                        if (conditionCode.CCID == 0)
                                        {
                                            CCID = Convert.ToInt64(db.SP_TableIdGenerator("Claims_Condition_Code_Id").FirstOrDefault().ToString());
                                            db.Claims_Condition_Code.Add(new Claims_Condition_Code()
                                            {
                                                Claims_Condition_Code_Id = CCID,
                                                Claim_No = cr.ClaimModel.Claim_No,
                                                ConditionCode = conditionCode.ConditionCode,
                                                Date = DateTime.UtcNow,
                                                Created_By = userId,
                                                Practice_Code = cr.PracticeCode,
                                                Modified_By = null,
                                                Modified_Date = null,
                                                Created_Date = DateTime.Now,
                                                Isdeleted = false,

                                            });
                                        }
                                        else
                                        {
                                            var existingConditionCode = db.Claims_Condition_Code.Find(conditionCode.CCID);

                                            if (existingConditionCode != null)
                                            {
                                                // Update the existing record's properties
                                                existingConditionCode.Claim_No = cr.ClaimModel.Claim_No;
                                                existingConditionCode.ConditionCode = conditionCode.ConditionCode;
                                                existingConditionCode.Date = DateTime.UtcNow;
                                                //existingConditionCode.Created_By = userId;
                                                existingConditionCode.Practice_Code = cr.PracticeCode;
                                                existingConditionCode.Modified_By = userId;
                                                existingConditionCode.Modified_Date = DateTime.Now;
                                               // existingConditionCode.Created_Date = DateTime.Now;
                                                existingConditionCode.Isdeleted = false;
                                            }
                                        }


                                    }
                                    db.SaveChanges();
                                }
                                else
                                {
                                    //...delete the Condition codes from db 
                                    deleteConditionCode(cr.ClaimModel.Claim_No, UpdateClaim.UbClaimDropdown.CcOde, userId, db);
                                }
                                if (UpdateClaim.UbClaimDropdown.OccSpanCode.Count > 0)
                                {
                                    //...delete the occurrence span codes from db 
                                    deleteOccurenceSpan(cr.ClaimModel.Claim_No, UpdateClaim.UbClaimDropdown.OccSpanCode, userId, db);

                                    foreach (var occurenceSpanCode in UpdateClaim.UbClaimDropdown.OccSpanCode)
                                    {
                                        long OSCID = 0;
                                        if (occurenceSpanCode.OSCID == 0)
                                        {
                                            OSCID = Convert.ToInt64(db.SP_TableIdGenerator("Claims_Occurence_Span_Code_Id").FirstOrDefault().ToString());

                                            db.Claims_Occurence_Span_Code.Add(new Claims_Occurence_Span_Code()
                                            {
                                                Claims_Occurence_Span_Code_Id = OSCID,
                                                ClaimNo = cr.ClaimModel.Claim_No,
                                                Descriptions = "",
                                                Created_By = userId,
                                                Practice_Code = cr.PracticeCode,
                                                OccSpanCode = occurenceSpanCode.OccSpanCode,
                                                DateFrom = DateTime.Parse(occurenceSpanCode.DateFrom),
                                                DateThrough = DateTime.Parse(occurenceSpanCode.DateThrough),
                                                Modified_By = null,
                                                Modified_Date = null,
                                                Created_Date = DateTime.Now,
                                                Isdeleted = false,
                                            });
                                        }
                                        else
                                        {
                                            var existingOccurenceSpanCode = db.Claims_Occurence_Span_Code.Find(occurenceSpanCode.OSCID);

                                            if (existingOccurenceSpanCode != null)
                                            {
                                                existingOccurenceSpanCode.ClaimNo = cr.ClaimModel.Claim_No;
                                                existingOccurenceSpanCode.OccSpanCode = occurenceSpanCode.OccSpanCode;
                                                existingOccurenceSpanCode.DateFrom = DateTime.Parse(occurenceSpanCode.DateFrom);
                                                existingOccurenceSpanCode.DateThrough = DateTime.Parse(occurenceSpanCode.DateThrough);
                                                //existingOccurenceSpanCode.Created_By = userId;
                                                existingOccurenceSpanCode.Practice_Code = cr.PracticeCode;
                                                existingOccurenceSpanCode.Modified_By = userId;
                                                existingOccurenceSpanCode.Modified_Date = DateTime.Now; ;
                                                //existingOccurenceSpanCode.Created_Date = DateTime.Now;
                                                existingOccurenceSpanCode.Isdeleted = false;
                                            }
                                        }
                                    }
                                    db.SaveChanges(); // Commit changes to the database
                                }
                                else
                                {
                                    //...delete the occurrence span codes from db 
                                    deleteOccurenceSpan(cr.ClaimModel.Claim_No, UpdateClaim.UbClaimDropdown.OccSpanCode, userId, db);

                                }
                                if (UpdateClaim.UbClaimDropdown.ValueCode.Count > 0)
                                {
                                    //...delete the value codes from db 
                                    deleteValueCode(cr.ClaimModel.Claim_No, UpdateClaim.UbClaimDropdown.ValueCode, userId, db);
                                    foreach (var valueCode in UpdateClaim.UbClaimDropdown.ValueCode)
                                    {
                                        long VCID = 0;
                                        if (valueCode.VCID == 0)
                                        {
                                            VCID = Convert.ToInt64(db.SP_TableIdGenerator("Claims_Value_Code_Id").FirstOrDefault().ToString());

                                            db.Claims_Value_Code.Add(new Claims_Value_Code()
                                            {
                                                Claims_Value_Code_Id = VCID,
                                                Claim_No = cr.ClaimModel.Claim_No,
                                                Created_By = userId,
                                                Practice_Code = cr.PracticeCode,
                                                Value_Codes_Id = valueCode.Value_Codes_Id,
                                                Amount = valueCode.Amount,
                                                Modified_By = null,
                                                Modified_Date = null,
                                                Created_Date = DateTime.Now,
                                                Isdeleted = false,
                                            });
                                        }
                                        else
                                        {
                                            var existingValueCode = db.Claims_Value_Code.Find(valueCode.VCID);
                                            if (existingValueCode != null)
                                            {
                                                // Update the existing record's properties
                                                existingValueCode.Claim_No = cr.ClaimModel.Claim_No;
                                                existingValueCode.Value_Codes_Id = valueCode.Value_Codes_Id;
                                                existingValueCode.Amount = valueCode.Amount;
                                                //existingValueCode.Created_By = userId;
                                                existingValueCode.Practice_Code = cr.PracticeCode;
                                                existingValueCode.Modified_By = userId;
                                                existingValueCode.Modified_Date = DateTime.Now;
                                                //existingValueCode.Created_Date = DateTime.Now;
                                                existingValueCode.Isdeleted = false;
                                            }
                                        }
                                    }
                                    db.SaveChanges(); // Commit changes to the database
                                   
                                }
                                else
                                {
                                    //...delete the value codes from db 
                                    deleteValueCode(cr.ClaimModel.Claim_No, UpdateClaim.UbClaimDropdown.ValueCode, userId, db);
                                }


                            }
                            









                            #endregion




                            db.Database.CurrentTransaction.Commit();
                            objResponse.Status = "Sucess";
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

        //..below method is for deleting value codes against institutional claims in update case...
        private void deleteValueCode(long claimNo,List<ValueeCode> newValueCodes,long userId, NPMDBEntities db)
        {
            try
            {
                var claimValues = db.Claims_Value_Code.Where(c => c.Claim_No == claimNo && c.Isdeleted==false).ToList();
                newValueCodes = newValueCodes.Where(c => c.VCID != 0).ToList();
                if (claimValues.Count > 0)
                {
                    foreach (var valueCode in claimValues)
                    {
                        var ifExist = newValueCodes.Where(x => x.VCID == valueCode.Claims_Value_Code_Id && x.VCID!=0).FirstOrDefault();
                        if (ifExist == null)
                        {
                            var existingValueCode = db.Claims_Value_Code.Find(valueCode.Claims_Value_Code_Id);
                            if (existingValueCode != null)
                            {
                                // delete the existing record's
                                existingValueCode.Isdeleted = true;
                                existingValueCode.Modified_By = userId;
                                existingValueCode.Modified_Date = DateTime.Now;
                            }
                        }
                    }
                    db.SaveChanges(); // Commit changes to the database
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //..below method is for deleting Occurrence codes against institutional claims in update case...
        private void deleteOccurenceCode(long claimNo, List<OccurrenceCodeModel> newOccurrenceCodes, long userId, NPMDBEntities db)
        {
            try
            {
                var claimValues = db.Claims_Occurrence_Code.Where(c => c.Claim_no == claimNo && c.Isdeleted == false).ToList();
                newOccurrenceCodes = newOccurrenceCodes.Where(c => c.OCID != 0).ToList();
                if (claimValues.Count > 0)
                {
                    foreach (var valueCode in claimValues)
                    {
                        var ifExist = newOccurrenceCodes.Where(x => x.OCID == valueCode.Claims_Occurrence_Code_Id && x.OCID != 0).FirstOrDefault();
                        if (ifExist == null)
                        {
                            var existingValueCode = db.Claims_Occurrence_Code.Find(valueCode.Claims_Occurrence_Code_Id);
                            if (existingValueCode != null)
                            {
                                // delete the existing record's
                                existingValueCode.Isdeleted = true;
                                existingValueCode.Modified_By = userId;
                                existingValueCode.Modified_Date = DateTime.Now;
                            }
                        }
                    }
                    db.SaveChanges(); // Commit changes to the database
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //..below method is for deleting Occurrence Spans against institutional claims in update case...
        private void deleteOccurenceSpan(long claimNo, List<OccurenceSpanModel> newOccurrenceSpan, long userId, NPMDBEntities db)
        {
            try
            {
                var claimValues = db.Claims_Occurence_Span_Code.Where(c => c.ClaimNo == claimNo && c.Isdeleted == false).ToList();
                newOccurrenceSpan = newOccurrenceSpan.Where(c => c.OSCID != 0).ToList();
                if (claimValues.Count > 0)
                {
                    foreach (var valueCode in claimValues)
                    {
                        var ifExist = newOccurrenceSpan.Where(x => x.OSCID == valueCode.Claims_Occurence_Span_Code_Id && x.OSCID != 0).FirstOrDefault();
                        if (ifExist == null)
                        {
                            var existingValueCode = db.Claims_Occurence_Span_Code.Find(valueCode.Claims_Occurence_Span_Code_Id);
                            if (existingValueCode != null)
                            {
                                // delete the existing record's
                                existingValueCode.Isdeleted = true;
                                existingValueCode.Modified_By = userId;
                                existingValueCode.Modified_Date = DateTime.Now;
                            }
                        }
                    }
                    db.SaveChanges(); // Commit changes to the database
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
       
        //..below method is for deleting Condition Codes against institutional claims in update case...
        private void deleteConditionCode(long claimNo, List<ConditionCodeModel> newConditionCode, long userId, NPMDBEntities db)
        {
            try
            {
                var claimValues = db.Claims_Condition_Code.Where(c => c.Claim_No == claimNo && c.Isdeleted == false).ToList();
                newConditionCode = newConditionCode.Where(c => c.CCID != 0).ToList();
                if (claimValues.Count > 0)
                {
                    foreach (var valueCode in claimValues)
                    {
                        var ifExist = newConditionCode.Where(x => x.CCID == valueCode.Claims_Condition_Code_Id && x.CCID != 0).FirstOrDefault();
                        if (ifExist == null)
                        {
                            var existingValueCode = db.Claims_Condition_Code.Find(valueCode.Claims_Condition_Code_Id);
                            if (existingValueCode != null)
                            {
                                // delete the existing record's
                                existingValueCode.Isdeleted = true;
                                existingValueCode.Modified_By = userId;
                                existingValueCode.Modified_Date = DateTime.Now;
                            }
                        }
                    }
                    db.SaveChanges(); // Commit changes to the database
                }
            }
            catch (Exception)
            {
                throw;
            }
        }



        //private async Task<ResponseModel> SaveDropdownsData(UbClaimDropdowns ubClaimDropdowns , long prac_code , long claim_no)
        //{



        //        ResponseModel objResponse = new ResponseModel();
        //    try
        //    {
        //        if (ubClaimDropdowns == null)
        //        {
        //            Console.WriteLine("hello");
        //        }

        //        using (var ctx = new NPMDBEntities())
        //        {

        //            if (ubClaimDropdowns.OccCode.Count > 0)
        //            {
        //                foreach (var occurrenceCode in ubClaimDropdowns.OccCode)
        //                {

        //                    //ResponseModel objResponse = new ResponseModel();
        //                     long iClaimNo = Convert.ToInt64(ctx.SP_TableIdGenerator("Claim_No").FirstOrDefault().ToString());
        //                    long OCID = Convert.ToInt64(ctx.SP_TableIdGenerator("Claims_Value_Code_Id").FirstOrDefault());
        //                    ctx.Claims_Occurrence_Code.Add(new Claims_Occurrence_Code()
        //                    {

        //                     Claims_Occurrence_Code_Id = OCID,
        //                        Claim_no = claim_no,
        //                        OccCode = occurrenceCode.OccCode,
        //                        Date = DateTime.TryParse(occurrenceCode.Date2, out DateTime parsedDate) ? (DateTime?)parsedDate : null,
        //                         Created_By = 35512032,
        //                        Descriptions = "Test",
        //                        Practice_Code = prac_code,
        //                        Modified_By = null,
        //                        Modified_Date = null,
        //                        Created_Date = DateTime.Now,
        //                        Isdeleted=occurrenceCode.Isdeleted,

        //                    });

        //                    if (ctx.SaveChanges() > 0)
        //                    {
        //                        objResponse.Status = "Success";
        //                    }
        //                    else
        //                    {
        //                        objResponse.Status = "Occ Code Error";
        //                    }

        //                }
        //            }             
        //            if(ubClaimDropdowns.CcOde.Count > 0)
        //            {
        //                foreach (var conditionCode in ubClaimDropdowns.CcOde)
        //                {

        //                    //ResponseModel objResponse2 = new ResponseModel();
        //                    var CCID = Convert.ToInt64(ctx.SP_TableIdGenerator("Claims_Condition_Code_Id").FirstOrDefault().ToString());
        //                    ctx.Claims_Condition_Code.Add(new Claims_Condition_Code()
        //                    {
        //                        Claims_Condition_Code_Id = CCID,
        //                        Claim_No = claim_no,
        //                        ConditionCode = conditionCode.ConditionCode,
        //                        Date = DateTime.UtcNow,
        //                        // Created_By = userid,
        //                        Practice_Code = prac_code,
        //                        Modified_By = null,
        //                        Modified_Date = null,
        //                        Created_Date = DateTime.Now,

        //                    });

        //                    if (ctx.SaveChanges() > 0)
        //                    {
        //                        objResponse.Status = "Success";
        //                    }
        //                    else
        //                    {
        //                        objResponse.Status = "Occ Code Error";
        //                    }

        //                }

        //            }
        //            if (ubClaimDropdowns.OccSpanCode.Count > 0)
        //            {
        //                foreach (var occurenceSpanCode in ubClaimDropdowns.OccSpanCode)
        //                {

        //                   // ResponseModel objResponse3 = new ResponseModel();
        //                    var OSCID = Convert.ToInt64(ctx.SP_TableIdGenerator("Claims_Occurence_Span_Code_Id").FirstOrDefault().ToString());

        //                    ctx.Claims_Occurence_Span_Code.Add(new Claims_Occurence_Span_Code()
        //                    {
        //                        Claims_Occurence_Span_Code_Id = OSCID,
        //                        ClaimNo = claim_no,
        //                        Descriptions = "Test",
        //                        // Created_By = userid,
        //                        Practice_Code = prac_code,
        //                        OccSpanCode = occurenceSpanCode.OccSpanCode,
        //                        //DateFrom = occurenceSpanCode.DateFrom,
        //                        //DateThrough = occurenceSpanCode.DateThrough,
        //                        Modified_By = null,
        //                        Modified_Date = null,
        //                        Created_Date = DateTime.Now,

        //                    });

        //                    if (ctx.SaveChanges() > 0)
        //                    {
        //                        objResponse.Status = "Success";
        //                    }
        //                    else
        //                    {
        //                        objResponse.Status = "Occ Code Error";
        //                    }



        //                }
        //            }
        //            if (ubClaimDropdowns.ValueCode.Count > 0)
        //            {
        //                foreach (var valueCode in ubClaimDropdowns.ValueCode)
        //                {

        //                   // ResponseModel objResponse3 = new ResponseModel();
        //                    var VCID = Convert.ToInt64(ctx.SP_TableIdGenerator("Claims_Value_Code_Id").FirstOrDefault().ToString());

        //                    ctx.Claims_Value_Code.Add(new Claims_Value_Code()
        //                    {
        //                        Claims_Value_Code_Id = VCID,
        //                        Claim_No = claim_no,
        //                        // Created_By = userid,
        //                        Practice_Code = prac_code,
        //                        Value_Codes_Id = valueCode.Value_Codes_Id,
        //                        Amount = valueCode.Amount,
        //                        Modified_By = null,
        //                        Modified_Date = null,
        //                        Created_Date = DateTime.Now,

        //                    });

        //                    if (ctx.SaveChanges() > 0)
        //                    {
        //                        objResponse.Status = "Success";
        //                    }
        //                    else
        //                    {
        //                        objResponse.Status = "Occ Code Error";
        //                    }


        //                }
        //            }


        //            await ctx.SaveChangesAsync();


        //            objResponse.Status = "Sucess";
        //            objResponse.Response = "Added Succesfully";

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        objResponse.Status = "Error";
        //        objResponse.Response = ex.Message;
        //    }
        //    return objResponse;
        //}

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
                        insurance_owed_amount_cents = 0,
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

     
    }


  

   
}


