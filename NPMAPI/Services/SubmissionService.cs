using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.UI.WebControls;
using Dapper;
using EdiFabric.Core.Model.Edi;
using EdiFabric.Framework.Readers;
using EdiFabric.Templates.Hipaa5010;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.SignalR.Hosting;
using Newtonsoft.Json;
using NLog.Fluent;
using NPMAPI.Enums;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;
using NPOI.OpenXmlFormats.Dml.Diagram;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Ocsp;
using PracticeSaopApi.ServiceReferenceCore;
using Renci.SshNet;
using static iTextSharp.awt.geom.Point2D;
using static NPMAPI.Controllers.SubmissionController;
using static NPOI.HSSF.Util.HSSFColor;

namespace NPMAPI.Services
{
    public class SubmissionService : ISubmissionRepository
    {
        private readonly IPracticeRepository _practiceService;
        public SubmissionService(IPracticeRepository practiceService)
        {
            _practiceService = practiceService;
        }

        DateTime? Date_Filing = null;
        List<decimal?> claimpayment = new List<decimal?>();
        string secStatus = "";
        public string GetFormattedDate()
        {

            return Date_Filing.HasValue ? Date_Filing.Value.ToString("yyyyMMdd") : "Date is null";

        }

        //.. below method secondary fixation along with primary done by abbas ahmed and asim mehmood.

        public ResponseModel GenerateBatch_5010_P_P(long practice_id, long claim_id)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                var pri_Status = "";
                var sec_Status = "";
                var oth_Status = "";
                var Claim_Type = "";
                using (var ctx = new NPMDBEntities())
                {
                    pri_Status = ctx.Claims.Where(c => c.Claim_No == claim_id)
                                       .Select(c => c.Pri_Status).FirstOrDefault();
                    sec_Status = ctx.Claims.Where(c => c.Claim_No == claim_id)
                                       .Select(c => c.Sec_Status).FirstOrDefault();
                    oth_Status = ctx.Claims.Where(c => c.Claim_No == claim_id)
                                       .Select(c => c.Oth_Status).FirstOrDefault();
                    Claim_Type = ctx.Claims.Where(c => c.Claim_No == claim_id)
                                       .Select(c => c.Claim_Type).FirstOrDefault();
                }


                    string strBatchString = "";
                    string strBatchStringR = "";
                    int segmentCount = 0;
                    List<string> errorList;

                    //string billingOrganizationName = "practiceName";//practiceName
                    string sumbitterId = "";
                    string submitterCompanyName = "";
                    string submitterContactPerson = "";
                    string submitterCompanyEmail = "";
                    string submitterCompanyPhone = "";
                    string batchId = "";

                    long InspayerId = 0;
                    string PayerNumber = "";

                    errorList = new List<string>();

                    List<spGetBatchCompanyDetails_Result> batchCompanyInfo = null;
                    List<spGetBatchClaimsInfo_Result> batchClaimInfo = null;
                    List<spGetBatchClaimsDiagnosis_Result> batchClaimDiagnosis = null;
                    List<spGetBatchClaimsProcedurestest_Result> batchClaimProcedures = null;
                    List<spGetBatchClaimsInsurancesInfo_Result> insuraceInfo = null;
                    List<SPDataModel> sPDataModels = null;
                    List<ClaimSubmissionModel> claimSubmissionInfo = new List<ClaimSubmissionModel>();

                    using (var ctx = new NPMDBEntities())
                    {
                        batchCompanyInfo = ctx.spGetBatchCompanyDetails(practice_id.ToString()).ToList();
                    }

                    if (batchCompanyInfo != null && batchCompanyInfo.Count > 0)
                    {
                        sumbitterId = batchCompanyInfo[0].Submitter_Id;
                        submitterCompanyName = batchCompanyInfo[0].Company_Name;
                        submitterContactPerson = batchCompanyInfo[0].Contact_Person;
                        submitterCompanyEmail = batchCompanyInfo[0].Company_Email;
                        submitterCompanyPhone = batchCompanyInfo[0].Company_Phone;
                    }

                    if (string.IsNullOrEmpty(sumbitterId))
                    {
                        errorList.Add("Patient Submitter ID is missing.");
                    }
                    if (string.IsNullOrEmpty(submitterCompanyName))
                    {
                        errorList.Add("Company ClearingHouse information is missing.");
                    }
                    if (string.IsNullOrEmpty(submitterCompanyEmail) && string.IsNullOrEmpty(submitterCompanyPhone))
                    {
                        errorList.Add("Submitter Contact Information is Missing.");
                    }
                    if (errorList.Count == 0)
                    {
                        using (var ctx = new NPMDBEntities())
                        {
                            batchClaimInfo = ctx.spGetBatchClaimsInfo(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                            batchClaimDiagnosis = ctx.spGetBatchClaimsDiagnosis(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                            batchClaimProcedures = ctx.spGetBatchClaimsProcedurestest(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                            insuraceInfo = ctx.spGetBatchClaimsInsurancesInfo(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                            sPDataModels = getSpResult(claim_id.ToString(), "P").ToList();
                        }

                        foreach (var claim in batchClaimInfo)
                        {

                            if (claim.Patient_Id == null)
                            {
                                errorList.Add("Patient identifier is missing. DOS:" + claim.Dos);
                            }
                            else if (claim.Billing_Physician == null)
                            {
                                errorList.Add("Billing Physician identifier is missing. DOS:" + claim.Dos);
                            }


                            IEnumerable<spGetBatchClaimsInsurancesInfo_Result> claimInsurances = (from ins in insuraceInfo
                                                                                                  where ins.Claim_No == claim.Claim_No
                                                                                                  select ins).ToList();

                            spGetBatchClaimsDiagnosis_Result claimDiagnosis = (from spGetBatchClaimsDiagnosis_Result diag in batchClaimDiagnosis
                                                                               where diag.Claim_No == claim.Claim_No
                                                                               select diag).FirstOrDefault();

                            IEnumerable<spGetBatchClaimsProcedurestest_Result> claimProcedures = (from spGetBatchClaimsProcedurestest_Result proc in batchClaimProcedures
                                                                                                  where proc.Claim_No == claim.Claim_No
                                                                                                  select proc).ToList();







                            ClaimSubmissionModel claimSubmissionModel = new ClaimSubmissionModel();
                            claimSubmissionModel.claim_No = claim.Claim_No;
                            claimSubmissionModel.claimInfo = claim;
                            claimSubmissionModel.claimInsurance = claimInsurances as List<spGetBatchClaimsInsurancesInfo_Result>;
                            claimSubmissionModel.claimDiagnosis = claimDiagnosis as spGetBatchClaimsDiagnosis_Result;
                            claimSubmissionModel.claimProcedures = claimProcedures as List<spGetBatchClaimsProcedurestest_Result>;



                            List<uspGetBatchClaimsProviderPayersDataFromUSP_Result> claimBillingProviderPayerInfo;
                            foreach (var ins in claimInsurances)
                            {
                                if (ins.Insurace_Type.Trim().ToUpper().Equals("P") && ins.Inspayer_Id != null)//primary
                                {

                                    using (var ctx = new NPMDBEntities())
                                    {
                                        claimBillingProviderPayerInfo = ctx.uspGetBatchClaimsProviderPayersDataFromUSP(ins.Inspayer_Id.ToString(), claim.Claim_No.ToString(), "CLAIM_ID").ToList();

                                        if (claimBillingProviderPayerInfo != null && claimBillingProviderPayerInfo.Count > 0)
                                        {
                                            claimSubmissionModel.claimBillingProviderPayer = claimBillingProviderPayerInfo[0];
                                        }
                                    }
                                    //..Below Condition is added by tamour dated 04/19/2024 to enforce in case of Medicare of texas to use SSN only
                                    InspayerId = ins.Inspayer_Id;
                                    if (!string.IsNullOrEmpty(ins.Payer_Number))
                                    {
                                        PayerNumber = ins.Payer_Number;
                                    }
                                    break;
                                }
                            }

                            /*
                             * Assign Other objects of hospital claim
                             *  
                             * 
                             * */
                            claimSubmissionInfo.Add(claimSubmissionModel);

                        }

                        if (claimSubmissionInfo.Count > 0)
                        {
                            //batchId = claimSubmissionInfo[0].claim_No.ToString(); // Temporariy ... will be populated by actual batch id.
                            string claimNumber = claimSubmissionInfo[0].claim_No.ToString();
                            batchId = claimNumber.Substring(3);

                            string dateTime_yyMMdd = DateTime.Now.ToString("yyMMdd");
                            string dateTime_yyyyMMdd = DateTime.Now.ToString("yyyyMMdd");
                            string dateTime_HHmm = DateTime.Now.ToString("HHmm");

                            // ISA02 Authorization Information AN 10 - 10 R
                            string authorizationInfo = string.Empty.PadRight(10);// 10 characters

                            //ISA04 Security Information AN 10-10 R
                            string securityInfo = string.Empty.PadRight(10);// 10 characters

                            segmentCount = 0;

                            #region ISA Header
                            // INTERCHANGE CONTROL HEADER
                            //Readable file Header
                            strBatchStringR = "~Interchange Control Header~";
                            strBatchStringR += "ISA*";
                            strBatchStringR += "00*" + authorizationInfo + "*00*" + securityInfo + "*ZZ*" + sumbitterId.PadRight(15) + "*ZZ*263923727000000*";
                            strBatchStringR += dateTime_yyMMdd + "*";
                            strBatchStringR += dateTime_HHmm + "*";
                            strBatchStringR += "^*00501*000000001*0*P*:~";

                            strBatchString = "ISA*";
                            strBatchString += "00*" + authorizationInfo + "*00*" + securityInfo + "*ZZ*" + sumbitterId.PadRight(15) + "*ZZ*263923727000000*";
                            strBatchString += dateTime_yyMMdd + "*";
                            strBatchString += dateTime_HHmm + "*";
                            strBatchString += "^*00501*000000001*0*P*:~";
                            segmentCount++;
                            //FUNCTIONAL GROUP HEADER
                            strBatchStringR += "Functional Group Header~";
                            strBatchStringR += "GS*HC*" + sumbitterId + "*263923727*";
                            strBatchStringR += dateTime_yyyyMMdd + "*";
                            strBatchStringR += dateTime_HHmm + "*";
                            strBatchStringR += batchId.ToString() + "*X*005010X222A1~";

                            strBatchString += "GS*HC*" + sumbitterId + "*263923727*";
                            strBatchString += dateTime_yyyyMMdd + "*";
                            strBatchString += dateTime_HHmm + "*";
                            strBatchString += batchId.ToString() + "*X*005010X222A1~";  //-->5010 GS08 Changed from 004010X098A1 to 005010X222 in 5010
                                                                                        // need to send batch_id in GS06 instead of 16290 so that can be traced from 997 response file
                            segmentCount++;
                            //TRANSACTION SET HEADER
                            strBatchStringR += "Transaction Set Header~";
                            strBatchStringR += "ST*837*0001*005010X222A1~";

                            strBatchString += "ST*837*0001*005010X222A1~";  //-->5010 new element addedd. ST03 Implementation Convention Reference (005010X222)
                            segmentCount++;
                            //BEGINNING OF HIERARCHICAL TRANSACTION
                            strBatchStringR += "Beginning of Hierarchical Transaction~";
                            strBatchStringR += "BHT*0019*00*000000001*";
                            strBatchStringR += dateTime_yyyyMMdd + "*";
                            strBatchStringR += dateTime_HHmm + "*";
                            strBatchStringR += "CH~";

                            strBatchString += "BHT*0019*00*000000001*";
                            strBatchString += dateTime_yyyyMMdd + "*";
                            strBatchString += dateTime_HHmm + "*";
                            strBatchString += "CH~";
                            segmentCount++;

                            #endregion

                            #region LOOP 1000A (Sumbitter Information)
                            strBatchStringR += "Loop ID - 1000A - Submitter Name~";

                            #region Submitter Company Name
                            strBatchStringR += "Submitter Name~";
                            strBatchStringR += "NM1*41*2*";  //-->5010 NM103  Increase from 35 - 60
                            strBatchStringR += submitterCompanyName; // -->5010 NM104  Increase from 25 - 35
                            strBatchStringR += "*****46*" + sumbitterId;// -->5010 New element added NM112 Name Last or Organization Name 1-60
                            strBatchStringR += "~";

                            strBatchString += "NM1*41*2*";  //-->5010 NM103  Increase from 35 - 60
                            strBatchString += submitterCompanyName; // -->5010 NM104  Increase from 25 - 35
                            strBatchString += "*****46*" + sumbitterId;// -->5010 New element added NM112 Name Last or Organization Name 1-60
                            strBatchString += "~";
                            segmentCount++;
                            #endregion

                            #region SUBMITTER EDI CONTACT INFORMATION
                            strBatchStringR += "Submitter Contact Information~";
                            strBatchStringR += "PER*IC*";

                            strBatchString += "PER*IC*";
                            if (!string.IsNullOrEmpty(submitterContactPerson))
                            {
                                strBatchStringR += submitterContactPerson;
                                strBatchString += submitterContactPerson;
                            }

                            if (!string.IsNullOrEmpty(submitterCompanyPhone))
                            {
                                strBatchStringR += "*TE*" + submitterCompanyPhone.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim();
                                strBatchString += "*TE*" + submitterCompanyPhone.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim();

                            }
                            if (!string.IsNullOrEmpty(submitterCompanyEmail))
                            {
                                strBatchStringR += "*EM*" + submitterCompanyEmail;
                                strBatchString += "*EM*" + submitterCompanyEmail;
                            }
                            strBatchStringR += "~";
                            strBatchString += "~";
                            segmentCount++;
                            #endregion

                            #endregion

                            #region LOOP 1000B (RECEIVER NAME)
                            strBatchStringR += "Loop ID - 1000B - Receiver Name~";
                            strBatchStringR += "Receiver Name~";
                            strBatchStringR += "NM1*40*2*263923727000000*****46*" + sumbitterId + "~";

                            strBatchString += "NM1*40*2*263923727000000*****46*" + sumbitterId + "~";
                            segmentCount++;
                            #endregion

                            int HL = 1;


                            foreach (var claim in claimSubmissionInfo)
                            {
                                long patientId = (long)claim.claimInfo.Patient_Id;
                                long claimId = claim.claimInfo.Claim_No;
                                string DOS = claim.claimInfo.Dos;
                                string patientName = claim.claimInfo.Lname + ", " + claim.claimInfo.Fname;

                                string paperPayerID = "";
                                string Billing_Provider_NPI = "";
                                string TaxonomyCode = "";
                                string FederalTaxID = "";
                                string FederalTaxIDType = "";
                                string SSN_Number = "";

                                string box_33_type = "";

                                #region Check If Payer Validation Expires
                                // check if payer validation expires

                                if (claim.claimBillingProviderPayer != null)
                                {
                                    if (string.IsNullOrEmpty(claim.claimBillingProviderPayer.Validation_Expiry_Date.ToString()) && claim.claimBillingProviderPayer.Validation_Expiry_Date.ToString() != "01/01/1900")
                                    {

                                        string validationExpriyDate = claim.claimBillingProviderPayer.Validation_Expiry_Date.ToString();
                                        DateTime dtExpiry = DateTime.Parse(validationExpriyDate);
                                        DateTime dtToday = new DateTime();

                                        if (DateTime.Compare(dtExpiry, dtToday) >= 0) // expires
                                        {
                                            errorList.Add("VALIDATION EXPIRED : Provider validation with the Payer has been expired.");
                                        }

                                    }
                                }
                                #endregion

                                #region Provider NPI/Group NPI on the basis of Box 33 Type . Group or Individual | Federal Tax ID | Box33                         
                                if (claim.claimBillingProviderPayer != null)
                                {
                                    if (!string.IsNullOrEmpty(claim.claimBillingProviderPayer.Provider_Identification_Number_Type)
                                        && !string.IsNullOrEmpty(claim.claimBillingProviderPayer.Provider_Identification_Number))
                                    {

                                        FederalTaxIDType = claim.claimBillingProviderPayer.Provider_Identification_Number_Type;
                                        FederalTaxID = claim.claimBillingProviderPayer.Provider_Identification_Number;
                                    }

                                    if (!string.IsNullOrEmpty(claim.claimBillingProviderPayer.Box_33_Type))
                                    {
                                        box_33_type = claim.claimBillingProviderPayer.Box_33_Type;
                                    }
                                }
                                if (string.IsNullOrEmpty(FederalTaxIDType) || string.IsNullOrEmpty(FederalTaxID) || string.IsNullOrEmpty(SSN_Number))
                                {
                                    FederalTaxIDType = claim.claimInfo.Federal_Taxidnumbertype;
                                    FederalTaxID = claim.claimInfo.Federal_Taxid;
                                    SSN_Number = claim.claimInfo.SSN;
                                }



                                if (string.IsNullOrEmpty(box_33_type))
                                {
                                    switch (FederalTaxIDType)
                                    {
                                        case "EIN": // Group
                                            box_33_type = "GROUP";
                                            break;
                                        case "SSN": // Individual
                                            box_33_type = "INDIVIDUAL";
                                            break;
                                    }
                                }
                                switch (box_33_type)
                                {
                                    case "GROUP": // Group  
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Bl_Group_Npi))
                                        {
                                            Billing_Provider_NPI = claim.claimInfo.Bl_Group_Npi;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Grp_Taxonomy_Id))
                                        {
                                            TaxonomyCode = claim.claimInfo.Grp_Taxonomy_Id;
                                        }
                                        break;
                                    case "INDIVIDUAL": // Individual
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Bl_Npi))
                                        {
                                            Billing_Provider_NPI = claim.claimInfo.Bl_Npi;
                                        }

                                        if (!string.IsNullOrEmpty(claim.claimInfo.Taxonomy_Code))
                                        {
                                            TaxonomyCode = claim.claimInfo.Taxonomy_Code;
                                        }
                                        break;
                                }
                                #endregion

                                #region LOOP 2000A
                                strBatchStringR += "Loop ID - 2000A - Billing Provider Hierarchical Level~";

                                #region BILLING PROVIDER HIERARCHICAL LEVEL
                                strBatchStringR += "Billing Provider Hierarchical Level~";
                                strBatchStringR += "HL*" + HL + "**";
                                strBatchStringR += "20*1~";

                                strBatchString += "HL*" + HL + "**";
                                strBatchString += "20*1~";
                                segmentCount++;

                                #endregion

                                #region BILLING PROVIDER SPECIALTY INFORMATION
                                strBatchStringR += "Billing Provider Specialty Information~";
                                strBatchStringR += "PRV*BI*PXC*" + TaxonomyCode + "~";

                                strBatchString += "PRV*BI*PXC*" + TaxonomyCode + "~";
                                segmentCount++;

                                #endregion

                                #endregion

                                #region LOOP 2010AA (Billing Provider Information)
                                strBatchStringR += "Loop ID - 2010AA - Billing Provider Name~";

                                #region Billing Provider Name
                                strBatchStringR += "Billing Provider Name~";

                                switch (box_33_type)
                                {
                                    case "GROUP": // Group                                                        
                                        if (!string.IsNullOrEmpty(submitterCompanyName))
                                        {

                                            strBatchStringR += "NM1*85*2*";
                                            strBatchStringR += submitterCompanyName + "*****XX*";
                                            strBatchString += "NM1*85*2*";
                                            strBatchString += submitterCompanyName + "*****XX*";

                                        }
                                        else
                                        {
                                            errorList.Add("2010AA - Billing Provider Organization Name Missing.");
                                        }

                                        if (!string.IsNullOrEmpty(Billing_Provider_NPI))
                                        {
                                            strBatchStringR += Billing_Provider_NPI;
                                            strBatchString += Billing_Provider_NPI;
                                        }
                                        else
                                        {
                                            errorList.Add("2010AA - Billing Provider Group NPI Missing.");
                                        }
                                        break;
                                    case "INDIVIDUAL": // Individual  
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Bl_Lname) && !string.IsNullOrEmpty(claim.claimInfo.Bl_Fname))
                                        {

                                            strBatchStringR += "NM1*85*1*";
                                            strBatchStringR += claim.claimInfo.Bl_Lname + "*" + claim.claimInfo.Bl_Fname + "*" + claim.claimInfo.Bl_Mi + "***XX*";
                                            strBatchString += "NM1*85*1*";
                                            strBatchString += claim.claimInfo.Bl_Lname + "*" + claim.claimInfo.Bl_Fname + "*" + claim.claimInfo.Bl_Mi + "***XX*";

                                        }
                                        else
                                        {
                                            errorList.Add("2010AA - Billing Provider Name Missing.");
                                        }

                                        if (!string.IsNullOrEmpty(Billing_Provider_NPI))
                                        {
                                            strBatchStringR += Billing_Provider_NPI;
                                            strBatchString += Billing_Provider_NPI;
                                        }
                                        else
                                        {
                                            errorList.Add("2010AA - Billing Provider Individual NPI Missing.");
                                        }

                                        break;
                                }
                                strBatchStringR += "~";

                                strBatchString += "~";
                                segmentCount++;

                                #endregion

                                #region BILLING PROVIDER ADDRESS
                                strBatchStringR += "Billing Provider Address~";

                                switch (box_33_type)
                                {
                                    case "GROUP": // Group                                                                               
                                        if (string.IsNullOrEmpty(claim.claimInfo.Bill_Address_Grp.Trim())
                                                || string.IsNullOrEmpty(claim.claimInfo.Bill_City_Grp.Trim())
                                                || string.IsNullOrEmpty(claim.claimInfo.Bill_State_Grp.Trim())
                                                || string.IsNullOrEmpty(claim.claimInfo.Bill_Zip_Grp.Trim()))
                                        {
                                            errorList.Add("BILLING ADDRESS ! Billing Provider Group Address is Missing.");
                                        }
                                        else
                                        {
                                            strBatchStringR += "N3*";
                                            strBatchStringR += claim.claimInfo.Bill_Address_Grp.Trim() + "~";
                                            strBatchString += "N3*";
                                            strBatchString += claim.claimInfo.Bill_Address_Grp.Trim() + "~";
                                            segmentCount++;

                                            strBatchStringR += "Billing Provider City, State, ZIP~";
                                            strBatchStringR += "N4*";
                                            strBatchStringR += claim.claimInfo.Bill_City_Grp.Trim() + "*";
                                            strBatchStringR += claim.claimInfo.Bill_State_Grp.Trim() + "*";
                                            strBatchString += "N4*";
                                            strBatchString += claim.claimInfo.Bill_City_Grp.Trim() + "*";
                                            strBatchString += claim.claimInfo.Bill_State_Grp.Trim() + "*";
                                            if (string.IsNullOrEmpty(claim.claimInfo.Bill_Zip_Grp.Trim()))
                                            {
                                                strBatchStringR += "     ";
                                                strBatchString += "     ";
                                            }
                                            else
                                            {
                                                strBatchStringR += claim.claimInfo.Bill_Zip_Grp.Trim() + "~";
                                                strBatchString += claim.claimInfo.Bill_Zip_Grp.Trim() + "~";
                                            }
                                            segmentCount++;
                                        }
                                        break;
                                    case "INDIVIDUAL": // Individual  

                                        if (string.IsNullOrEmpty(claim.claimInfo.Bl_Address.Trim())
                                               || string.IsNullOrEmpty(claim.claimInfo.Bl_City.Trim())
                                               || string.IsNullOrEmpty(claim.claimInfo.Bl_State.Trim())
                                               || string.IsNullOrEmpty(claim.claimInfo.Bl_Zip.Trim()))
                                        {
                                            errorList.Add("BILLING ADDRESS ! Billing Provider Individual Address is Missing.");
                                        }
                                        else
                                        {
                                            strBatchStringR += "N3*";
                                            strBatchStringR += claim.claimInfo.Bl_Address.Trim() + "~";
                                            strBatchString += "N3*";
                                            strBatchString += claim.claimInfo.Bl_Address.Trim() + "~";
                                            segmentCount++;

                                            strBatchStringR += "Billing Provider City, State, ZIP~";
                                            strBatchStringR += "N4*";
                                            strBatchStringR += claim.claimInfo.Bl_City.Trim() + "*";
                                            strBatchStringR += claim.claimInfo.Bl_State.Trim() + "*";
                                            strBatchString += "N4*";
                                            strBatchString += claim.claimInfo.Bl_City.Trim() + "*";
                                            strBatchString += claim.claimInfo.Bl_State.Trim() + "*";
                                            if (string.IsNullOrEmpty(claim.claimInfo.Bl_Zip.Trim()))
                                            {
                                                strBatchStringR += "     ";
                                                strBatchString += "     ";
                                            }
                                            else
                                            {
                                                strBatchStringR += claim.claimInfo.Bl_Zip.Trim() + "~";
                                                strBatchString += claim.claimInfo.Bl_Zip.Trim() + "~";
                                            }
                                            segmentCount++;

                                        }

                                        break;
                                }


                                #endregion

                                #region BILLING PROVIDER Tax Identification
                                strBatchStringR += "Billing Provider Tax ID~";

                                // hcfa box 25.. 
                                if (!string.IsNullOrEmpty(FederalTaxIDType) && !string.IsNullOrEmpty(FederalTaxID))
                                {
                                    if (FederalTaxIDType.Equals("EIN"))
                                    {
                                        //..Below Condition is added by tamour dated 04/04/2024 to enforce in case of Medicare of texas to use SSN only
                                        if (practice_id == 35510256 && InspayerId == 50033169 && PayerNumber == "04412")
                                        {
                                            strBatchStringR += "REF*SY*";
                                            strBatchString += "REF*SY*";
                                        }
                                        else
                                        {
                                            strBatchStringR += "REF*EI*";
                                            strBatchString += "REF*EI*";
                                        }
                                    }
                                    else if (FederalTaxIDType.Equals("SSN"))
                                    {
                                        strBatchStringR += "REF*SY*";
                                        strBatchString += "REF*SY*";
                                    }
                                    //..Below Condition is added by tamour dated 04/04/2024 to enforce in case of Medicare of texas to use SSN only
                                    if (practice_id == 35510256 && InspayerId == 50033169 && PayerNumber == "04412")
                                    {
                                        if (!string.IsNullOrEmpty(SSN_Number))
                                        {
                                            strBatchStringR += SSN_Number + "~";
                                            strBatchString += SSN_Number + "~";
                                        }
                                        else
                                        {
                                            errorList.Add("Billing provider SSN number is missing.");
                                        }
                                    }
                                    else
                                    {
                                        strBatchStringR += FederalTaxID + "~";
                                        strBatchString += FederalTaxID + "~";
                                    }
                                    segmentCount += 1;
                                }
                                else
                                {
                                    errorList.Add("Billing provider federal tax id number/type missing.");
                                }

                                #endregion

                                #region  BILLING PROVIDER CONTACT INFORMATION
                                strBatchStringR += "Billing Provider CONTACT INFORMATION~";

                                switch (FederalTaxIDType)
                                {
                                    case "EIN":
                                        if (!string.IsNullOrEmpty(submitterCompanyName)
                                                && !string.IsNullOrEmpty(claim.claimInfo.Phone_No))
                                        {
                                            strBatchStringR += "PER*IC*" + submitterCompanyName;
                                            strBatchStringR += "*TE*" + claim.claimInfo.Phone_No.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim() + "~";
                                            strBatchString += "PER*IC*" + submitterCompanyName;
                                            strBatchString += "*TE*" + claim.claimInfo.Phone_No.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim() + "~";
                                            segmentCount++;
                                        }
                                        else
                                        {
                                            errorList.Add("Billing Provider Contact Information Missing.");

                                        }
                                        break;
                                    case "SSN":
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Bl_Lname)
                                                && !string.IsNullOrEmpty(claim.claimInfo.Phone_No))
                                        {
                                            strBatchStringR += "PER*IC*" + claim.claimInfo.Bl_Lname;
                                            strBatchStringR += "*TE*" + claim.claimInfo.Phone_No.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim() + "~";
                                            strBatchString += "PER*IC*" + claim.claimInfo.Bl_Lname;
                                            strBatchString += "*TE*" + claim.claimInfo.Phone_No.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim() + "~";
                                            segmentCount++;
                                        }
                                        else
                                        {
                                            errorList.Add("Billing Provider Contact Information Missing.");
                                        }
                                        break;
                                }
                                #endregion

                                #endregion

                                #region LOOP 2010AB (PAY-TO ADDRESS NAME)
                                strBatchStringR += "LOOP 2010AB PAY-TO ADDRESS NAME~";
                                strBatchStringR += "PAY -TO PROVIDER NAME~";

                                switch (box_33_type)
                                {
                                    case "GROUP": // Group
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Bill_Address_Grp?.Trim())
                                            && !string.IsNullOrEmpty(claim.claimInfo.Pay_To_Address_Grp?.Trim())
                                            && !claim.claimInfo.Bill_Address_Grp.Trim().Equals(claim.claimInfo.Pay_To_Address_Grp.Trim()))
                                        {
                                            if (!string.IsNullOrEmpty(claim.claimInfo.Pay_To_Address_Grp.Trim())
                                                || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_City_Grp.Trim())
                                                || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_State_Grp.Trim())
                                                || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_Zip_Grp.Trim()))
                                            {

                                                if (string.IsNullOrEmpty(claim.claimInfo.Pay_To_Address_Grp.Trim())
                                                        || string.IsNullOrEmpty(claim.claimInfo.Pay_To_City_Grp.Trim())
                                                        || string.IsNullOrEmpty(claim.claimInfo.Pay_To_State_Grp.Trim()))
                                                {
                                                    errorList.Add("2010AB : Pay to Provider Group Address is incomplete.");
                                                }
                                                else
                                                {
                                                    switch (FederalTaxIDType)
                                                    {
                                                        case "EIN":
                                                            strBatchStringR += "NM1*87*2~";
                                                            strBatchString += "NM1*87*2~";
                                                            segmentCount++;
                                                            break;
                                                        case "SSN":
                                                            strBatchStringR += "NM1*87*1~";
                                                            strBatchString += "NM1*87*1~";
                                                            segmentCount++;
                                                            break;
                                                    }

                                                    strBatchStringR += "PAY-TO PROVIDER ADDRESS~";
                                                    strBatchStringR += "N3*";
                                                    strBatchStringR += claim.claimInfo.Pay_To_Address_Grp + "~";

                                                    strBatchString += "N3*";
                                                    strBatchString += claim.claimInfo.Pay_To_Address_Grp + "~";
                                                    segmentCount++;

                                                    strBatchStringR += "PAY - TO PROVIDER CITY~";
                                                    strBatchStringR += "N4*";
                                                    strBatchStringR += claim.claimInfo.Pay_To_City_Grp.Trim() + "*";
                                                    strBatchStringR += claim.claimInfo.Pay_To_State_Grp + "*";
                                                    strBatchString += "N4*";
                                                    strBatchString += claim.claimInfo.Pay_To_City_Grp.Trim() + "*";
                                                    strBatchString += claim.claimInfo.Pay_To_State_Grp + "*";
                                                    if (string.IsNullOrEmpty(claim.claimInfo.Pay_To_Zip_Grp.Trim()))
                                                    {
                                                        strBatchStringR += "     ";
                                                        strBatchString += "     ";
                                                    }
                                                    else
                                                    {
                                                        strBatchStringR += claim.claimInfo.Pay_To_Zip_Grp.Trim() + "~";
                                                        strBatchString += claim.claimInfo.Pay_To_Zip_Grp.Trim() + "~";
                                                    }
                                                    segmentCount++;

                                                }
                                            }
                                        }


                                        break;
                                    case "INDIVIDUAL": // Individual  

                                        if (!string.IsNullOrEmpty(claim.claimInfo.Bl_Address?.Trim())
                                            && !string.IsNullOrEmpty(claim.claimInfo.Pay_To_Address?.Trim())
                                            && !claim.claimInfo.Pay_To_Address.Trim().Equals(claim.claimInfo.Bl_Address.Trim()))
                                        {
                                            if (!string.IsNullOrEmpty(claim.claimInfo.Pay_To_Address.Trim())
                                                || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_City.Trim())
                                                || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_State.Trim())
                                                || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_Zip.Trim()))
                                            {

                                                if (string.IsNullOrEmpty(claim.claimInfo.Pay_To_Address.Trim())
                                                        || string.IsNullOrEmpty(claim.claimInfo.Pay_To_City.Trim())
                                                        || string.IsNullOrEmpty(claim.claimInfo.Pay_To_State.Trim()))
                                                {
                                                    errorList.Add("2010AB : Pay to Provider Individual Address is incomplete");
                                                }
                                                else
                                                {
                                                    switch (FederalTaxIDType)
                                                    {
                                                        case "EIN":
                                                            strBatchStringR += "NM1*87*2~";
                                                            strBatchString += "NM1*87*2~";
                                                            segmentCount++;
                                                            break;
                                                        case "SSN":
                                                            strBatchStringR += "NM1*87*1~";
                                                            strBatchString += "NM1*87*1~";
                                                            segmentCount++;
                                                            break;
                                                    }

                                                    strBatchStringR += "PAY-TO PROVIDER ADDRESS~";
                                                    strBatchStringR += "N3*";
                                                    strBatchStringR += claim.claimInfo.Pay_To_Address + "~";
                                                    strBatchString += "N3*";
                                                    strBatchString += claim.claimInfo.Pay_To_Address + "~";
                                                    segmentCount++;

                                                    strBatchStringR += "PAY - TO PROVIDER CITY~";
                                                    strBatchStringR += "N4*";
                                                    strBatchStringR += claim.claimInfo.Pay_To_City.Trim() + "*";
                                                    strBatchStringR += claim.claimInfo.Pay_To_State + "*";
                                                    strBatchString += "N4*";
                                                    strBatchString += claim.claimInfo.Pay_To_City.Trim() + "*";
                                                    strBatchString += claim.claimInfo.Pay_To_State + "*";
                                                    if (string.IsNullOrEmpty(claim.claimInfo.Pay_To_Zip.Trim()))
                                                    {
                                                        strBatchStringR += "     ";
                                                        strBatchString += "     ";
                                                    }
                                                    else
                                                    {
                                                        strBatchStringR += claim.claimInfo.Pay_To_Zip.Trim() + "~";
                                                        strBatchString += claim.claimInfo.Pay_To_Zip.Trim() + "~";
                                                    }
                                                    segmentCount++;

                                                }
                                            }
                                        }



                                        break;
                                }

                                #endregion


                                int P = HL;
                                HL = HL + 1;
                                int CHILD = 0;

                                string SBR02 = "18";


                                //---Extract Primar Secondary and Other Insurance Information before processing-----------
                                spGetBatchClaimsInsurancesInfo_Result primaryIns = null;
                                spGetBatchClaimsInsurancesInfo_Result SecondaryIns = null;
                                spGetBatchClaimsInsurancesInfo_Result otherIns = null;

                                if (claim.claimInsurance != null && claim.claimInsurance.Count > 0)
                                {
                                    foreach (var ins in claim.claimInsurance)
                                    {
                                        switch (ins.Insurace_Type.ToUpper().Trim())
                                        {
                                            case "P":
                                                primaryIns = ins;
                                                break;
                                            case "S":
                                                SecondaryIns = ins;
                                                break;
                                            case "O":
                                                otherIns = ins;
                                                break;
                                        }
                                    }
                                }

                                //--End

                                if (claim.claimInsurance == null || claim.claimInsurance.Count == 0)
                                {
                                    errorList.Add("Patient Insurance Information is missing.");
                                }
                                else if (primaryIns == null)
                                {
                                    errorList.Add("Patient Primary Insurance Information is missing.");
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(primaryIns.GRelationship)
                               && primaryIns.GRelationship.Trim().ToUpper().Equals("S"))
                                    {
                                        primaryIns.Glname = claim.claimInfo.Lname;
                                        primaryIns.Gfname = claim.claimInfo.Fname;
                                        primaryIns.Gmi = claim.claimInfo.Mname;
                                        primaryIns.Gaddress = claim.claimInfo.Address;
                                        primaryIns.Gcity = claim.claimInfo.City;
                                        primaryIns.Gdob = claim.claimInfo.Dob;
                                        primaryIns.Ggender = claim.claimInfo.Gender.ToString();
                                        primaryIns.Gstate = claim.claimInfo.State;
                                        primaryIns.Gzip = claim.claimInfo.Zip;
                                    }


                                    if (!primaryIns.GRelationship.Trim().ToUpper().Equals("S") && primaryIns.Guarantor_Id == null)
                                    {
                                        errorList.Add("Subscriber information is missing.");

                                    }
                                    if (primaryIns.Inspayer_Id == null)
                                    {
                                        errorList.Add("Payer's information is missing.");
                                    }

                                    if (
                                        !primaryIns.GRelationship.Trim().ToUpper().Equals("S")
                                        && !string.IsNullOrEmpty(primaryIns.GRelationship))
                                    {
                                        SBR02 = "";
                                        CHILD = 1;
                                    }


                                    #region LOOP 2000B
                                    strBatchStringR += "Loop ID -2000B - Subscriber Hierarchical Level~";


                                    #region HL: SUBSCRIBER HIERARCHICAL LEVEL
                                    strBatchStringR += "Subscriber Hierarchical Level~";
                                    strBatchStringR += "HL*";
                                    strBatchStringR += HL + "*" + P + "*";
                                    strBatchStringR += "22*" + CHILD + "~";

                                    strBatchString += "HL*";
                                    strBatchString += HL + "*" + P + "*";
                                    strBatchString += "22*" + CHILD + "~";
                                    segmentCount++;
                                    #endregion

                                    #region SBR: SUBSCRIBER INFORMATION
                                    strBatchStringR += "Subscriber Information~";
                                    strBatchStringR += "SBR*";

                                    strBatchString += "SBR*";
                                    if (primaryIns != null)
                                    {
                                        strBatchString += "P";
                                        strBatchStringR += "P*";
                                    }
                                    else if (SecondaryIns != null)
                                    {
                                        strBatchString += "S";
                                        strBatchStringR += "S*";
                                    }
                                    else if (otherIns != null)
                                    {
                                        strBatchString += "T";
                                        strBatchStringR += "T*";
                                    }
                                    strBatchString += "*";
                                    string groupNo = "";
                                    string planName = "";
                                    string payerTypeCode = "";

                                    if (!string.IsNullOrEmpty(primaryIns.Group_Number))
                                    {
                                        groupNo = primaryIns.Group_Number;
                                    }
                                    else
                                    {
                                        groupNo = "";
                                    }





                                    if (!string.IsNullOrEmpty(primaryIns.Insgroup_Name) && primaryIns.Insgroup_Name.Equals("MEDICARE"))
                                    {
                                        if (!string.IsNullOrEmpty(primaryIns.plan_name) && primaryIns.plan_name.ToUpper().Contains("MEDICARE"))
                                        {
                                            planName = primaryIns.plan_name;
                                        }
                                        else
                                        {
                                            planName = "MEDICARE";
                                        }
                                    }
                                    else
                                    {
                                        planName = primaryIns.plan_name;
                                    }

                                    // MISSING [To Do]
                                    //payerTypeCode = primaryIns.getPayertype_code()
                                    payerTypeCode = primaryIns.insurance_type_code;


                                    //---------***********************************-------------
                                    strBatchStringR += SBR02 + "*" + groupNo + "*" + planName + "*****" + payerTypeCode + "~";
                                    strBatchString += SBR02 + "*" + groupNo + "*" + planName + "*****" + payerTypeCode + "~";
                                    segmentCount++;
                                    #endregion

                                    #endregion

                                    #region LOOP 2010BA (SUBSCRIBER Information)
                                    strBatchStringR += "Loop ID -2010BA - Subscriber Name~";
                                    strBatchStringR += "Subscriber Name~";
                                    strBatchStringR += "NM1*IL*1*";

                                    strBatchString += "NM1*IL*1*";
                                    if ((string.IsNullOrEmpty(primaryIns.Glname)
                                    || string.IsNullOrEmpty(primaryIns.Gfname))
                                    && string.IsNullOrEmpty(primaryIns.GRelationship)
                                    && !primaryIns.GRelationship.Trim().ToUpper().Equals("S"))
                                    {
                                        errorList.Add("Subscriber Last/First Name missing.");
                                    }

                                    //Entering Subscriber Information if Relationship is SELF-----
                                    if (SBR02.Equals("18"))
                                    {
                                        if (!isAlphaNumeric(claim.claimInfo.Lname)
                                            || !isAlphaNumeric(claim.claimInfo.Fname)
                                            )
                                        {
                                            errorList.Add("Subscriber Name must be Alpha Numeric.");
                                        }
                                        else
                                        {

                                            strBatchStringR += claim.claimInfo.Lname + "*"
                                                  + claim.claimInfo.Fname + "*"
                                                  + claim.claimInfo.Mname + "***MI*"
                                                  + primaryIns.Policy_Number.ToUpper() + "~";

                                            strBatchString += claim.claimInfo.Lname + "*"
                                                    + claim.claimInfo.Fname + "*"
                                                    + claim.claimInfo.Mname + "***MI*"
                                                    + primaryIns.Policy_Number.ToUpper() + "~";
                                            segmentCount++;

                                        }

                                        if (string.IsNullOrEmpty(claim.claimInfo.Address)
                                            || string.IsNullOrEmpty(claim.claimInfo.City)
                                             || string.IsNullOrEmpty(claim.claimInfo.State)
                                             || string.IsNullOrEmpty(claim.claimInfo.Zip))
                                        {
                                            errorList.Add("Patient Address is incomplete.");
                                        }
                                        else
                                        {
                                            strBatchStringR += "Subscriber Address~";
                                            strBatchStringR += "N3*" + claim.claimInfo.Address + "~";
                                            strBatchString += "N3*" + claim.claimInfo.Address + "~";
                                            segmentCount++;

                                            strBatchStringR += "Subscriber City, State, ZIP~";
                                            strBatchStringR += "N4*" + claim.claimInfo.City + "*"
                                                    + claim.claimInfo.State + "*";
                                            strBatchStringR += (!string.IsNullOrEmpty(claim.claimInfo.Zip) ? claim.claimInfo.Zip : "     ") + "~";

                                            strBatchString += "N4*" + claim.claimInfo.City + "*"
                                                    + claim.claimInfo.State + "*";
                                            strBatchString += (!string.IsNullOrEmpty(claim.claimInfo.Zip) ? claim.claimInfo.Zip : "     ") + "~";
                                            segmentCount++;
                                        }

                                        strBatchStringR += "Subscriber Demographic Information~";
                                        strBatchStringR += "DMG*D8*";

                                        strBatchString += "DMG*D8*";
                                        if (string.IsNullOrEmpty(claim.claimInfo.Dob))
                                        {
                                            errorList.Add("Patient DOB is missing.");
                                        }
                                        else
                                        {
                                            strBatchStringR += !string.IsNullOrEmpty(claim.claimInfo.Dob) ? claim.claimInfo.Dob.Split('/')[0] + claim.claimInfo.Dob.Split('/')[1] + claim.claimInfo.Dob.Split('/')[2] : "";
                                            strBatchStringR += "*";

                                            strBatchString += !string.IsNullOrEmpty(claim.claimInfo.Dob) ? claim.claimInfo.Dob.Split('/')[0] + claim.claimInfo.Dob.Split('/')[1] + claim.claimInfo.Dob.Split('/')[2] : "";
                                            strBatchString += "*";
                                        }
                                        if (string.IsNullOrEmpty(claim.claimInfo.Gender.ToString()))
                                        {
                                            errorList.Add("Patient Gender is missing.");
                                        }
                                        else
                                        {
                                            strBatchStringR += claim.claimInfo.Gender.ToString();
                                            strBatchString += claim.claimInfo.Gender.ToString();

                                        }
                                        strBatchStringR += "~";
                                        strBatchString += "~";
                                        segmentCount++;
                                    } //--END
                                    else //---Entering Subscriber Information In case of other than SELF---------
                                    {
                                        strBatchStringR += primaryIns.Glname + "*"
                                                + primaryIns.Gfname + "*"
                                                + primaryIns.Gmi + "***MI*"
                                                + primaryIns.Policy_Number.ToUpper() + "~";

                                        strBatchString += primaryIns.Glname + "*"
                                                + primaryIns.Gfname + "*"
                                                + primaryIns.Gmi + "***MI*"
                                                + primaryIns.Policy_Number.ToUpper() + "~";
                                        segmentCount++;

                                        if (string.IsNullOrEmpty(primaryIns.Gaddress)
                                           || string.IsNullOrEmpty(primaryIns.Gcity)
                                            || string.IsNullOrEmpty(primaryIns.Gstate)
                                            || string.IsNullOrEmpty(primaryIns.Gzip))
                                        {
                                            errorList.Add("Subscriber Address is incomplete.");
                                        }
                                        else
                                        {
                                            strBatchStringR += "Subscriber Address~";
                                            strBatchStringR += "N3*" + primaryIns.Gaddress + "~";
                                            strBatchString += "N3*" + primaryIns.Gaddress + "~";
                                            segmentCount++;

                                            strBatchStringR += "Subscriber City, State, ZIP~";
                                            strBatchStringR += "N4*" + primaryIns.Gcity + "*"
                                                    + primaryIns.Gstate + "*";
                                            strBatchStringR += (string.IsNullOrEmpty(primaryIns.Gzip) ? primaryIns.Gzip : "     ") + "~";

                                            strBatchString += "N4*" + primaryIns.Gcity + "*"
                                                    + primaryIns.Gstate + "*";
                                            strBatchString += (string.IsNullOrEmpty(primaryIns.Gzip) ? primaryIns.Gzip : "     ") + "~";
                                            segmentCount++;
                                        }

                                        strBatchStringR += "Subscriber Demographic Information~";
                                        strBatchStringR += "DMG*D8*";

                                        strBatchString += "DMG*D8*";
                                        if (string.IsNullOrEmpty(primaryIns.Gdob))
                                        {
                                            errorList.Add("Subscriber DOB is missing.");
                                        }
                                        else
                                        {
                                            strBatchStringR += string.IsNullOrEmpty(primaryIns.Gdob) ? primaryIns.Gdob.Split('/')[0] + primaryIns.Gdob.Split('/')[1] + primaryIns.Gdob.Split('/')[2] : "";
                                            strBatchStringR += "*";

                                            strBatchString += string.IsNullOrEmpty(primaryIns.Gdob) ? primaryIns.Gdob.Split('/')[0] + primaryIns.Gdob.Split('/')[1] + primaryIns.Gdob.Split('/')[2] : "";
                                            strBatchString += "*";
                                        }

                                        if (string.IsNullOrEmpty(primaryIns.Ggender))
                                        {
                                            errorList.Add("Subscriber Gender is missing.");
                                        }
                                        else
                                        {
                                            strBatchStringR += primaryIns.Ggender;
                                            strBatchString += primaryIns.Ggender;

                                        }
                                        strBatchStringR += "~";
                                        strBatchString += "~";
                                        segmentCount++;
                                    }

                                    #endregion

                                    #region LOOP 2010BB (PAYER INFORMATION)
                                    strBatchStringR += "Loop ID - 2010BB - Payer Name~";
                                    strBatchStringR += "Payer Name~";

                                    if (string.IsNullOrEmpty(primaryIns.plan_name))
                                    {
                                        errorList.Add("Payer name missing.");

                                    }
                                    string paperPayerName = "";
                                    if (!string.IsNullOrEmpty(primaryIns.plan_name) && primaryIns.plan_name.Trim().ToUpper().Equals("MEDICARE"))
                                    {
                                        paperPayerName = "MEDICARE";
                                    }
                                    else
                                    {
                                        paperPayerName = primaryIns.plan_name;
                                    }

                                    paperPayerID = primaryIns.Payer_Number;
                                    if (!string.IsNullOrEmpty(paperPayerID))
                                    {
                                        strBatchStringR += "NM1*PR*2*" + paperPayerName + "*****PI*" + paperPayerID + "~";
                                        strBatchString += "NM1*PR*2*" + paperPayerName + "*****PI*" + paperPayerID + "~";
                                        segmentCount++;
                                    }
                                    else
                                    {
                                        errorList.Add("Payer id is compulsory in case of Gateway EDI Clearing house.");
                                    }
                                    if (!string.IsNullOrEmpty(primaryIns.Insgroup_Name) && primaryIns.plan_name.Trim().ToUpper().Equals("WORK COMP"))
                                    {
                                        strBatchStringR += "Payer Address~";
                                        if (string.IsNullOrEmpty(primaryIns.Sub_Empaddress)
                                                || string.IsNullOrEmpty(primaryIns.Sub_Emp_City)
                                                || string.IsNullOrEmpty(primaryIns.Sub_Emp_State)
                                                || string.IsNullOrEmpty(primaryIns.Sub_Emp_Zip))
                                        {
                                            errorList.Add("Payer is Worker Company, so its subscriber employer’s address is necessary.");

                                        }
                                        strBatchStringR += "N3*" + primaryIns.Sub_Empaddress + "~";
                                        strBatchString += "N3*" + primaryIns.Sub_Empaddress + "~";
                                        segmentCount++;

                                        strBatchStringR += "Payer City, State, ZIP~";
                                        strBatchStringR += "N4*" + primaryIns.Sub_Emp_City + "*"
                                                + primaryIns.Sub_Emp_State + "*";

                                        strBatchString += "N4*" + primaryIns.Sub_Emp_City + "*"
                                                + primaryIns.Sub_Emp_State + "*";
                                        if (!string.IsNullOrEmpty(primaryIns.Sub_Emp_Zip))
                                        {
                                            strBatchStringR += primaryIns.Sub_Emp_Zip;
                                            strBatchString += primaryIns.Sub_Emp_Zip;

                                        }
                                        else
                                        {
                                            strBatchStringR += "     ";
                                            strBatchString += "     ";
                                        }
                                        strBatchStringR += "~";
                                        strBatchString += "~";
                                        segmentCount++;
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(primaryIns.Ins_Address)
                                                || string.IsNullOrEmpty(primaryIns.Ins_City)
                                                || string.IsNullOrEmpty(primaryIns.Ins_State)
                                                || string.IsNullOrEmpty(primaryIns.Ins_Zip))
                                        {
                                            errorList.Add("Payer address incomplete.");
                                        }
                                        strBatchStringR += "N3*" + primaryIns.Ins_Address;
                                        strBatchStringR += "~";

                                        strBatchString += "N3*" + primaryIns.Ins_Address;
                                        strBatchString += "~";
                                        segmentCount++;

                                        strBatchStringR += "Payer City, State, ZIP~";
                                        strBatchStringR += "N4*" + primaryIns.Ins_City + "*" + primaryIns.Ins_State + "*";
                                        strBatchStringR += (string.IsNullOrEmpty(primaryIns.Ins_Zip)) ? "     " : primaryIns.Ins_Zip.Trim();
                                        strBatchStringR += "~";

                                        strBatchString += "N4*" + primaryIns.Ins_City + "*" + primaryIns.Ins_State + "*";
                                        strBatchString += (string.IsNullOrEmpty(primaryIns.Ins_Zip)) ? "     " : primaryIns.Ins_Zip.Trim();
                                        strBatchString += "~";
                                        segmentCount++;
                                    }

                                    #endregion

                                    #region LOOP 2010C , 2010CA

                                    if (!string.IsNullOrEmpty(primaryIns.GRelationship)
                                       && !primaryIns.GRelationship.ToUpper().Trim().Equals("S"))
                                    {

                                        #region LOOP 2000C
                                        strBatchStringR += "Loop ID - 2000C - Patient Hierarchical Level~";

                                        #region HL : (PATIENT HIERARCHICAL LEVEL)
                                        strBatchStringR += "Patient Hierarchical Level~";

                                        int PHL = HL;
                                        HL++;
                                        strBatchStringR += "HL*" + HL + "*" + PHL + "*23*0~";
                                        strBatchString += "HL*" + HL + "*" + PHL + "*23*0~";
                                        segmentCount++;
                                        #endregion


                                        #region PAT : (PATIENT RELATIONAL INFORMATION)
                                        strBatchStringR += "PATIENT RELATIONAL INFORMATION~";
                                        strBatchStringR += "PAT*";

                                        strBatchString += "PAT*";
                                        String temp = "";
                                        if (string.IsNullOrEmpty(primaryIns.GRelationship))
                                        {
                                            errorList.Add("Subscriber relationship is missing.");
                                        }
                                        else
                                        {
                                            if (primaryIns.GRelationship.Trim().ToUpper().Equals("S"))
                                            {
                                                temp = "18";
                                            }
                                            else if (primaryIns.GRelationship.Trim().ToUpper().Equals("P"))
                                            {
                                                temp = "01";
                                            }
                                            else if (primaryIns.GRelationship.Trim().ToUpper().Equals("C"))
                                            {
                                                temp = "19";
                                            }
                                            else if (primaryIns.GRelationship.Trim().ToUpper().Equals("O"))
                                            {
                                                temp = "G8";
                                            }
                                        }

                                        strBatchStringR += temp + "****D8***~";
                                        strBatchString += temp + "****D8***~";
                                        segmentCount++;
                                        #endregion

                                        #endregion


                                        #region LOOP 2010CA
                                        strBatchStringR += "Loop ID -2010CA - Patient Name~";


                                        #region PATIENT NAME INFORMATION
                                        strBatchStringR += "Patient Name~";
                                        strBatchStringR += "NM1*QC*1*";

                                        strBatchString += "NM1*QC*1*";

                                        //----ENTERING PATIENT INFORMATION NOW------------
                                        strBatchStringR += claim.claimInfo.Lname + "*";
                                        strBatchStringR += claim.claimInfo.Fname + "*";
                                        strBatchStringR += claim.claimInfo.Mname + "***MI*";

                                        strBatchString += claim.claimInfo.Lname + "*";
                                        strBatchString += claim.claimInfo.Fname + "*";
                                        strBatchString += claim.claimInfo.Mname + "***MI*";
                                        if (string.IsNullOrEmpty(primaryIns.Policy_Number))
                                        {
                                            errorList.Add("Subscriber policy number  missing.");
                                        }
                                        strBatchStringR += primaryIns.Policy_Number.ToUpper() + "~";

                                        strBatchString += primaryIns.Policy_Number.ToUpper() + "~";
                                        segmentCount++;
                                        strBatchStringR += "Patient Address~";
                                        strBatchStringR += "N3*" + claim.claimInfo.Address.Trim() + "~";

                                        strBatchString += "N3*" + claim.claimInfo.Address.Trim() + "~";
                                        segmentCount++;
                                        strBatchStringR += "Patient City, State, ZIP~";
                                        strBatchStringR += "N4*" + claim.claimInfo.City.Trim() + "*" + claim.claimInfo.State.Trim() + "*"
                                                + claim.claimInfo.Zip.Trim() + "~";

                                        strBatchString += "N4*" + claim.claimInfo.City.Trim() + "*" + claim.claimInfo.State.Trim() + "*"
                                                + claim.claimInfo.Zip.Trim() + "~";
                                        segmentCount++;

                                        if (string.IsNullOrEmpty(claim.claimInfo.Gender.ToString()))
                                        {
                                            errorList.Add("Patient gender missing.");
                                        }
                                        strBatchStringR += "Patient Demographic Information~";
                                        strBatchStringR += "DMG*D8*" + claim.claimInfo.Dob.Split('/')[0] + claim.claimInfo.Dob.Split('/')[1] + claim.claimInfo.Dob.Split('/')[2] + "*" + claim.claimInfo.Gender.ToString() + "~";
                                        strBatchString += "DMG*D8*" + claim.claimInfo.Dob.Split('/')[0] + claim.claimInfo.Dob.Split('/')[1] + claim.claimInfo.Dob.Split('/')[2] + "*" + claim.claimInfo.Gender.ToString() + "~";
                                        segmentCount++;
                                        #endregion

                                        #endregion

                                    }

                                    #endregion

                                    HL++;

                                    #region LOOP 2300
                                    strBatchStringR += "Loop ID - 2300 - Claim Information~";
                                    strBatchStringR += "Claim Information~";
                                    strBatchStringR += "CLM*" + claim.claim_No + "*";

                                    strBatchString += "CLM*" + claim.claim_No + "*";

                                    decimal total_amount = 0;

                                    if (claim.claimInfo.Is_Resubmitted)
                                    {
                                        foreach (var proc in claim.claimProcedures)
                                        {
                                            if (proc.Is_Resubmitted)
                                            {
                                                total_amount = total_amount + (decimal)proc.Total_Charges;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        total_amount = claim.claimInfo.Claim_Total;
                                    }


                                    string ClaimFrequencyCode = (bool)claim.claimInfo.Is_Corrected ? claim.claimInfo.RSCode.ToString() : "1";
                                    string PatFirstVisitDatesegmentCount = "";

                                    strBatchStringR += string.Format("{0:0.00}", total_amount) + "***" + claim.claimInfo.Claim_Pos + ":B:" + ClaimFrequencyCode + "*Y*A*Y*Y*P"; // 5010
                                    strBatchString += string.Format("{0:0.00}", total_amount) + "***" + claim.claimInfo.Claim_Pos + ":B:" + ClaimFrequencyCode + "*Y*A*Y*Y*P"; // 5010


                                    #region Accident Info
                                    int isErrorInAccident = 0;

                                    if (!string.IsNullOrEmpty(claim.claimInfo.Accident_Type))
                                    {

                                        switch (claim.claimInfo.Accident_Type.ToUpper())
                                        {
                                            case "OA":
                                                strBatchStringR += "*OA";
                                                strBatchString += "*OA";
                                                break;
                                            case "AA":
                                                strBatchStringR += "*AA";
                                                strBatchString += "*AA";
                                                break;
                                            case "EM":
                                                strBatchStringR += "*EM";
                                                strBatchString += "*EM";
                                                break;
                                            default:
                                                isErrorInAccident = 1;
                                                break;
                                        }


                                        if (isErrorInAccident == 0)
                                        {
                                            if (!string.IsNullOrEmpty(claim.claimInfo.Accident_State))
                                            {
                                                strBatchStringR += ":::" + claim.claimInfo.Accident_State + "~";
                                                strBatchString += ":::" + claim.claimInfo.Accident_State + "~";
                                                segmentCount++;
                                            }
                                            else
                                            {
                                                if (claim.claimInfo.Accident_Type.ToUpper().Equals("OA")
                                                    || claim.claimInfo.Accident_Type.ToUpper().Equals("EM"))
                                                {
                                                    strBatchStringR += "~";
                                                    strBatchString += "~";
                                                    segmentCount++;
                                                }
                                                else
                                                {
                                                    isErrorInAccident = 2;
                                                }
                                            }

                                            if (isErrorInAccident == 0)
                                            {
                                                #region DATE  ACCIDENT
                                                strBatchStringR += "Claim Date";
                                                strBatchStringR += "DTP*439*D8*";

                                                strBatchString += "DTP*439*D8*";
                                                if (!string.IsNullOrEmpty(claim.claimInfo.Accident_Date) && !claim.claimInfo.Accident_Date.Equals("1900/01/01"))
                                                {
                                                    string[] splitedAccidentDate = claim.claimInfo.Accident_Date.Split('/');
                                                    if (splitedAccidentDate.Count() != 3)
                                                    {
                                                        isErrorInAccident = 3;
                                                    }
                                                    strBatchStringR += splitedAccidentDate[0] + splitedAccidentDate[1] + splitedAccidentDate[2] + "~";
                                                    strBatchString += splitedAccidentDate[0] + splitedAccidentDate[1] + splitedAccidentDate[2] + "~";
                                                    segmentCount++;
                                                }
                                                else
                                                {
                                                    isErrorInAccident = 4;
                                                }

                                                #endregion
                                            }
                                        }
                                    }
                                    else
                                    {
                                        strBatchStringR += "~";
                                        strBatchString += "~";
                                        segmentCount++;
                                    }
                                    #endregion

                                    #region DATE - INITIAL TREATMENT
                                    if (!string.IsNullOrEmpty(PatFirstVisitDatesegmentCount))
                                    {
                                        strBatchStringR += "DATE - INITIAL TREATMENT";
                                        strBatchStringR += PatFirstVisitDatesegmentCount;
                                        strBatchString += PatFirstVisitDatesegmentCount;
                                        segmentCount++;
                                    }

                                    #endregion

                                    #region DATE -  Last X-Ray Date

                                    if (!string.IsNullOrEmpty(claim.claimInfo.Last_Xray_Date) && !claim.claimInfo.Last_Xray_Date.Equals("1900/01/01"))
                                    {
                                        string[] spltdlastXrayDate = claim.claimInfo.Last_Xray_Date.Split('/');
                                        string LastXrayDate = spltdlastXrayDate[0] + spltdlastXrayDate[1] + spltdlastXrayDate[2];
                                        strBatchStringR += "DATE -  Last X-Ray Date~";
                                        strBatchStringR += "DTP*455*D8*" + LastXrayDate + "~";
                                        strBatchString += "DTP*455*D8*" + LastXrayDate + "~";
                                        segmentCount++;
                                    }

                                    #endregion

                                    #region DATE - ADMISSION (HOSPITALIZATION)


                                    if (!string.IsNullOrEmpty(claim.claimInfo.Hospital_From) && !claim.claimInfo.Hospital_From.Equals("1900/01/01"))
                                    {
                                        string[] spltdHospitalFromDate = claim.claimInfo.Hospital_From.Split('/');
                                        if (spltdHospitalFromDate.Count() != 3)
                                        {
                                            isErrorInAccident = 3;
                                        }
                                        string hospitalFromDate = spltdHospitalFromDate[0] + spltdHospitalFromDate[1] + spltdHospitalFromDate[2];
                                        strBatchStringR += "DATE - ADMISSION HOSPITALIZATION FromDate~";
                                        strBatchStringR += "DTP*435*D8*" + hospitalFromDate + "~";
                                        strBatchString += "DTP*435*D8*" + hospitalFromDate + "~";
                                        segmentCount++;
                                    }

                                    if (!string.IsNullOrEmpty(claim.claimInfo.Hospital_To) && !claim.claimInfo.Hospital_To.Equals("1900/01/01"))
                                    {
                                        string[] spltdHospitalTO = claim.claimInfo.Hospital_To.Split('/');
                                        if (spltdHospitalTO.Count() != 3)
                                        {
                                            isErrorInAccident = 3;
                                        }
                                        string hospitalTo = spltdHospitalTO[0] + spltdHospitalTO[1] + spltdHospitalTO[2];
                                        strBatchStringR += "DATE - ADMISSION HOSPITALIZATION ToDate~";
                                        strBatchStringR += "DTP*096*D8*" + hospitalTo + "~";
                                        strBatchString += "DTP*096*D8*" + hospitalTo + "~";
                                        segmentCount++;
                                    }

                                    #endregion


                                    if (isErrorInAccident >= 1)
                                    {
                                        if (isErrorInAccident == 1)
                                        {
                                            errorList.Add("Accident Type is missing.");
                                        }
                                        else if (isErrorInAccident == 2)
                                        {
                                            errorList.Add("State of accident is necessary.");
                                        }
                                        else if (isErrorInAccident == 3)
                                        {
                                            errorList.Add("Format of date of accident is not correct.");
                                        }
                                        else if (isErrorInAccident == 4)
                                        {
                                            errorList.Add("Date of accident is missing.");
                                        }
                                    }


                                    #region PRIOR AUTHORIZATION
                                    if (!string.IsNullOrEmpty(claim.claimInfo.Prior_Authorization))
                                    {
                                        strBatchStringR += "PRIOR AUTHORIZATION~";
                                        strBatchStringR += "REF*G1*" + claim.claimInfo.Prior_Authorization + "~";
                                        strBatchString += "REF*G1*" + claim.claimInfo.Prior_Authorization + "~";
                                        segmentCount++;
                                    }
                                    #endregion

                                    #region PAYER CLAIM CONTROL NUMBER
                                    if (!string.IsNullOrEmpty(claim.claimInfo.Claim_Number))
                                    {
                                        strBatchStringR += "PAYER CLAIM CONTROL NUMBER~";
                                        strBatchStringR += "REF*F8*" + claim.claimInfo.Claim_Number + "~";
                                        strBatchString += "REF*F8*" + claim.claimInfo.Claim_Number + "~";
                                        segmentCount++;
                                    }
                                    #endregion

                                    #region CLINICAL LABORATORY IMPROVEMENT AMENDMENT (CLIA) NUMBER
                                    if (!string.IsNullOrEmpty(claim.claimInfo.Clia_Number))
                                    {
                                        strBatchStringR += "CLINICAL LABORATORY IMPROVEMENT AMENDMENT (CLIA) NUMBER~";
                                        strBatchStringR += "REF*X4*" + claim.claimInfo.Clia_Number + "~";
                                        strBatchString += "REF*X4*" + claim.claimInfo.Clia_Number + "~";
                                        segmentCount++;
                                    }
                                    #endregion

                                    #region National Clinical trial Number (NCT)
                                    if (!string.IsNullOrEmpty(claim.claimInfo.Additional_Claim_Info))
                                    {
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Additional_Claim_Info) && claim.claimInfo.Additional_Claim_Info.StartsWith("CT") && claim.claimInfo.Additional_Claim_Info.Length > 2)
                                        {
                                            string newValue = claim.claimInfo.Additional_Claim_Info.Substring(2);
                                            if (!string.IsNullOrEmpty(newValue))
                                            {
                                                strBatchStringR += "National Clinical trial Number (NCT)~";
                                                strBatchStringR += "REF*P4*" + newValue + "~";
                                                strBatchString += "REF*P4*" + newValue + "~";
                                                segmentCount++;
                                            }
                                        }
                                        else
                                        {
                                            #region CLAIM NOTE (LUO)
                                            if (!string.IsNullOrEmpty(claim.claimInfo.Additional_Claim_Info))
                                            {
                                                strBatchStringR += "CLAIM NOTE (LUO)";
                                                strBatchStringR += "NTE*ADD*" + claim.claimInfo.Additional_Claim_Info + "~";
                                                strBatchString += "NTE*ADD*" + claim.claimInfo.Additional_Claim_Info + "~";
                                                segmentCount++;
                                            }
                                            #endregion
                                        }
                                    }
                                    #endregion

                                    #region CLAIM NOTE (LUO)
                                    if (!string.IsNullOrEmpty(claim.claimInfo.Luo))
                                    {
                                        strBatchStringR += "CLAIM NOTE (LUO)~";
                                        strBatchStringR += "NTE*ADD*" + claim.claimInfo.Luo + "~";
                                        strBatchString += "NTE*ADD*" + claim.claimInfo.Luo + "~";
                                        segmentCount++;
                                    }
                                    #endregion
                                    #region New:REF - Referral_Number
                                    //var claimid = claim.claim_No;
                                    if (sPDataModels != null && sPDataModels.Any())
                                    {
                                        var referralNumber = sPDataModels[0].REFERRAL_NUMBER;

                                        if (!string.IsNullOrEmpty(referralNumber))
                                        {
                                            strBatchString += "REF*9F*";
                                            strBatchString += $"{referralNumber}~";
                                            segmentCount++;
                                        }
                                    }
                                    #endregion End!
                                    #region HEALTH CARE DIAGNOSIS CODE
                                    strBatchStringR += "HEALTH CARE DIAGNOSIS CODE~";
                                    strBatchStringR += "HI*";
                                    strBatchString += "HI*";

                                    // ICD-10 Claim
                                    if ((bool)claim.claimInfo.Icd_10_Claim)
                                    {
                                        strBatchStringR += "ABK:";  // BK=ICD-9 ABK=ICD-10
                                        strBatchString += "ABK:";
                                    }
                                    else // ICD-9 Claim
                                    {
                                        strBatchStringR += "BK:";  // BK=ICD-9 ABK=ICD-10 
                                        strBatchString += "BK:";
                                    }

                                    //Adding claim ICDS Diagnosis COdes
                                    int diagCount = 0;
                                    if (claim.claimDiagnosis != null)
                                    {
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code1))
                                        {
                                            strBatchStringR += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code1);
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code1);
                                            diagCount++;
                                        }

                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code2))
                                        {
                                            strBatchStringR += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code2);
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code2);
                                            diagCount++;
                                        }

                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code3))
                                        {
                                            strBatchStringR += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code3);
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code3);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code4))
                                        {
                                            strBatchStringR += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code4);
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code4);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code5))
                                        {
                                            strBatchStringR += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code5);
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code5);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code6))
                                        {
                                            strBatchStringR += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code6);
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code6);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code7))
                                        {
                                            strBatchStringR += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code7);
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code7);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code8))
                                        {
                                            strBatchStringR += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code8);
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code8);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code9))
                                        {
                                            strBatchStringR += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code9);
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code9);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code10))
                                        {
                                            strBatchStringR += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code10);
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code10);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code11))
                                        {
                                            strBatchStringR += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code11);
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code11);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code12))
                                        {
                                            strBatchStringR += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code12);
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code12);
                                            diagCount++;
                                        }
                                    }
                                    if (diagCount == 0)
                                    {
                                        if ((bool)claim.claimInfo.Icd_10_Claim)
                                        {
                                            errorList.Add("HI*ABK:ABF!Claims Diagnosis (ICD-10) are missing.");
                                        }
                                        else
                                        {
                                            errorList.Add("HI*BK:BF!Claims Diagnosis (ICD-9) are missing.");
                                        }


                                    }
                                    strBatchStringR += "~";
                                    strBatchString += "~";
                                    segmentCount++;


                                    #endregion

                                    #endregion

                                    #region LOOP 2310A (REFERRING PROVIDER)
                                    strBatchStringR += "Loop ID - 2310A - Referring Provider Name~";
                                    if (claim.claimInfo.Referring_Physician != null)
                                    {
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Ref_Npi))
                                        {

                                            if (!isAlphaNumeric(claim.claimInfo.Ref_Lname)
                                                    || !isAlphaNumeric(claim.claimInfo.Ref_Fname))
                                            {
                                                errorList.Add("Referring provider’s Name must be Alpha Numeric..");
                                            }
                                            else
                                            {
                                                strBatchStringR += "Referring Provider Name~";
                                                strBatchStringR += "NM1*DN*1*" + claim.claimInfo.Ref_Lname + "*"
                                                        + claim.claimInfo.Ref_Fname + "****XX*"
                                                        + claim.claimInfo.Ref_Npi + "~";

                                                strBatchString += "NM1*DN*1*" + claim.claimInfo.Ref_Lname + "*"
                                                        + claim.claimInfo.Ref_Fname + "****XX*"
                                                        + claim.claimInfo.Ref_Npi + "~";

                                                segmentCount++;
                                            }
                                        }
                                        else
                                        {
                                            errorList.Add("Referring provider’s NPI is missing.");
                                        }
                                    }
                                    #endregion

                                    #region LOOP 2310B (RENDERING PROVIDER)
                                    strBatchStringR += "Loop ID - 2310B - Rendering Provider Name~";
                                    if (claim.claimInfo.Attending_Physician != null)
                                    {
                                        #region RENDERING PROVIDER NAME
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Att_Npi))
                                        {

                                            if (!isAlphaNumeric(claim.claimInfo.Att_Lname)
                                                    && !isAlphaNumeric(claim.claimInfo.Att_Fname))
                                            {
                                                errorList.Add("Rendering provider’s Name must be Alpha Numeric.");
                                            }
                                            else
                                            {

                                                strBatchStringR += "Rendering Provider Name~";
                                                strBatchStringR += "NM1*82*1*" + claim.claimInfo.Att_Lname + "*"
                                                        + claim.claimInfo.Att_Fname + "****XX*"
                                                        + claim.claimInfo.Att_Npi + "~";

                                                strBatchString += "NM1*82*1*" + claim.claimInfo.Att_Lname + "*"
                                                        + claim.claimInfo.Att_Fname + "****XX*"
                                                        + claim.claimInfo.Att_Npi + "~";

                                                segmentCount++;
                                            }

                                        }
                                        else
                                        {
                                            errorList.Add("Rendering Provider NPI Missing.");

                                        }
                                        #endregion

                                        #region RENDERING PROVIDER SPECIALTY INFORMATION

                                        if (!string.IsNullOrEmpty(claim.claimInfo.Att_Taxonomy_Code))
                                        {
                                            strBatchStringR += "Rendering Provider Specialty Information~";
                                            strBatchStringR += "PRV*PE*PXC*" + claim.claimInfo.Att_Taxonomy_Code + "~";

                                            strBatchString += "PRV*PE*PXC*" + claim.claimInfo.Att_Taxonomy_Code + "~"; //5010 CODE CHAGED FROM ZZ TO PXC
                                            segmentCount++;
                                        }
                                        else
                                        {
                                            errorList.Add("Gateway edi require Rendering Provider Taxonomy Code.");
                                        }
                                        #endregion

                                        #region RENDERING PROVIDER SPECIALTY INFORMATION                                
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Att_State_License))
                                        {
                                            strBatchStringR += "Rendering Provider Specialty Information~";
                                            strBatchStringR += "REF*0B*" + claim.claimInfo.Att_State_License + "~";
                                            strBatchString += "REF*0B*" + claim.claimInfo.Att_State_License + "~";
                                            segmentCount++;
                                        }
                                        #endregion

                                    }
                                    else
                                    {
                                        errorList.Add("Rendering Provider Information missing..");
                                    }
                                    #endregion



                                    #region LOOP 2310C (SERVICE FACILITY LOCATION)
                                    strBatchStringR += "LOOP 2310C SERVICE FACILITY LOCATION~";


                                    if (claim.claimInfo.Facility_Code != 0)
                                    {

                                        if (!string.IsNullOrEmpty(claim.claimInfo.Facility_Npi))
                                        {
                                            strBatchStringR += "Service Facility Name~";
                                            strBatchStringR += "NM1*77*2*" + claim.claimInfo.Facility_Name + "*****XX*"
                                                    + claim.claimInfo.Facility_Npi + "~";
                                            strBatchString += "NM1*77*2*" + claim.claimInfo.Facility_Name + "*****XX*"
                                                    + claim.claimInfo.Facility_Npi + "~";
                                        }
                                        else
                                        {
                                            strBatchStringR += "Service Facility Name~";
                                            strBatchStringR += "NM1*77*2*" + claim.claimInfo.Facility_Name + "*****XX*~";
                                            strBatchString += "NM1*77*2*" + claim.claimInfo.Facility_Name + "*****XX*~";
                                        }
                                        segmentCount++;

                                        if (string.IsNullOrEmpty(claim.claimInfo.Facility_Address)
                                                || string.IsNullOrEmpty(claim.claimInfo.Facility_City)
                                                || string.IsNullOrEmpty(claim.claimInfo.Facility_State)
                                                || string.IsNullOrEmpty(claim.claimInfo.Facility_Zip))
                                        {
                                            errorList.Add("Facility's address incomplete.");
                                        }

                                        strBatchStringR += "Service Facility Address~";
                                        strBatchStringR += "N3*" + claim.claimInfo.Facility_Address + "~";
                                        strBatchString += "N3*" + claim.claimInfo.Facility_Address + "~";
                                        segmentCount++;

                                        strBatchStringR += "Service Facility City, State, ZIP~";
                                        strBatchStringR += "N4*" + claim.claimInfo.Facility_City + "*"
                                                + claim.claimInfo.Facility_State + "*";
                                        strBatchString += "N4*" + claim.claimInfo.Facility_City + "*"
                                                + claim.claimInfo.Facility_State + "*";
                                        if (string.IsNullOrEmpty(claim.claimInfo.Facility_Zip))
                                        {
                                            strBatchStringR += "     " + "~";
                                            strBatchString += "     " + "~";
                                        }
                                        else
                                        {
                                            strBatchStringR += claim.claimInfo.Facility_Zip + "~";
                                            strBatchString += claim.claimInfo.Facility_Zip + "~";
                                        }
                                        segmentCount++;
                                    }


                                    #endregion


                                    if (SecondaryIns != null)
                                    {
                                        #region LOOP 2320
                                        strBatchStringR += "LOOP 2320 OTHER SUBSCRIBER INFORMATION~";

                                        #region OTHER SUBSCRIBER INFORMATION

                                        string SBR02_secondary = "18";

                                        if (!string.IsNullOrEmpty(SecondaryIns.GRelationship))
                                        {
                                            switch (SecondaryIns.GRelationship.ToUpper())
                                            {
                                                case "C":// Child
                                                    SBR02_secondary = "19";
                                                    break;
                                                case "P"://SPOUSE
                                                    SBR02_secondary = "01";
                                                    break;
                                                case "S"://Self
                                                    SBR02_secondary = "18";
                                                    break;
                                                case "O": // Other
                                                    SBR02_secondary = "G8";
                                                    break;
                                            }
                                        }

                                        strBatchStringR += "OTHER SUBSCRIBER INFORMATION~";
                                        strBatchStringR += "SBR*S*";
                                        strBatchString += "SBR*S*";
                                        string PlanNameSec = "", InsPayerTypeCodeSec = "", payerTypeCodeSec = "";

                                        if (!string.IsNullOrEmpty(SecondaryIns.Insgroup_Name) && SecondaryIns.Insgroup_Name.Contains("MEDICARE"))
                                    {
                                        if (!string.IsNullOrEmpty(SecondaryIns.plan_name) && SecondaryIns.plan_name.ToUpper().Contains("MEDICARE"))
                                        {
                                            PlanNameSec = SecondaryIns.plan_name;
                                        }
                                        else
                                        {
                                            PlanNameSec = "MEDICARE";
                                        }

                                        // Changing for SBR09 Error by TriZetto
                                        payerTypeCodeSec = SecondaryIns.insurance_type_code;
                                        //payerTypeCodeSec = "47"; //5010 required in case of medicare is secondary or ter.
                                        /*                        
                                         12	Medicare Secondary Working Aged Beneficiary or Spouse with Employer Group Health Plan
                                         13	Medicare Secondary End Stage Renal Disease
                                         14	Medicare Secondary , No Fault Insurance including Auto is Primary
                                         15	Medicare Secondary Worker’s Compensation
                                         16	Medicare Secondary Public Health Service (PHS) or other Federal Agency
                                         16	Medicare Secondary Public Health Service
                                         41	Medicare Secondary Black Lung
                                         42	Medicare Secondary Veteran’s Administration
                                         43	Medicare Secondary Veteran’s Administration
                                         47	Medicare Secondary, Other Liability Insurance is Primary
                                         */

                                    }
                                        else
                                        {
                                            PlanNameSec = SecondaryIns.plan_name;
                                            payerTypeCodeSec = SecondaryIns.insurance_type_code;
                                        }


                                        strBatchStringR += SBR02_secondary + "*" + SecondaryIns.Group_Number + "*" + PlanNameSec + "*" + InsPayerTypeCodeSec + "****" + payerTypeCodeSec + "~";
                                        strBatchString += SBR02_secondary + "*" + SecondaryIns.Group_Number + "*" + PlanNameSec + "*" + InsPayerTypeCodeSec + "****" + payerTypeCodeSec + "~";
                                        segmentCount++;

                                        #endregion

                                        #region OTHER INSURANCE COVERAGE INFORMATION

                                        if (!string.IsNullOrEmpty(SecondaryIns.GRelationship)
                                   && SecondaryIns.GRelationship.ToUpper().Equals("S"))
                                        {
                                            strBatchStringR += "OTHER INSURANCE COVERAGE INFORMATION~";
                                            strBatchStringR += "OI***Y*P**Y~"; //- Changed C to P as per 5010
                                            strBatchString += "OI***Y*P**Y~";
                                            segmentCount++;

                                        }
                                        else
                                        {
                                            strBatchStringR += "OTHER INSURANCE COVERAGE INFORMATION~";
                                            strBatchStringR += "OI***Y*P**Y~"; //- Changed C to P as per 5010
                                            strBatchString += "OI***Y*P**Y~";
                                            segmentCount++;
                                        }


                                        #endregion

                                        #endregion

                                        #region LOOP 2330A (OTHER SUBSCRIBER NAME and Address)
                                        strBatchStringR += "Loop ID - 2330A - Other Subscriber Name~";
                                        if (!string.IsNullOrEmpty(SecondaryIns.GRelationship)
                                    && SecondaryIns.GRelationship.ToUpper().Trim().Equals("S"))
                                        {
                                            strBatchStringR += "Other Subscriber Name~";
                                            strBatchStringR += "NM1*IL*1*";
                                            strBatchString += "NM1*IL*1*";

                                            if (string.IsNullOrEmpty(claim.claimInfo.Lname) || string.IsNullOrEmpty(claim.claimInfo.Fname))
                                            {
                                                errorList.Add("Self -- Secondary Insurnace'subscriber Last/First Name missing.");
                                            }
                                            else
                                            {
                                                strBatchStringR += claim.claimInfo.Lname + "*"
                                                        + claim.claimInfo.Fname + "*"
                                                        + claim.claimInfo.Mname + "***MI*"
                                                        + SecondaryIns.Policy_Number.ToUpper() + "~";
                                                strBatchString += claim.claimInfo.Lname + "*"
                                                       + claim.claimInfo.Fname + "*"
                                                       + claim.claimInfo.Mname + "***MI*"
                                                       + SecondaryIns.Policy_Number.ToUpper() + "~";
                                                segmentCount++;
                                            }
                                            if (string.IsNullOrEmpty(claim.claimInfo.Address)
                                                    || string.IsNullOrEmpty(claim.claimInfo.City)
                                                    || string.IsNullOrEmpty(claim.claimInfo.State)
                                                    || string.IsNullOrEmpty(claim.claimInfo.Zip))
                                            {
                                                errorList.Add("Self -- Subscriber Address incomplete.");
                                            }
                                            else
                                            {
                                                strBatchStringR += "Other Subscriber Address~";
                                                strBatchStringR += "N3*" + claim.claimInfo.Address + "~";
                                                strBatchString += "N3*" + claim.claimInfo.Address + "~";
                                                segmentCount++;

                                                strBatchStringR += "Other Subscriber City, State, ZIP~";
                                                strBatchStringR += "N4*" + claim.claimInfo.City + "*"
                                                        + claim.claimInfo.State + "*";
                                                strBatchString += "N4*" + claim.claimInfo.City + "*"
                                                        + claim.claimInfo.State + "*";
                                                if (string.IsNullOrEmpty(claim.claimInfo.Zip))
                                                {
                                                    strBatchStringR += "     " + "~";
                                                    strBatchString += "     " + "~";
                                                }
                                                else
                                                {
                                                    strBatchStringR += claim.claimInfo.Zip + "~";
                                                    strBatchString += claim.claimInfo.Zip + "~";
                                                }
                                                segmentCount++;
                                            }
                                        }
                                        else
                                        {
                                            strBatchStringR += "Other Subscriber Name~";
                                            strBatchStringR += "NM1*IL*1*";
                                            strBatchString += "NM1*IL*1*";

                                            if (string.IsNullOrEmpty(SecondaryIns.Glname) || string.IsNullOrEmpty(SecondaryIns.Gfname))
                                            {
                                                errorList.Add("Secondary Insurnace'subscriber Last/First Name missing.");

                                            }
                                            else
                                            {
                                                strBatchStringR += SecondaryIns.Glname + "*"
                                                        + SecondaryIns.Gfname + "*"
                                                        + SecondaryIns.Gmi + "***MI*"
                                                        + SecondaryIns.Policy_Number.ToUpper() + "~";
                                                strBatchString += SecondaryIns.Glname + "*"
                                                        + SecondaryIns.Gfname + "*"
                                                        + SecondaryIns.Gmi + "***MI*"
                                                        + SecondaryIns.Policy_Number.ToUpper() + "~";
                                                segmentCount++;
                                            }
                                            if (string.IsNullOrEmpty(SecondaryIns.Gaddress)
                                                    || string.IsNullOrEmpty(SecondaryIns.Gcity)
                                                    || string.IsNullOrEmpty(SecondaryIns.Gstate)
                                                    || string.IsNullOrEmpty(SecondaryIns.Gzip))
                                            {
                                                errorList.Add("Secondary Subscriber Address incomplete.");
                                            }
                                            else
                                            {
                                                strBatchStringR += "Other Subscriber Address~";
                                                strBatchStringR += "N3*" + SecondaryIns.Gaddress + "~";
                                                strBatchString += "N3*" + SecondaryIns.Gaddress + "~";
                                                segmentCount++;

                                                strBatchStringR += "Other Subscriber City, State, ZIP~";
                                                strBatchStringR += "N4*" + SecondaryIns.Gcity + "*"
                                                        + SecondaryIns.Gstate + "*";

                                                strBatchString += "N4*" + SecondaryIns.Gcity + "*"
                                                        + SecondaryIns.Gstate + "*";
                                                if (string.IsNullOrEmpty(SecondaryIns.Gzip))
                                                {
                                                    strBatchStringR += "     " + "~";
                                                    strBatchString += "     " + "~";
                                                }
                                                else
                                                {
                                                    strBatchStringR += SecondaryIns.Gzip + "~";
                                                    strBatchString += SecondaryIns.Gzip + "~";
                                                }
                                                segmentCount++;
                                            }
                                        }
                                        #endregion


                                        #region LOOP 2330B (OTHER PAYER AND AND ADDRESS)
                                        strBatchStringR += "Loop ID -2330B - Other Payer Name~";

                                        string SecInsPayerName = "";
                                        if (string.IsNullOrEmpty(SecondaryIns.plan_name))
                                        {
                                            errorList.Add("Secondary's payer name missing.");
                                        }
                                        else
                                        {
                                            if (SecondaryIns.Insgroup_Name.Trim().Contains("MEDICARE"))
                                            {
                                                SecInsPayerName = "MEDICARE";
                                            }
                                            else
                                            {
                                                SecInsPayerName = SecondaryIns.plan_name;
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(SecondaryIns.Payer_Number))
                                        {
                                            string secPayerNumber = primaryIns.Payer_Number.Equals(SecondaryIns.Payer_Number) ? SecondaryIns.Payer_Number + "A" : SecondaryIns.Payer_Number;
                                            strBatchStringR += " Other Payer Name~";
                                            strBatchStringR += "NM1*PR*2*" + SecInsPayerName + "*****PI*" + secPayerNumber + "~";
                                            strBatchString += "NM1*PR*2*" + SecInsPayerName + "*****PI*" + secPayerNumber + "~";
                                            segmentCount++;
                                        }
                                        else
                                        {
                                            errorList.Add("Secondary's insurance payer id is compulsory in case of Gateway EDI Clearing house.");
                                        }

                                        //Obsolete
                                        //strBatchString += "N3*" + SecondaryIns.Gaddress + "~";
                                        //segmentCount++;
                                        //strBatchString += "N4*" + SecondaryIns.Gcity + "*" + SecondaryIns.Gstate + "*" + SecondaryIns.Gzip.Trim() + "~";
                                        //segmentCount++;

                                        strBatchStringR += "Other Payer Address~";
                                        strBatchStringR += "N3*" + SecondaryIns.Ins_Address + "~";
                                        strBatchString += "N3*" + SecondaryIns.Ins_Address + "~";
                                        segmentCount++;
                                        strBatchStringR += "Other Payer City, State, ZIP~";
                                        strBatchStringR += "N4*" + SecondaryIns.Ins_City + "*" + SecondaryIns.Ins_State + "*" + SecondaryIns.Ins_Zip.Trim() + "~";
                                        strBatchString += "N4*" + SecondaryIns.Ins_City + "*" + SecondaryIns.Ins_State + "*" + SecondaryIns.Ins_Zip.Trim() + "~";
                                        segmentCount++;

                                        #endregion
                                    }
                                    #region New:NM1*DQ - SUPERVISING PROVIDER
                                    //var claimid = claim.claim_No;
                                    //if (sPDataModels != null && sPDataModels.Any())
                                    //{
                                    //    strBatchString += "NM1*DQ*1*" + sPDataModels[0].ProviderLNamer + "*"
                                    //                  + sPDataModels[0].ProviderFNamer + "****XX*"
                                    //                  + claim.claimInfo.Att_Npi + "~";
                                    //    segmentCount++;
                                    //}
                                    // Above code is commented and below is added by Shahzad Khan EDI for live Fixation
                                    if (sPDataModels != null && sPDataModels.Any())
                                    {
                                        var ProviderL = sPDataModels[0].ProviderLNamer;

                                        if ((!string.IsNullOrEmpty(ProviderL) && (claim.claimInfo.Att_Lname != sPDataModels[0].ProviderLNamer && claim.claimInfo.Att_Fname != sPDataModels[0].ProviderFNamer)))
                                        {
                                            strBatchString += "NM1*DQ*1*" + sPDataModels[0].ProviderLNamer + "*"
                                                       + sPDataModels[0].ProviderFNamer + "****XX*"
                                                       + sPDataModels[0].ProviderNpi + "~";
                                            segmentCount++;
                                        }



                                }
                                    #endregion End!

                                    //---Adding Submit/RESUBMIT CLAIM CPTS-----------
                                    int line_no = 0;
                                    if (claim.claimProcedures != null && claim.claimProcedures.Count() > 0)
                                    {
                                        foreach (var proc in claim.claimProcedures)
                                        {

                                            if (claim.claimInfo.Is_Resubmitted && !proc.Is_Resubmitted)
                                            {
                                                continue;
                                            }

                                            line_no = line_no + 1;

                                            #region LOOP 2400
                                            strBatchStringR += "Loop ID -2400 - Service Line Number~";


                                            #region SERVICE LINE   
                                            strBatchStringR += "Service Line Number~";
                                            strBatchStringR += "LX*" + line_no + "~";
                                            strBatchString += "LX*" + line_no + "~";
                                            segmentCount++;
                                            #endregion


                                            #region PROFESSIONAL SERVICE
                                            if (!string.IsNullOrEmpty(claim.claimInfo.Claim_Pos))
                                            {

                                                if (proc.Total_Charges > 0)
                                                {
                                                    string modifiers = "";
                                                    if (!string.IsNullOrEmpty(proc.Mod1.Trim()))
                                                    {
                                                        modifiers += ":" + proc.Mod1.Trim();
                                                    }
                                                    else
                                                    {
                                                        modifiers += ":";
                                                    }
                                                    if (!string.IsNullOrEmpty(proc.Mod2.Trim()))
                                                    {
                                                        modifiers += ":" + proc.Mod2.Trim();
                                                    }
                                                    else
                                                    {
                                                        modifiers += ":";
                                                    }
                                                    if (!string.IsNullOrEmpty(proc.Mod3.Trim()))
                                                    {
                                                        modifiers += ":" + proc.Mod3.Trim();
                                                    }
                                                    else
                                                    {
                                                        modifiers += ":";
                                                    }
                                                    if (!string.IsNullOrEmpty(proc.Mod4.Trim()))
                                                    {
                                                        modifiers += ":" + proc.Mod4.Trim();
                                                    }
                                                    else
                                                    {
                                                        modifiers += ":";
                                                    }
                                                    strBatchStringR += "Professional Service~";
                                                    strBatchStringR += "SV1*HC:" + proc.Proc_Code.Trim() + modifiers + ":" + proc.ProcedureDescription + "*"
                                                            + string.Format("{0:0.00}", proc.Total_Charges) + "*UN*"
                                                            + proc.Units + "*"
                                                            + claim.claimInfo.Claim_Pos + "*"
                                                            + "*";
                                                    strBatchString += "SV1*HC:" + proc.Proc_Code.Trim() + modifiers + ":" + proc.ProcedureDescription + "*"
                                                            + string.Format("{0:0.00}", proc.Total_Charges) + "*UN*"
                                                            + proc.Units + "*"
                                                            + claim.claimInfo.Claim_Pos + "*"
                                                            + "*";
                                                }
                                                else
                                                {
                                                    errorList.Add("Procedure Code:  " + proc.Proc_Code.Trim() + " has ZERO charges");
                                                }
                                            }
                                            else
                                            {
                                                errorList.Add("Claim's pos code missing");
                                            }

                                            string pointers = "";
                                            if (proc.Dx_Pointer1 > 0)
                                            {
                                                pointers = proc.Dx_Pointer1.ToString();
                                            }
                                            if (proc.Dx_Pointer2 > 0)
                                            {
                                                pointers += ":" + proc.Dx_Pointer2.ToString();
                                            }
                                            if (proc.Dx_Pointer3 > 0)
                                            {
                                                pointers += ":" + proc.Dx_Pointer3.ToString();
                                            }
                                            if (proc.Dx_Pointer4 > 0)
                                            {
                                                pointers += ":" + proc.Dx_Pointer4.ToString();
                                            }

                                            strBatchStringR += pointers + "~";
                                            strBatchString += pointers + "~";
                                            segmentCount++;

                                            #endregion

                                            #region SERVICE Date

                                            strBatchStringR += "Service Date~";
                                            strBatchStringR += "DTP*472*RD8*";
                                            strBatchString += "DTP*472*RD8*";

                                            string[] splittedFROMDOS = proc.DosFrom.Split('/');
                                            string[] splittedTODOS = proc.Dos_To.Split('/');
                                            string Date_Of_Service_FROM = splittedFROMDOS[0] + splittedFROMDOS[1] + splittedFROMDOS[2];
                                            string Date_Of_Service_TO = splittedTODOS[0] + splittedTODOS[1] + splittedTODOS[2];
                                            strBatchStringR += Date_Of_Service_FROM + "-" + Date_Of_Service_TO + "~";

                                            strBatchString += Date_Of_Service_FROM + "-" + Date_Of_Service_TO + "~";
                                            segmentCount++;
                                            #endregion

                                            #region LINE ITEM CONTROL NUMBER (CLAIM PROCEDURES ID)
                                            strBatchStringR += "Line Item Control Number~";
                                            strBatchStringR += "REF*6R*" + proc.Claim_Procedures_Id.ToString() + "~";
                                            strBatchString += "REF*6R*" + proc.Claim_Procedures_Id.ToString() + "~";
                                            segmentCount++;
                                            #endregion

                                            #region LINE Note
                                            if (!string.IsNullOrEmpty(proc.Notes.Trim()))
                                            {
                                                strBatchStringR += "LINE Note~";
                                                strBatchStringR += "NTE*ADD*" + proc.Notes.Trim() + "~";
                                                strBatchString += "NTE*ADD*" + proc.Notes.Trim() + "~";
                                                segmentCount++;
                                            }

                                            #endregion

                                            #endregion


                                            #region LOOP 2410 (DRUG IDENTIFICATION)
                                            strBatchStringR += "LOOP - 2410 - DRUG IDENTIFICATION~";


                                            if (!string.IsNullOrEmpty(proc.Ndc_Code))
                                            {
                                                strBatchStringR += "Procdure NDC Code~";
                                                strBatchStringR += "LIN**N4*" + proc.Ndc_Code.Trim() + "~";
                                                strBatchString += "LIN**N4*" + proc.Ndc_Code.Trim() + "~";
                                                segmentCount++;
                                                if (proc.Ndc_Qty > 0)
                                                {
                                                    if (!string.IsNullOrEmpty(proc.Ndc_Measure))
                                                    {
                                                        strBatchStringR += "Procdure  Ndc_Qty Ndc_Measure~";
                                                        strBatchStringR += "CTP****" + proc.Ndc_Qty.ToString() + "*" + proc.Ndc_Measure + "*~";
                                                        strBatchString += "CTP****" + proc.Ndc_Qty.ToString() + "*" + proc.Ndc_Measure + "*~";
                                                        segmentCount++;
                                                    }
                                                    else
                                                    {
                                                        errorList.Add("Procedure NDC Quantity/Qual or Unit Price is missing.");
                                                    }
                                                }

                                            }

                                            #endregion
                                        }
                                    }
                                    if (line_no == 0)
                                    {
                                        errorList.Add("Claim Procedures missing.");
                                    }
                                }
                            }

                        }
                    }
                    if (errorList.Count == 0)
                    {
                        segmentCount += 3;
                        strBatchStringR += "~SE*" + segmentCount + "*0001~";
                        strBatchStringR += "GE * 1 * " + batchId + "~";
                        strBatchStringR += "IEA*1*000000001~";

                        strBatchString += "SE*" + segmentCount + "*0001~GE*1*" + batchId + "~IEA*1*000000001~";

                        objResponse.Status = "Success";
                        objResponse.Response = strBatchString;
                        objResponse.Response1 = strBatchStringR;

                        //using (var w = new StreamWriter(HttpContext.Current.Server.MapPath("/SubmissionFile/" + claim_id + ".txt"), false))
                        //{
                        //    w.WriteLine(strBatchString);
                        //}

                    }
                    else
                    {
                        objResponse.Status = "Error";
                        objResponse.Response = errorList;
                        objResponse.Response1 = errorList;

                    }

                
                


            }
            catch (Exception)
            {
                throw;
            }
            return objResponse;
        }

        public ResponseModel GenerateBatch_5010_P_S(long practice_id, long claim_id)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                var pri_Status = "";
                var sec_Status = "";
                var oth_Status = "";
                var Claim_Type = "";
                using (var ctx = new NPMDBEntities())
                {
                    pri_Status = ctx.Claims.Where(c => c.Claim_No == claim_id)
                                       .Select(c => c.Pri_Status).FirstOrDefault();
                    sec_Status = ctx.Claims.Where(c => c.Claim_No == claim_id)
                                       .Select(c => c.Sec_Status).FirstOrDefault();
                    oth_Status = ctx.Claims.Where(c => c.Claim_No == claim_id)
                                       .Select(c => c.Oth_Status).FirstOrDefault();
                    Claim_Type = ctx.Claims.Where(c => c.Claim_No == claim_id)
                                       .Select(c => c.Claim_Type).FirstOrDefault();
                }
                    //..secondary claim scenario

                    var matchedData = new List<dynamic>();
                    List<DataModel> dataModels = new List<DataModel>();
                    DateTime procDosFrom = DateTime.MinValue;
                    DateTime procDosTo = DateTime.MinValue;
                    DateTime procDosFrom_test = DateTime.MinValue;
                    DateTime procDosTo_test = DateTime.MinValue;
                    string strBatchString = "";
                    string strBatchStringR = "";
                    int segmentCount = 0;
                    List<string> errorList;

                    //string billingOrganizationName = "practiceName";//practiceName
                    string sumbitterId = "";
                    string submitterCompanyName = "";
                    string submitterContactPerson = "";
                    string submitterCompanyEmail = "";
                    string submitterCompanyPhone = "";
                    string batchId = "";
                    string secStatus = "";
                    errorList = new List<string>();

                    List<spGetBatchCompanyDetails_Result> batchCompanyInfo = null;
                    List<spGetBatchClaimsInfo_Result> batchClaimInfo = null;
                    List<spGetBatchClaimsDiagnosis_Result> batchClaimDiagnosis = null;
                    List<spGetBatchClaimsProcedurestest_Result> batchClaimProcedures = null;
                    List<spGetBatchClaimsInsurancesInfo_Result> insuraceInfo = null;
                    List<SPDataModel> sPDataModels = null;

                    List<ClaimSubmissionModel> claimSubmissionInfo = new List<ClaimSubmissionModel>();
                    List<DataModel> datamodel = new List<DataModel>();

                    using (var ctx = new NPMDBEntities())
                    {
                        batchCompanyInfo = ctx.spGetBatchCompanyDetails(practice_id.ToString()).ToList();
                    }

                    if (batchCompanyInfo != null && batchCompanyInfo.Count > 0)
                    {
                        sumbitterId = batchCompanyInfo[0].Submitter_Id;
                        submitterCompanyName = batchCompanyInfo[0].Company_Name;
                        submitterContactPerson = batchCompanyInfo[0].Contact_Person;
                        submitterCompanyEmail = batchCompanyInfo[0].Company_Email;
                        submitterCompanyPhone = batchCompanyInfo[0].Company_Phone;
                    }

                    if (string.IsNullOrEmpty(sumbitterId))
                    {
                        errorList.Add("Patient Submitter ID is missing.");
                    }
                    if (string.IsNullOrEmpty(submitterCompanyName))
                    {
                        errorList.Add("Company ClearingHouse information is missing.");
                    }
                    if (string.IsNullOrEmpty(submitterCompanyEmail) && string.IsNullOrEmpty(submitterCompanyPhone))
                    {
                        errorList.Add("Submitter Contact Information is Missing.");
                    }

                    if (errorList.Count == 0)
                    {
                        using (var ctx = new NPMDBEntities())
                        {
                            batchClaimInfo = ctx.spGetBatchClaimsInfo(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                            batchClaimDiagnosis = ctx.spGetBatchClaimsDiagnosis(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                            batchClaimProcedures = ctx.spGetBatchClaimsProcedurestest(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                            insuraceInfo = ctx.spGetBatchClaimsInsurancesInfo(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                            secStatus = ctx.Claims.Where(c => c.Claim_No == claim_id)
                                .Select(c => c.Sec_Status).FirstOrDefault();
                            sPDataModels = getSpResult(claim_id.ToString(), "P").ToList();
                        }

                        foreach (var claim in batchClaimInfo)
                        {

                            if (claim.Patient_Id == null)
                            {
                                errorList.Add("Patient identifier is missing. DOS:" + claim.Dos);
                            }
                            else if (claim.Billing_Physician == null)
                            {
                                errorList.Add("Billing Physician identifier is missing. DOS:" + claim.Dos);
                            }


                            IEnumerable<spGetBatchClaimsInsurancesInfo_Result> claimInsurances = (from ins in insuraceInfo
                                                                                                  where ins.Claim_No == claim.Claim_No
                                                                                                  select ins).ToList();

                            spGetBatchClaimsDiagnosis_Result claimDiagnosis = (from spGetBatchClaimsDiagnosis_Result diag in batchClaimDiagnosis
                                                                               where diag.Claim_No == claim.Claim_No
                                                                               select diag).FirstOrDefault();

                            IEnumerable<spGetBatchClaimsProcedurestest_Result> claimProcedures = (from spGetBatchClaimsProcedurestest_Result proc in batchClaimProcedures
                                                                                                  where proc.Claim_No == claim.Claim_No
                                                                                                  select proc).ToList();







                            ClaimSubmissionModel claimSubmissionModel = new ClaimSubmissionModel();
                            claimSubmissionModel.claim_No = claim.Claim_No;
                            claimSubmissionModel.claimInfo = claim;
                            claimSubmissionModel.claimInsurance = claimInsurances as List<spGetBatchClaimsInsurancesInfo_Result>;
                            claimSubmissionModel.claimDiagnosis = claimDiagnosis as spGetBatchClaimsDiagnosis_Result;
                            claimSubmissionModel.claimProcedures = claimProcedures as List<spGetBatchClaimsProcedurestest_Result>;



                            List<uspGetBatchClaimsProviderPayersDataFromUSP_Result> claimBillingProviderPayerInfo;
                            foreach (var ins in claimInsurances)
                            {
                                if (ins.Insurace_Type.Trim().ToUpper().Equals("P") && ins.Inspayer_Id != null)//primary
                                {

                                    using (var ctx = new NPMDBEntities())
                                    {
                                        claimBillingProviderPayerInfo = ctx.uspGetBatchClaimsProviderPayersDataFromUSP(ins.Inspayer_Id.ToString(), claim.Claim_No.ToString(), "CLAIM_ID").ToList();

                                        if (claimBillingProviderPayerInfo != null && claimBillingProviderPayerInfo.Count > 0)
                                        {
                                            claimSubmissionModel.claimBillingProviderPayer = claimBillingProviderPayerInfo[0];
                                        }
                                    }
                                    break;
                                }
                            }

                            /*
                             * Assign Other objects of hospital claim
                             *  
                             * 
                             * */
                            claimSubmissionInfo.Add(claimSubmissionModel);

                        }

                        if (claimSubmissionInfo.Count > 0)
                        {
                            //batchId = claimSubmissionInfo[0].claim_No.ToString(); // Temporariy ... will be populated by actual batch id.
                            string claimNumber = claimSubmissionInfo[0].claim_No.ToString();
                            batchId = claimNumber.Substring(3);
                            string dateTime_yyMMdd = DateTime.Now.ToString("yyMMdd");
                            string dateTime_yyyyMMdd = DateTime.Now.ToString("yyyyMMdd");
                            string dateTime_HHmm = DateTime.Now.ToString("HHmm");

                            // ISA02 Authorization Information AN 10 - 10 R
                            string authorizationInfo = string.Empty.PadRight(10);// 10 characters

                            //ISA04 Security Information AN 10-10 R
                            string securityInfo = string.Empty.PadRight(10);// 10 characters

                            segmentCount = 0;

                            #region ISA Header
                            // INTERCHANGE CONTROL HEADER
                            strBatchStringR = "~Interchange Control Header~";
                            strBatchStringR += "ISA*";
                            strBatchStringR += "00*" + authorizationInfo + "*00*" + securityInfo + "*ZZ*" + sumbitterId.PadRight(15) + "*ZZ*263923727000000*";
                            strBatchStringR += dateTime_yyMMdd + "*";
                            strBatchStringR += dateTime_HHmm + "*";
                            strBatchStringR += "^*00501*000000001*0*P*:~";

                            strBatchString = "ISA*";
                            strBatchString += "00*" + authorizationInfo + "*00*" + securityInfo + "*ZZ*" + sumbitterId.PadRight(15) + "*ZZ*263923727000000*";
                            strBatchString += dateTime_yyMMdd + "*";
                            strBatchString += dateTime_HHmm + "*";
                            strBatchString += "^*00501*000000001*0*P*:~";
                            segmentCount++;
                            //FUNCTIONAL GROUP HEADER
                            strBatchStringR += "Functional Group Header~";
                            strBatchStringR += "GS*HC*" + sumbitterId + "*263923727*";
                            strBatchStringR += dateTime_yyyyMMdd + "*";
                            strBatchStringR += dateTime_HHmm + "*";
                            strBatchStringR += batchId.ToString() + "*X*005010X222A1~";

                            strBatchString += "GS*HC*" + sumbitterId + "*263923727*";
                            strBatchString += dateTime_yyyyMMdd + "*";
                            strBatchString += dateTime_HHmm + "*";
                            strBatchString += batchId.ToString() + "*X*005010X222A1~";  //-->5010 GS08 Changed from 004010X098A1 to 005010X222 in 5010
                                                                                        // need to send batch_id in GS06 instead of 16290 so that can be traced from 997 response file
                            segmentCount++;
                            //TRANSACTION SET HEADER
                            strBatchStringR += "Transaction Set Header~";
                            strBatchStringR += "ST*837*0001*005010X222A1~";

                            strBatchString += "ST*837*0001*005010X222A1~";  //-->5010 new element addedd. ST03 Implementation Convention Reference (005010X222)
                            segmentCount++;
                            //BEGINNING OF HIERARCHICAL TRANSACTION
                            strBatchStringR += "Beginning of Hierarchical Transaction~";
                            strBatchStringR += "BHT*0019*00*000000001*";
                            strBatchStringR += dateTime_yyyyMMdd + "*";
                            strBatchStringR += dateTime_HHmm + "*";
                            strBatchStringR += "CH~";

                            strBatchString += "BHT*0019*00*000000001*";
                            strBatchString += dateTime_yyyyMMdd + "*";
                            strBatchString += dateTime_HHmm + "*";
                            strBatchString += "CH~";
                            segmentCount++;

                            #endregion

                            #region LOOP 1000A (Sumbitter Information)
                            strBatchStringR += "Loop ID - 1000A - Submitter Name~";



                            #region Submitter Company Name
                            strBatchStringR += "Submitter Name~";
                            strBatchStringR += "NM1*41*2*";  //-->5010 NM103  Increase from 35 - 60
                            strBatchStringR += submitterCompanyName; // -->5010 NM104  Increase from 25 - 35
                            strBatchStringR += "*****46*" + sumbitterId;// -->5010 New element added NM112 Name Last or Organization Name 1-60
                            strBatchStringR += "~";

                            strBatchString += "NM1*41*2*";  //-->5010 NM103  Increase from 35 - 60
                            strBatchString += submitterCompanyName; // -->5010 NM104  Increase from 25 - 35
                            strBatchString += "*****46*" + sumbitterId;// -->5010 New element added NM112 Name Last or Organization Name 1-60
                            strBatchString += "~";
                            segmentCount++;
                            #endregion

                            #region SUBMITTER EDI CONTACT INFORMATION
                            strBatchStringR += "Submitter Contact Information~";
                            strBatchStringR += "PER*IC*";

                            strBatchString += "PER*IC*";
                            if (!string.IsNullOrEmpty(submitterContactPerson))
                            {
                                strBatchStringR += submitterContactPerson;
                                strBatchString += submitterContactPerson;
                            }

                            if (!string.IsNullOrEmpty(submitterCompanyPhone))
                            {
                                strBatchStringR += "*TE*" + submitterCompanyPhone.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim();
                                strBatchString += "*TE*" + submitterCompanyPhone.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim();

                            }
                            if (!string.IsNullOrEmpty(submitterCompanyEmail))
                            {
                                strBatchStringR += "*EM*" + submitterCompanyEmail;
                                strBatchString += "*EM*" + submitterCompanyEmail;
                            }
                            strBatchStringR += "~";
                            strBatchString += "~";
                            segmentCount++;
                            #endregion

                            #endregion

                            #region LOOP 1000B (RECEIVER NAME)
                            strBatchStringR += "Loop ID - 1000B - Receiver Name~";
                            strBatchStringR += "Receiver Name~";
                            strBatchStringR += "NM1*40*2*263923727000000*****46*" + sumbitterId + "~";

                            strBatchString += "NM1*40*2*263923727000000*****46*" + sumbitterId + "~";
                            segmentCount++;
                            #endregion

                            int HL = 1;


                            foreach (var claim in claimSubmissionInfo)
                            {
                                long patientId = (long)claim.claimInfo.Patient_Id;
                                long claimId = claim.claimInfo.Claim_No;
                                string claim_id1 = claim_id.ToString();
                                string DOS = claim.claimInfo.Dos;
                                string patientName = claim.claimInfo.Lname + ", " + claim.claimInfo.Fname;

                                string paperPayerID = "";
                                string Billing_Provider_NPI = "";
                                string TaxonomyCode = "";
                                string FederalTaxID = "";
                                string FederalTaxIDType = "";

                                string box_33_type = "";

                                #region Check If Payer Validation Expires
                                // check if payer validation expires

                                if (claim.claimBillingProviderPayer != null)
                                {
                                    if (string.IsNullOrEmpty(claim.claimBillingProviderPayer.Validation_Expiry_Date.ToString()) && claim.claimBillingProviderPayer.Validation_Expiry_Date.ToString() != "01/01/1900")
                                    {

                                        string validationExpriyDate = claim.claimBillingProviderPayer.Validation_Expiry_Date.ToString();
                                        DateTime dtExpiry = DateTime.Parse(validationExpriyDate);
                                        DateTime dtToday = new DateTime();

                                        if (DateTime.Compare(dtExpiry, dtToday) >= 0) // expires
                                        {
                                            errorList.Add("VALIDATION EXPIRED : Provider validation with the Payer has been expired.");
                                        }

                                    }
                                }
                                #endregion

                                #region Provider NPI/Group NPI on the basis of Box 33 Type . Group or Individual | Federal Tax ID | Box33                         
                                if (claim.claimBillingProviderPayer != null)
                                {
                                    if (!string.IsNullOrEmpty(claim.claimBillingProviderPayer.Provider_Identification_Number_Type)
                                        && !string.IsNullOrEmpty(claim.claimBillingProviderPayer.Provider_Identification_Number))
                                    {

                                        FederalTaxIDType = claim.claimBillingProviderPayer.Provider_Identification_Number_Type;
                                        FederalTaxID = claim.claimBillingProviderPayer.Provider_Identification_Number;
                                    }

                                    if (!string.IsNullOrEmpty(claim.claimBillingProviderPayer.Box_33_Type))
                                    {
                                        box_33_type = claim.claimBillingProviderPayer.Box_33_Type;
                                    }
                                }
                                if (string.IsNullOrEmpty(FederalTaxIDType) || string.IsNullOrEmpty(FederalTaxID))
                                {
                                    FederalTaxIDType = claim.claimInfo.Federal_Taxidnumbertype;
                                    FederalTaxID = claim.claimInfo.Federal_Taxid;
                                }



                                if (string.IsNullOrEmpty(box_33_type))
                                {
                                    switch (FederalTaxIDType)
                                    {
                                        case "EIN": // Group
                                            box_33_type = "GROUP";
                                            break;
                                        case "SSN": // Individual
                                            box_33_type = "INDIVIDUAL";
                                            break;
                                    }
                                }
                                switch (box_33_type)
                                {
                                    case "GROUP": // Group  
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Bl_Group_Npi))
                                        {
                                            Billing_Provider_NPI = claim.claimInfo.Bl_Group_Npi;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Grp_Taxonomy_Id))
                                        {
                                            TaxonomyCode = claim.claimInfo.Grp_Taxonomy_Id;
                                        }
                                        break;
                                    case "INDIVIDUAL": // Individual
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Bl_Npi))
                                        {
                                            Billing_Provider_NPI = claim.claimInfo.Bl_Npi;
                                        }

                                        if (!string.IsNullOrEmpty(claim.claimInfo.Taxonomy_Code))
                                        {
                                            TaxonomyCode = claim.claimInfo.Taxonomy_Code;
                                        }
                                        break;
                                }
                                #endregion

                                #region LOOP 2000A
                                strBatchStringR += "Loop ID - 2000A - Billing Provider Hierarchical  Level~";

                                #region BILLING PROVIDER HIERARCHICAL LEVEL
                                strBatchStringR += "Billing Provider Hierarchical Level~";
                                strBatchStringR += "HL*" + HL + "**";
                                strBatchStringR += "20*1~";

                                strBatchString += "HL*" + HL + "**";
                                strBatchString += "20*1~";
                                segmentCount++;

                                #endregion

                                #region BILLING PROVIDER SPECIALTY INFORMATION
                                strBatchStringR += "Billing Provider Specialty Information~";
                                strBatchStringR += "PRV*BI*PXC*" + TaxonomyCode + "~";

                                strBatchString += "PRV*BI*PXC*" + TaxonomyCode + "~";
                                segmentCount++;

                                #endregion

                                #endregion

                                #region LOOP 2010AA (Billing Provider Information)
                                strBatchStringR += "Loop ID - 2010AA - Billing Provider Name~";

                                #region Billing Provider Name
                                strBatchStringR += "Billing Provider Name~";

                                switch (box_33_type)
                                {
                                    case "GROUP": // Group                                                        
                                        if (!string.IsNullOrEmpty(submitterCompanyName))
                                        {

                                            strBatchStringR += "NM1*85*2*";
                                            strBatchStringR += submitterCompanyName + "*****XX*";
                                            strBatchString += "NM1*85*2*";
                                            strBatchString += submitterCompanyName + "*****XX*";

                                        }
                                        else
                                        {
                                            errorList.Add("2010AA - Billing Provider Organization Name Missing.");
                                        }

                                        if (!string.IsNullOrEmpty(Billing_Provider_NPI))
                                        {
                                            strBatchStringR += Billing_Provider_NPI;
                                            strBatchString += Billing_Provider_NPI;
                                        }
                                        else
                                        {
                                            errorList.Add("2010AA - Billing Provider Group NPI Missing.");
                                        }
                                        break;
                                    case "INDIVIDUAL": // Individual  
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Bl_Lname)
                                                && !string.IsNullOrEmpty(claim.claimInfo.Bl_Fname))
                                        {
                                            strBatchStringR += "NM1*85*1*";
                                            strBatchStringR += claim.claimInfo.Bl_Lname + "*" + claim.claimInfo.Bl_Fname + "*" + claim.claimInfo.Bl_Mi + "***XX*";

                                            strBatchString += "NM1*85*1*";
                                            strBatchString += claim.claimInfo.Bl_Lname + "*" + claim.claimInfo.Bl_Fname + "*" + claim.claimInfo.Bl_Mi + "***XX*";

                                        }
                                        else
                                        {
                                            errorList.Add("2010AA - Billing Provider Name Missing.");
                                        }

                                        if (!string.IsNullOrEmpty(Billing_Provider_NPI))
                                        {
                                            strBatchStringR += Billing_Provider_NPI;
                                            strBatchString += Billing_Provider_NPI;
                                        }
                                        else
                                        {
                                            errorList.Add("2010AA - Billing Provider Individual NPI Missing.");
                                        }

                                        break;
                                }
                                strBatchStringR += "~";
                                strBatchString += "~";
                                segmentCount++;

                                #endregion

                                #region BILLING PROVIDER ADDRESS
                                strBatchStringR += "Billing Provider Address~";

                                switch (box_33_type)
                                {
                                    case "GROUP": // Group                                                                               
                                        if (string.IsNullOrEmpty(claim.claimInfo.Bill_Address_Grp.Trim())
                                                || string.IsNullOrEmpty(claim.claimInfo.Bill_City_Grp.Trim())
                                                || string.IsNullOrEmpty(claim.claimInfo.Bill_State_Grp.Trim())
                                                || string.IsNullOrEmpty(claim.claimInfo.Bill_Zip_Grp.Trim()))
                                        {
                                            errorList.Add("BILLING ADDRESS ! Billing Provider Group Address is Missing.");
                                        }
                                        else
                                        {
                                            strBatchStringR += "N3*";
                                            strBatchStringR += claim.claimInfo.Bill_Address_Grp.Trim() + "~";
                                            strBatchString += "N3*";
                                            strBatchString += claim.claimInfo.Bill_Address_Grp.Trim() + "~";
                                            segmentCount++;

                                            strBatchStringR += "Billing Provider City, State, ZIP~";

                                            strBatchStringR += "N4*";
                                            strBatchStringR += claim.claimInfo.Bill_City_Grp.Trim() + "*";
                                            strBatchStringR += claim.claimInfo.Bill_State_Grp.Trim() + "*";

                                            strBatchString += "N4*";
                                            strBatchString += claim.claimInfo.Bill_City_Grp.Trim() + "*";
                                            strBatchString += claim.claimInfo.Bill_State_Grp.Trim() + "*";
                                            if (string.IsNullOrEmpty(claim.claimInfo.Bill_Zip_Grp.Trim()))
                                            {
                                                strBatchStringR += "     ";
                                                strBatchString += "     ";
                                            }
                                            else
                                            {
                                                strBatchString += claim.claimInfo.Bill_Zip_Grp.Trim() + "~";
                                            }
                                            segmentCount++;
                                        }
                                        break;
                                    case "INDIVIDUAL": // Individual  

                                        if (string.IsNullOrEmpty(claim.claimInfo.Bl_Address.Trim())
                                               || string.IsNullOrEmpty(claim.claimInfo.Bl_City.Trim())
                                               || string.IsNullOrEmpty(claim.claimInfo.Bl_State.Trim())
                                               || string.IsNullOrEmpty(claim.claimInfo.Bl_Zip.Trim()))
                                        {
                                            errorList.Add("BILLING ADDRESS ! Billing Provider Individual Address is Missing.");
                                        }
                                        else
                                        {
                                            strBatchStringR += "N3*";
                                            strBatchStringR += claim.claimInfo.Bl_Address.Trim() + "~";

                                            strBatchString += "N3*";
                                            strBatchString += claim.claimInfo.Bl_Address.Trim() + "~";
                                            segmentCount++;

                                            strBatchStringR += "Billing Provider City, State, ZIP~";
                                            strBatchStringR += "N4*";
                                            strBatchStringR += claim.claimInfo.Bl_City.Trim() + "*";
                                            strBatchStringR += claim.claimInfo.Bl_State.Trim() + "*";

                                            strBatchString += "N4*";
                                            strBatchString += claim.claimInfo.Bl_City.Trim() + "*";
                                            strBatchString += claim.claimInfo.Bl_State.Trim() + "*";
                                            if (string.IsNullOrEmpty(claim.claimInfo.Bl_Zip.Trim()))
                                            {
                                                strBatchString += "     ";
                                            }
                                            else
                                            {
                                                strBatchStringR += claim.claimInfo.Bl_Zip.Trim() + "~";
                                                strBatchString += claim.claimInfo.Bl_Zip.Trim() + "~";
                                            }
                                            segmentCount++;

                                        }

                                        break;
                                }


                                #endregion

                                #region BILLING PROVIDER Tax Identification
                                strBatchStringR += "Billing Provider Tax ID";

                                // hcfa box 25.. 
                                if (!string.IsNullOrEmpty(FederalTaxIDType) && !string.IsNullOrEmpty(FederalTaxID))
                                {
                                    if (FederalTaxIDType.Equals("EIN"))
                                    {
                                        strBatchStringR += "REF*EI*";
                                        strBatchString += "REF*EI*";
                                    }
                                    else if (FederalTaxIDType.Equals("SSN"))
                                    {
                                        strBatchStringR += "REF*SY*";
                                        strBatchString += "REF*SY*";
                                    }
                                    strBatchStringR += FederalTaxID + "~";
                                    strBatchString += FederalTaxID + "~";
                                    segmentCount += 1;
                                }
                                else
                                {
                                    errorList.Add("Billing provider federal tax id number/type missing.");
                                }

                                #endregion

                                #region  BILLING PROVIDER CONTACT INFORMATION
                                switch (FederalTaxIDType)
                                {
                                    case "EIN":
                                        if (!string.IsNullOrEmpty(submitterCompanyName)
                                                && !string.IsNullOrEmpty(claim.claimInfo.Phone_No))
                                        {
                                            strBatchStringR += "PER*IC*" + submitterCompanyName;
                                            strBatchStringR += "*TE*" + claim.claimInfo.Phone_No.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim() + "~";
                                            strBatchString += "PER*IC*" + submitterCompanyName;
                                            strBatchString += "*TE*" + claim.claimInfo.Phone_No.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim() + "~";
                                            segmentCount++;
                                        }
                                        else
                                        {
                                            errorList.Add("Billing Provider Contact Information Missing.");

                                        }
                                        break;
                                    case "SSN":
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Bl_Lname)
                                                && !string.IsNullOrEmpty(claim.claimInfo.Phone_No))
                                        {
                                            strBatchStringR += "PER*IC*" + claim.claimInfo.Bl_Lname;
                                            strBatchStringR += "*TE*" + claim.claimInfo.Phone_No.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim() + "~";
                                            strBatchString += "PER*IC*" + claim.claimInfo.Bl_Lname;
                                            strBatchString += "*TE*" + claim.claimInfo.Phone_No.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim() + "~";
                                            segmentCount++;
                                        }
                                        else
                                        {
                                            errorList.Add("Billing Provider Contact Information Missing.");
                                        }
                                        break;
                                }
                                #endregion

                                #endregion

                                #region LOOP 2010AB (PAY-TO ADDRESS NAME)
                                strBatchStringR += "LOOP 2010AB PAY-TO ADDRESS NAME";
                                strBatchStringR += "PAY -TO PROVIDER NAME";

                                switch (box_33_type)
                                {
                                    case "GROUP": // Group
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Bill_Address_Grp?.Trim())
                                            && !string.IsNullOrEmpty(claim.claimInfo.Pay_To_Address_Grp?.Trim())
                                            && !claim.claimInfo.Bill_Address_Grp.Trim().Equals(claim.claimInfo.Pay_To_Address_Grp.Trim()))
                                        {
                                            if (!string.IsNullOrEmpty(claim.claimInfo.Pay_To_Address_Grp.Trim())
                                                || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_City_Grp.Trim())
                                                || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_State_Grp.Trim())
                                                || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_Zip_Grp.Trim()))
                                            {

                                                if (string.IsNullOrEmpty(claim.claimInfo.Pay_To_Address_Grp.Trim())
                                                        || string.IsNullOrEmpty(claim.claimInfo.Pay_To_City_Grp.Trim())
                                                        || string.IsNullOrEmpty(claim.claimInfo.Pay_To_State_Grp.Trim()))
                                                {
                                                    errorList.Add("2010AB : Pay to Provider Group Address is incomplete.");
                                                }
                                                else
                                                {
                                                    switch (FederalTaxIDType)
                                                    {
                                                        case "EIN":
                                                            strBatchStringR += "NM1*87*2~";
                                                            strBatchString += "NM1*87*2~";
                                                            segmentCount++;
                                                            break;
                                                        case "SSN":
                                                            strBatchStringR += "NM1*87*1~";
                                                            strBatchString += "NM1*87*1~";
                                                            segmentCount++;
                                                            break;
                                                    }

                                                    strBatchStringR += "PAY-TO PROVIDER ADDRESS~";
                                                    strBatchStringR += "N3*";
                                                    strBatchStringR += claim.claimInfo.Pay_To_Address_Grp + "~";

                                                    strBatchString += "N3*";
                                                    strBatchString += claim.claimInfo.Pay_To_Address_Grp + "~";
                                                    segmentCount++;

                                                    strBatchStringR += "PAY - TO PROVIDER CITY~";
                                                    strBatchStringR += "N4*";
                                                    strBatchStringR += claim.claimInfo.Pay_To_City_Grp.Trim() + "*";
                                                    strBatchStringR += claim.claimInfo.Pay_To_State_Grp + "*";

                                                    strBatchString += "N4*";
                                                    strBatchString += claim.claimInfo.Pay_To_City_Grp.Trim() + "*";
                                                    strBatchString += claim.claimInfo.Pay_To_State_Grp + "*";
                                                    if (string.IsNullOrEmpty(claim.claimInfo.Pay_To_Zip_Grp.Trim()))
                                                    {
                                                        strBatchStringR += "     ";
                                                        strBatchString += "     ";
                                                    }
                                                    else
                                                    {
                                                        strBatchStringR += claim.claimInfo.Pay_To_Zip_Grp.Trim() + "~";
                                                        strBatchString += claim.claimInfo.Pay_To_Zip_Grp.Trim() + "~";
                                                    }
                                                    segmentCount++;

                                                }
                                            }
                                        }


                                        break;
                                    case "INDIVIDUAL": // Individual  

                                        if (!string.IsNullOrEmpty(claim.claimInfo.Bl_Address?.Trim())
                                            && !string.IsNullOrEmpty(claim.claimInfo.Pay_To_Address?.Trim())
                                            && !claim.claimInfo.Pay_To_Address.Trim().Equals(claim.claimInfo.Bl_Address.Trim()))
                                        {
                                            if (!string.IsNullOrEmpty(claim.claimInfo.Pay_To_Address.Trim())
                                                || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_City.Trim())
                                                || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_State.Trim())
                                                || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_Zip.Trim()))
                                            {

                                                if (string.IsNullOrEmpty(claim.claimInfo.Pay_To_Address.Trim())
                                                        || string.IsNullOrEmpty(claim.claimInfo.Pay_To_City.Trim())
                                                        || string.IsNullOrEmpty(claim.claimInfo.Pay_To_State.Trim()))
                                                {
                                                    errorList.Add("2010AB : Pay to Provider Individual Address is incomplete");
                                                }
                                                else
                                                {
                                                    switch (FederalTaxIDType)
                                                    {
                                                        case "EIN":
                                                            strBatchStringR += "NM1*87*2~";
                                                            strBatchString += "NM1*87*2~";
                                                            segmentCount++;
                                                            break;
                                                        case "SSN":
                                                            strBatchStringR += "NM1*87*1~";
                                                            strBatchString += "NM1*87*1~";
                                                            segmentCount++;
                                                            break;
                                                    }

                                                    strBatchStringR += "PAY-TO PROVIDER ADDRESS~";
                                                    strBatchStringR += "N3*";
                                                    strBatchStringR += claim.claimInfo.Pay_To_Address + "~";

                                                    strBatchString += "N3*";
                                                    strBatchString += claim.claimInfo.Pay_To_Address + "~";
                                                    segmentCount++;

                                                    strBatchStringR += "PAY - TO PROVIDER CITY~";
                                                    strBatchStringR += "N4*";
                                                    strBatchStringR += claim.claimInfo.Pay_To_City.Trim() + "*";
                                                    strBatchStringR += claim.claimInfo.Pay_To_State + "*";

                                                    strBatchString += "N4*";
                                                    strBatchString += claim.claimInfo.Pay_To_City.Trim() + "*";
                                                    strBatchString += claim.claimInfo.Pay_To_State + "*";
                                                    if (string.IsNullOrEmpty(claim.claimInfo.Pay_To_Zip.Trim()))
                                                    {
                                                        strBatchStringR += "     ";
                                                        strBatchString += "     ";
                                                    }
                                                    else
                                                    {
                                                        strBatchStringR += claim.claimInfo.Pay_To_Zip.Trim() + "~";
                                                        strBatchString += claim.claimInfo.Pay_To_Zip.Trim() + "~";
                                                    }
                                                    segmentCount++;

                                                }
                                            }
                                        }



                                        break;
                                }

                                #endregion


                                int P = HL;
                                HL = HL + 1;
                                int CHILD = 0;

                                string SBR02 = "18";


                                //---Extract Primar Secondary and Other Insurance Information before processing-----------
                                spGetBatchClaimsInsurancesInfo_Result primaryIns = null;
                                spGetBatchClaimsInsurancesInfo_Result SecondaryIns = null;
                                spGetBatchClaimsInsurancesInfo_Result otherIns = null;

                                if (claim.claimInsurance != null && claim.claimInsurance.Count > 0)
                                {
                                    foreach (var ins in claim.claimInsurance)
                                    {
                                        switch (ins.Insurace_Type.ToUpper().Trim())
                                        {
                                            case "P":
                                                primaryIns = ins;
                                                break;
                                            case "S":
                                                SecondaryIns = ins;
                                                break;
                                            case "O":
                                                otherIns = ins;
                                                break;
                                        }
                                    }
                                }

                                //--End

                                if (claim.claimInsurance == null || claim.claimInsurance.Count == 0)
                                {
                                    errorList.Add("Patient Insurance Information is missing.");
                                }
                                else if (primaryIns == null)
                                {
                                    errorList.Add("Patient Primary Insurance Information is missing.");
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(primaryIns.GRelationship)
                               && primaryIns.GRelationship.Trim().ToUpper().Equals("S"))
                                    {
                                        primaryIns.Glname = claim.claimInfo.Lname;
                                        primaryIns.Gfname = claim.claimInfo.Fname;
                                        primaryIns.Gmi = claim.claimInfo.Mname;
                                        primaryIns.Gaddress = claim.claimInfo.Address;
                                        primaryIns.Gcity = claim.claimInfo.City;
                                        primaryIns.Gdob = claim.claimInfo.Dob;
                                        primaryIns.Ggender = claim.claimInfo.Gender.ToString();
                                        primaryIns.Gstate = claim.claimInfo.State;
                                        primaryIns.Gzip = claim.claimInfo.Zip;
                                    }


                                    if (!primaryIns.GRelationship.Trim().ToUpper().Equals("S") && primaryIns.Guarantor_Id == null)
                                    {
                                        errorList.Add("Subscriber information is missing.");

                                    }
                                    if (primaryIns.Inspayer_Id == null)
                                    {
                                        errorList.Add("Payer's information is missing.");
                                    }

                                    if (
                                        !primaryIns.GRelationship.Trim().ToUpper().Equals("S")
                                        && !string.IsNullOrEmpty(primaryIns.GRelationship))
                                    {
                                        SBR02 = "";
                                        CHILD = 1;
                                    }


                                    #region LOOP 2000B
                                    strBatchStringR += "Loop ID -2000B - Subscriber Hierarchical Level~";

                                    #region HL: SUBSCRIBER HIERARCHICAL LEVEL
                                    strBatchStringR += "Subscriber Hierarchical Level~";

                                    strBatchStringR += "HL*";
                                    strBatchStringR += HL + "*" + P + "*";
                                    strBatchStringR += "22*" + CHILD + "~";

                                    strBatchString += "HL*";
                                    strBatchString += HL + "*" + P + "*";
                                    strBatchString += "22*" + CHILD + "~";
                                    segmentCount++;
                                    #endregion

                                    #region SBR: SUBSCRIBER INFORMATION
                                    strBatchStringR += "Subscriber Information~";
                                    strBatchStringR += "SBR*";

                                    strBatchString += "SBR*";
                                    if (SecondaryIns != null)
                                    {
                                        strBatchStringR += "S";
                                        strBatchString += "S";
                                    }
                                    else if (primaryIns != null)
                                    {
                                        strBatchStringR += "P";
                                        strBatchString += "P";
                                    }
                                    else if (otherIns != null)
                                    {
                                        strBatchStringR += "T";
                                        strBatchString += "T";
                                    }

                                    strBatchStringR += "*";
                                    strBatchString += "*";
                                    string groupNo = "";
                                    string planName = "";
                                    string payerTypeCode = "";

                                    if (SecondaryIns != null)
                                    {
                                        string SBR02_secondary = "18";

                                        if (!string.IsNullOrEmpty(SecondaryIns.GRelationship))
                                        {
                                            switch (SecondaryIns.GRelationship.ToUpper())
                                            {
                                                case "C":// Child
                                                    SBR02_secondary = "19";
                                                    break;
                                                case "P"://SPOUSE
                                                    SBR02_secondary = "01";
                                                    break;
                                                case "S"://Self
                                                    SBR02_secondary = "18";
                                                    break;
                                                case "O": // Other
                                                    SBR02_secondary = "G8";
                                                    break;
                                            }
                                        }

                                        //strBatchString += "SBR*S*";
                                        string PlanNameSec = "", InsPayerTypeCodeSec = "", payerTypeCodeSec = "";

                                        if (!string.IsNullOrEmpty(SecondaryIns.Insgroup_Name) && SecondaryIns.Insgroup_Name.Contains("MEDICARE"))
                                        {
                                            if (!string.IsNullOrEmpty(SecondaryIns.plan_name) && SecondaryIns.plan_name.ToUpper().Contains("MEDICARE"))
                                            {
                                                PlanNameSec = SecondaryIns.plan_name;
                                            }
                                            else
                                            {
                                                PlanNameSec = "MEDICARE";
                                            }
                                         // Changing for SBR09 Error by TriZetto
                                        payerTypeCodeSec = SecondaryIns.insurance_type_code;
                                        //payerTypeCodeSec = "47"; //5010 required in case of medicare is secondary or ter.
                                        /*                        
                                         12	Medicare Secondary Working Aged Beneficiary or Spouse with Employer Group Health Plan
                                         13	Medicare Secondary End Stage Renal Disease
                                         14	Medicare Secondary , No Fault Insurance including Auto is Primary
                                         15	Medicare Secondary Worker’s Compensation
                                         16	Medicare Secondary Public Health Service (PHS) or other Federal Agency
                                         16	Medicare Secondary Public Health Service
                                         41	Medicare Secondary Black Lung
                                         42	Medicare Secondary Veteran’s Administration
                                         43	Medicare Secondary Veteran’s Administration
                                         47	Medicare Secondary, Other Liability Insurance is Primary
                                         */

                                    }
                                        else
                                        {
                                            PlanNameSec = SecondaryIns.plan_name;
                                            payerTypeCodeSec = SecondaryIns.insurance_type_code;
                                        }


                                        strBatchStringR += SBR02_secondary + "*" + SecondaryIns.Group_Number + "*" + PlanNameSec + "*" + InsPayerTypeCodeSec + "****" + payerTypeCodeSec + "~";
                                        strBatchString += SBR02_secondary + "*" + SecondaryIns.Group_Number + "*" + PlanNameSec + "*" + InsPayerTypeCodeSec + "****" + payerTypeCodeSec + "~";
                                        segmentCount++;
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(primaryIns.Group_Number))
                                        {
                                            groupNo = primaryIns.Group_Number;
                                        }
                                        else
                                        {
                                            groupNo = "";
                                        }


                                        if (!string.IsNullOrEmpty(primaryIns.Insgroup_Name) && primaryIns.Insgroup_Name.Equals("MEDICARE"))
                                        {
                                            if (!string.IsNullOrEmpty(primaryIns.plan_name) && primaryIns.plan_name.ToUpper().Contains("MEDICARE"))
                                            {
                                                planName = primaryIns.plan_name;
                                            }
                                            else
                                            {
                                                planName = "MEDICARE";
                                            }
                                        }
                                        else
                                        {
                                            planName = primaryIns.plan_name;
                                        }

                                        // MISSING [To Do]
                                        //payerTypeCode = primaryIns.getPayertype_code()
                                        payerTypeCode = primaryIns.insurance_type_code;


                                        //---------***********************************-------------
                                        strBatchStringR += SBR02 + "*" + groupNo + "*" + planName + "*****" + payerTypeCode + "~";
                                        strBatchString += SBR02 + "*" + groupNo + "*" + planName + "*****" + payerTypeCode + "~";
                                        segmentCount++;
                                    }
                                    #endregion

                                    #endregion

                                    #region LOOP 2000BA (SUBSCRIBER Information)
                                    strBatchStringR += "Loop ID -2010BA - Subscriber Name~";
                                    strBatchStringR += "Subscriber Name~";
                                    strBatchStringR += "NM1*IL*1*";

                                    strBatchString += "NM1*IL*1*";
                                    if ((string.IsNullOrEmpty(primaryIns.Glname)
                                    || string.IsNullOrEmpty(primaryIns.Gfname))
                                    && string.IsNullOrEmpty(primaryIns.GRelationship)
                                    && !primaryIns.GRelationship.Trim().ToUpper().Equals("S"))
                                    {
                                        errorList.Add("Subscriber Last/First Name missing.");
                                    }
                                    if (SecondaryIns.GRelationship.Trim().ToUpper().Equals("S"))
                                    { SBR02 = "18"; }
                                    //Entering Subscriber Information if Relationship is SELF-----
                                    if (SBR02.Equals("18") && SecondaryIns.GRelationship.Trim().ToUpper().Equals("S"))
                                    {
                                        if (!isAlphaNumeric(claim.claimInfo.Lname)
                                            || !isAlphaNumeric(claim.claimInfo.Fname)
                                            )
                                        {
                                            errorList.Add("Subscriber Name must be Alpha Numeric.");
                                            errorList.Add("Subscriber Name must be Alpha Numeric.");
                                        }
                                        else
                                        {
                                            string policy_number;
                                            if (SecondaryIns != null)
                                            {
                                                policy_number = SecondaryIns.Policy_Number.ToUpper();
                                            }
                                            else
                                                policy_number = primaryIns.Policy_Number.ToUpper();
                                            strBatchStringR += claim.claimInfo.Lname + "*"
                                                    + claim.claimInfo.Fname + "*"
                                                    + claim.claimInfo.Mname + "***MI*"
                                                    + policy_number + "~";

                                            strBatchString += claim.claimInfo.Lname + "*"
                                                    + claim.claimInfo.Fname + "*"
                                                    + claim.claimInfo.Mname + "***MI*"
                                                    + policy_number + "~";
                                            segmentCount++;

                                        }

                                        if (string.IsNullOrEmpty(claim.claimInfo.Address)
                                            || string.IsNullOrEmpty(claim.claimInfo.City)
                                             || string.IsNullOrEmpty(claim.claimInfo.State)
                                             || string.IsNullOrEmpty(claim.claimInfo.Zip))
                                        {
                                            errorList.Add("Patient Address is incomplete.");
                                        }
                                        else
                                        {
                                            strBatchStringR += "Subscriber Address~";
                                            strBatchStringR += "N3*" + claim.claimInfo.Address + "~";
                                            strBatchString += "N3*" + claim.claimInfo.Address + "~";
                                            segmentCount++;

                                            strBatchStringR += "Subscriber City, State, ZIP~";
                                            strBatchStringR += "N4*" + claim.claimInfo.City + "*" + claim.claimInfo.State + "*";
                                            strBatchStringR += (!string.IsNullOrEmpty(claim.claimInfo.Zip) ? claim.claimInfo.Zip : "     ") + "~";

                                            strBatchString += "N4*" + claim.claimInfo.City + "*"
                                                    + claim.claimInfo.State + "*";
                                            strBatchString += (!string.IsNullOrEmpty(claim.claimInfo.Zip) ? claim.claimInfo.Zip : "     ") + "~";
                                            segmentCount++;
                                        }

                                        strBatchStringR += "Subscriber Demographic Information~";
                                        strBatchString += "DMG*D8*";
                                        if (string.IsNullOrEmpty(claim.claimInfo.Dob))
                                        {
                                            errorList.Add("Patient DOB is missing.");
                                        }
                                        else
                                        {
                                            strBatchStringR += !string.IsNullOrEmpty(claim.claimInfo.Dob) ? claim.claimInfo.Dob.Split('/')[0] + claim.claimInfo.Dob.Split('/')[1] + claim.claimInfo.Dob.Split('/')[2] : "";
                                            strBatchStringR += "*";
                                            strBatchString += !string.IsNullOrEmpty(claim.claimInfo.Dob) ? claim.claimInfo.Dob.Split('/')[0] + claim.claimInfo.Dob.Split('/')[1] + claim.claimInfo.Dob.Split('/')[2] : "";
                                            strBatchString += "*";
                                        }
                                        if (string.IsNullOrEmpty(claim.claimInfo.Gender.ToString()))
                                        {
                                            errorList.Add("Patient Gender is missing.");
                                        }
                                        else
                                        {
                                            strBatchStringR += claim.claimInfo.Gender.ToString();
                                            strBatchString += claim.claimInfo.Gender.ToString();

                                        }
                                        strBatchStringR += "~";
                                        strBatchString += "~";
                                        segmentCount++;
                                    } //--END
                                    else //---Entering Subscriber Information In case of other than SELF---------
                                    {
                                        string policy_number;
                                        if (SecondaryIns != null)
                                        {
                                            policy_number = SecondaryIns.Policy_Number.ToUpper();
                                        }
                                        else
                                            policy_number = SecondaryIns.Policy_Number.ToUpper();

                                        strBatchStringR += SecondaryIns.Glname + "*"
                                                + SecondaryIns.Gfname + "*"
                                                + SecondaryIns.Gmi + "***MI*"
                                                + policy_number + "~";

                                        strBatchString += SecondaryIns.Glname + "*"
                                                + SecondaryIns.Gfname + "*"
                                                + SecondaryIns.Gmi + "***MI*"
                                                + policy_number + "~";
                                        segmentCount++;

                                        if (string.IsNullOrEmpty(SecondaryIns.Gaddress)
                                           || string.IsNullOrEmpty(SecondaryIns.Gcity)
                                            || string.IsNullOrEmpty(SecondaryIns.Gstate)
                                            || string.IsNullOrEmpty(SecondaryIns.Gzip))
                                        {
                                            errorList.Add($"Secondary's Subscriber Address is incomplete {claimId}.");
                                        }
                                        else
                                        {
                                            strBatchStringR += "Subscriber Address~";
                                            strBatchStringR += "N3*" + SecondaryIns.Gaddress + "~";
                                            segmentCount++;

                                            strBatchString += "N3*" + SecondaryIns.Gaddress + "~";
                                            segmentCount++;

                                            strBatchStringR += "Subscriber City, State, ZIP~";
                                            strBatchStringR += "N4*" + SecondaryIns.Gcity + "*"
                                                    + SecondaryIns.Gstate + "*";
                                            strBatchStringR += (string.IsNullOrEmpty(SecondaryIns.Gzip) ? "     " : SecondaryIns.Gzip) + "~";

                                            strBatchString += "N4*" + SecondaryIns.Gcity + "*"
                                                    + SecondaryIns.Gstate + "*";
                                            strBatchString += (string.IsNullOrEmpty(SecondaryIns.Gzip) ? "     " : SecondaryIns.Gzip) + "~";
                                            segmentCount++;
                                        }

                                        strBatchStringR += "Subscriber Demographic Information~";
                                        strBatchStringR += "DMG*D8*";

                                        strBatchString += "DMG*D8*";
                                        if (string.IsNullOrEmpty(SecondaryIns.Gdob))
                                        {
                                            errorList.Add($"Secondary's Subscriber DOB is missing {claimId}.");
                                        }
                                        else
                                        {
                                            strBatchStringR += string.IsNullOrEmpty(SecondaryIns.Gdob) ? "" : SecondaryIns.Gdob.Split('/')[0] + SecondaryIns.Gdob.Split('/')[1] + SecondaryIns.Gdob.Split('/')[2];
                                            strBatchStringR += "*";
                                            strBatchString += string.IsNullOrEmpty(SecondaryIns.Gdob) ? "" : SecondaryIns.Gdob.Split('/')[0] + SecondaryIns.Gdob.Split('/')[1] + SecondaryIns.Gdob.Split('/')[2];
                                            strBatchString += "*";
                                        }

                                        if (string.IsNullOrEmpty(SecondaryIns.Ggender))
                                        {
                                            errorList.Add($"Secondary's Subscriber Gender is missing {claimId}.");
                                        }
                                        else
                                        {
                                            strBatchStringR += SecondaryIns.Ggender;
                                            strBatchString += SecondaryIns.Ggender;

                                        }
                                        strBatchStringR += "~";
                                        strBatchString += "~";
                                        segmentCount++;
                                    }

                                    #endregion

                                    #region LOOP 2010BB (PAYER INFORMATION)
                                    strBatchStringR += "Loop ID - 2010BB - Payer Name~";
                                    strBatchStringR += "Payer Name~";

                                    if (string.IsNullOrEmpty(primaryIns.plan_name))
                                    {
                                        errorList.Add("Payer name missing.");

                                    }
                                    if (SecondaryIns != null)
                                    {
                                        string SecInsPayerName = "";
                                        if (string.IsNullOrEmpty(SecondaryIns.plan_name))
                                        {
                                            errorList.Add("Secondary's payer name missing.");
                                        }
                                        else
                                        {
                                            if (SecondaryIns.Insgroup_Name.Trim().Contains("MEDICARE"))
                                            {
                                                SecInsPayerName = "MEDICARE";
                                            }
                                            else
                                            {
                                                SecInsPayerName = SecondaryIns.plan_name;
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(SecondaryIns.Payer_Number))
                                        {
                                            string secPayerNumber = primaryIns.Payer_Number.Equals(SecondaryIns.Payer_Number) ? SecondaryIns.Payer_Number + "A" : SecondaryIns.Payer_Number;
                                            strBatchStringR += "NM1*PR*2*" + SecInsPayerName + "*****PI*" + secPayerNumber + "~";
                                            strBatchString += "NM1*PR*2*" + SecInsPayerName + "*****PI*" + secPayerNumber + "~";
                                            segmentCount++;
                                        }
                                        else
                                        {
                                            errorList.Add("Secondary's insurance payer id is compulsory in case of Gateway EDI Clearing house.");
                                        }
                                    }
                                    else
                                    {
                                        string paperPayerName = "";
                                        if (!string.IsNullOrEmpty(primaryIns.plan_name) && primaryIns.plan_name.Trim().ToUpper().Equals("MEDICARE"))
                                        {
                                            paperPayerName = "MEDICARE";
                                        }
                                        else
                                        {
                                            paperPayerName = primaryIns.plan_name;
                                        }

                                        paperPayerID = primaryIns.Payer_Number;
                                        if (!string.IsNullOrEmpty(paperPayerID))
                                        {
                                            strBatchStringR += "NM1*PR*2*" + paperPayerName + "*****PI*" + paperPayerID + "~";
                                            strBatchString += "NM1*PR*2*" + paperPayerName + "*****PI*" + paperPayerID + "~";
                                            segmentCount++;
                                        }
                                        else
                                        {
                                            errorList.Add("Payer id is compulsory in case of Gateway EDI Clearing house.");
                                        }
                                    }

                                    if (SecondaryIns != null)
                                    {
                                        strBatchStringR += "Payer Address~";
                                        strBatchStringR += "N3*" + SecondaryIns.Ins_Address + "~";
                                        strBatchString += "N3*" + SecondaryIns.Ins_Address + "~";
                                        segmentCount++;
                                        strBatchStringR += "Payer City, State, ZIP~";
                                        strBatchStringR += "N4*" + SecondaryIns.Ins_City + "*" + SecondaryIns.Ins_State + "*" + SecondaryIns.Ins_Zip.Trim() + "~";
                                        strBatchString += "N4*" + SecondaryIns.Ins_City + "*" + SecondaryIns.Ins_State + "*" + SecondaryIns.Ins_Zip.Trim() + "~";
                                        segmentCount++;
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(primaryIns.Insgroup_Name) && primaryIns.plan_name.Trim().ToUpper().Equals("WORK COMP"))
                                        {
                                            if (string.IsNullOrEmpty(primaryIns.Sub_Empaddress)
                                                    || string.IsNullOrEmpty(primaryIns.Sub_Emp_City)
                                                    || string.IsNullOrEmpty(primaryIns.Sub_Emp_State)
                                                    || string.IsNullOrEmpty(primaryIns.Sub_Emp_Zip))
                                            {
                                                errorList.Add("Payer is Worker Company, so its subscriber employer’s address is necessary.");

                                            }
                                            strBatchStringR += "Payer Address~";
                                            strBatchStringR += "N3*" + primaryIns.Sub_Empaddress + "~";
                                            strBatchString += "N3*" + primaryIns.Sub_Empaddress + "~";
                                            segmentCount++;

                                            strBatchStringR += "Payer City, State, ZIP~";
                                            strBatchStringR += "N4*" + primaryIns.Sub_Emp_City + "*"
                                                    + primaryIns.Sub_Emp_State + "*";
                                            strBatchString += "N4*" + primaryIns.Sub_Emp_City + "*"
                                                    + primaryIns.Sub_Emp_State + "*";
                                            if (!string.IsNullOrEmpty(primaryIns.Sub_Emp_Zip))
                                            {
                                                strBatchStringR += primaryIns.Sub_Emp_Zip;
                                                strBatchString += primaryIns.Sub_Emp_Zip;

                                            }
                                            else
                                            {
                                                strBatchStringR += "     ";
                                                strBatchString += "     ";
                                            }
                                            strBatchStringR += "~";
                                            strBatchString += "~";
                                            segmentCount++;
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(primaryIns.Ins_Address)
                                                    || string.IsNullOrEmpty(primaryIns.Ins_City)
                                                    || string.IsNullOrEmpty(primaryIns.Ins_State)
                                                    || string.IsNullOrEmpty(primaryIns.Ins_Zip))
                                            {
                                                errorList.Add("Payer address incomplete.");
                                            }
                                            strBatchStringR += "Payer Address~";
                                            strBatchStringR += "N3*" + primaryIns.Ins_Address;
                                            strBatchStringR += "~";
                                            strBatchString += "N3*" + primaryIns.Ins_Address;
                                            strBatchString += "~";
                                            segmentCount++;
                                            strBatchStringR += "Payer City, State, ZIP~";
                                            strBatchStringR += "N4*" + primaryIns.Ins_City + "*" + primaryIns.Ins_State + "*";
                                            strBatchStringR += (string.IsNullOrEmpty(primaryIns.Ins_Zip)) ? "     " : primaryIns.Ins_Zip.Trim();
                                            strBatchStringR += "~";
                                            strBatchString += "N4*" + primaryIns.Ins_City + "*" + primaryIns.Ins_State + "*";
                                            strBatchString += (string.IsNullOrEmpty(primaryIns.Ins_Zip)) ? "     " : primaryIns.Ins_Zip.Trim();
                                            strBatchString += "~";
                                            segmentCount++;
                                        }
                                    }



                                    #endregion

                                    #region LOOP 2010C , 2010CA
                                    if (!SecondaryIns.GRelationship.Trim().ToUpper().Equals("S"))
                                    {
                                        if (!string.IsNullOrEmpty(primaryIns.GRelationship)
                                       && !primaryIns.GRelationship.ToUpper().Trim().Equals("S"))
                                        {

                                            #region LOOP 2000C
                                            strBatchStringR += "Loop ID - 2000C - Patient Hierarchical Level~";

                                            #region HL : (PATIENT HIERARCHICAL LEVEL)
                                            strBatchStringR += "Patient Hierarchical Level~";
                                            int PHL = HL;
                                            HL++;
                                            strBatchStringR += "HL*" + HL + "*" + PHL + "*23*0~";
                                            strBatchString += "HL*" + HL + "*" + PHL + "*23*0~";
                                            segmentCount++;
                                            #endregion


                                            #region PAT : (PATIENT RELATIONAL INFORMATION)
                                            strBatchStringR += "PATIENT RELATIONAL INFORMATION~";
                                            strBatchStringR += "PAT*";
                                            strBatchString += "PAT*";
                                            String temp = "";
                                            if (string.IsNullOrEmpty(primaryIns.GRelationship))
                                            {
                                                errorList.Add("Subscriber relationship is missing.");
                                            }
                                            else
                                            {
                                                if (primaryIns.GRelationship.Trim().ToUpper().Equals("S"))
                                                {
                                                    temp = "18";
                                                }
                                                else if (primaryIns.GRelationship.Trim().ToUpper().Equals("P"))
                                                {
                                                    temp = "01";
                                                }
                                                else if (primaryIns.GRelationship.Trim().ToUpper().Equals("C"))
                                                {
                                                    temp = "19";
                                                }
                                                else if (primaryIns.GRelationship.Trim().ToUpper().Equals("O"))
                                                {
                                                    temp = "G8";
                                                }
                                            }

                                            strBatchStringR += temp + "****D8***~";
                                            strBatchString += temp + "****D8***~";
                                            segmentCount++;
                                            #endregion

                                            #endregion


                                            #region LOOP 2010CA
                                            strBatchStringR += "Loop ID -2010CA - Patient Name~";

                                            #region PATIENT NAME INFORMATION
                                            strBatchStringR += "Patient Name~";
                                            strBatchStringR += "NM1*QC*1*";
                                            strBatchString += "NM1*QC*1*";

                                            //----ENTERING PATIENT INFORMATION NOW------------
                                            strBatchStringR += claim.claimInfo.Lname + "*";
                                            strBatchStringR += claim.claimInfo.Fname + "*";
                                            strBatchStringR += claim.claimInfo.Mname + "***MI*";

                                            strBatchString += claim.claimInfo.Lname + "*";
                                            strBatchString += claim.claimInfo.Fname + "*";
                                            strBatchString += claim.claimInfo.Mname + "***MI*";
                                            if (string.IsNullOrEmpty(primaryIns.Policy_Number))
                                            {
                                                errorList.Add("Subscriber policy number  missing.");
                                            }
                                            strBatchStringR += primaryIns.Policy_Number.ToUpper() + "~";
                                            strBatchString += primaryIns.Policy_Number.ToUpper() + "~";
                                            segmentCount++;

                                            strBatchStringR += "Patient Address~";
                                            strBatchStringR += "N3*" + claim.claimInfo.Address.Trim() + "~";
                                            strBatchString += "N3*" + claim.claimInfo.Address.Trim() + "~";
                                            segmentCount++;

                                            strBatchStringR += "Patient City, State, ZIP~";
                                            strBatchStringR += "N4*" + claim.claimInfo.City.Trim() + "*" + claim.claimInfo.State.Trim() + "*"
                                                    + claim.claimInfo.Zip.Trim() + "~";
                                            strBatchString += "N4*" + claim.claimInfo.City.Trim() + "*" + claim.claimInfo.State.Trim() + "*"
                                                    + claim.claimInfo.Zip.Trim() + "~";
                                            segmentCount++;

                                            if (string.IsNullOrEmpty(claim.claimInfo.Gender.ToString()))
                                            {
                                                errorList.Add("Patient gender missing.");
                                            }
                                            strBatchStringR += "Patient Demographic Information~";
                                            strBatchStringR += "DMG*D8*" + claim.claimInfo.Dob.Split('/')[0] + claim.claimInfo.Dob.Split('/')[1] + claim.claimInfo.Dob.Split('/')[2] + "*" + claim.claimInfo.Gender.ToString() + "~";
                                            strBatchString += "DMG*D8*" + claim.claimInfo.Dob.Split('/')[0] + claim.claimInfo.Dob.Split('/')[1] + claim.claimInfo.Dob.Split('/')[2] + "*" + claim.claimInfo.Gender.ToString() + "~";
                                            segmentCount++;
                                            #endregion
                                            #endregion
                                        }
                                    }
                                    #endregion

                                    HL++;

                                    #region LOOP 2300
                                    strBatchStringR += "Loop ID - 2300 - Claim Information~";
                                    strBatchStringR += "Claim Information~";
                                    strBatchStringR += "CLM*" + claim.claim_No + "*";

                                    strBatchString += "CLM*" + claim.claim_No + "*";

                                    decimal total_amount = 0;

                                    if (claim.claimInfo.Is_Resubmitted)
                                    {
                                        foreach (var proc in claim.claimProcedures)
                                        {
                                            if (proc.Is_Resubmitted)
                                            {
                                                total_amount = total_amount + (decimal)proc.Total_Charges;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        total_amount = claim.claimInfo.Claim_Total;
                                    }


                                    string ClaimFrequencyCode = (bool)claim.claimInfo.Is_Corrected ? claim.claimInfo.RSCode.ToString() : "1";
                                    string PatFirstVisitDatesegmentCount = "";

                                    strBatchStringR += string.Format("{0:0.00}", total_amount) + "***" + claim.claimInfo.Claim_Pos + ":B:" + ClaimFrequencyCode + "*Y*A*Y*Y*P"; // 5010
                                    strBatchString += string.Format("{0:0.00}", total_amount) + "***" + claim.claimInfo.Claim_Pos + ":B:" + ClaimFrequencyCode + "*Y*A*Y*Y*P"; // 5010


                                    #region Accident Info
                                    int isErrorInAccident = 0;

                                    if (!string.IsNullOrEmpty(claim.claimInfo.Accident_Type))
                                    {

                                        switch (claim.claimInfo.Accident_Type.ToUpper())
                                        {
                                            case "OA":
                                                strBatchStringR += "*OA";
                                                strBatchString += "*OA";
                                                break;
                                            case "AA":
                                                strBatchStringR += "*AA";
                                                strBatchString += "*AA";
                                                break;
                                            case "EM":
                                                strBatchStringR += "*EM";
                                                strBatchString += "*EM";
                                                break;
                                            default:
                                                isErrorInAccident = 1;
                                                break;
                                        }


                                        if (isErrorInAccident == 0)
                                        {
                                            if (!string.IsNullOrEmpty(claim.claimInfo.Accident_State))
                                            {
                                                strBatchStringR += ":::" + claim.claimInfo.Accident_State + "~";
                                                strBatchString += ":::" + claim.claimInfo.Accident_State + "~";
                                                segmentCount++;
                                            }
                                            else
                                            {
                                                if (claim.claimInfo.Accident_Type.ToUpper().Equals("OA")
                                                    || claim.claimInfo.Accident_Type.ToUpper().Equals("EM"))
                                                {
                                                    strBatchStringR += "~";
                                                    strBatchString += "~";
                                                    segmentCount++;
                                                }
                                                else
                                                {
                                                    isErrorInAccident = 2;
                                                }
                                            }

                                            if (isErrorInAccident == 0)
                                            {
                                                #region DATE  ACCIDENT
                                                strBatchStringR += "Claim Date";
                                                strBatchStringR += "DTP*439*D8*";
                                                strBatchString += "DTP*439*D8*";
                                                if (!string.IsNullOrEmpty(claim.claimInfo.Accident_Date) && !claim.claimInfo.Accident_Date.Equals("1900/01/01"))
                                                {
                                                    string[] splitedAccidentDate = claim.claimInfo.Accident_Date.Split('/');
                                                    if (splitedAccidentDate.Count() != 3)
                                                    {
                                                        isErrorInAccident = 3;
                                                    }
                                                    strBatchStringR += splitedAccidentDate[0] + splitedAccidentDate[1] + splitedAccidentDate[2] + "~";
                                                    strBatchString += splitedAccidentDate[0] + splitedAccidentDate[1] + splitedAccidentDate[2] + "~";
                                                    segmentCount++;
                                                }
                                                else
                                                {
                                                    isErrorInAccident = 4;
                                                }

                                                #endregion
                                            }
                                        }
                                    }
                                    else
                                    {
                                        strBatchStringR += "~";
                                        strBatchString += "~";
                                        segmentCount++;
                                    }
                                    #endregion

                                    #region DATE - INITIAL TREATMENT
                                    if (!string.IsNullOrEmpty(PatFirstVisitDatesegmentCount))
                                    {
                                        strBatchStringR += "DATE - INITIAL TREATMENT";
                                        strBatchStringR += PatFirstVisitDatesegmentCount;
                                        strBatchString += PatFirstVisitDatesegmentCount;
                                        segmentCount++;
                                    }

                                    #endregion

                                    #region DATE -  Last X-Ray Date

                                    if (!string.IsNullOrEmpty(claim.claimInfo.Last_Xray_Date) && !claim.claimInfo.Last_Xray_Date.Equals("1900/01/01"))
                                    {
                                        string[] spltdlastXrayDate = claim.claimInfo.Last_Xray_Date.Split('/');
                                        string LastXrayDate = spltdlastXrayDate[0] + spltdlastXrayDate[1] + spltdlastXrayDate[2];
                                        strBatchStringR += "DATE -  Last X-Ray Date~";
                                        strBatchStringR += "DTP*455*D8*" + LastXrayDate + "~";
                                        strBatchString += "DTP*455*D8*" + LastXrayDate + "~";
                                        segmentCount++;
                                    }

                                    #endregion

                                    #region DATE - ADMISSION (HOSPITALIZATION)


                                    if (!string.IsNullOrEmpty(claim.claimInfo.Hospital_From) && !claim.claimInfo.Hospital_From.Equals("1900/01/01"))
                                    {
                                        string[] spltdHospitalFromDate = claim.claimInfo.Hospital_From.Split('/');
                                        if (spltdHospitalFromDate.Count() != 3)
                                        {
                                            isErrorInAccident = 3;
                                        }

                                        string hospitalFromDate = spltdHospitalFromDate[0] + spltdHospitalFromDate[1] + spltdHospitalFromDate[2];
                                        strBatchStringR += "DATE - ADMISSION HOSPITALIZATION FromDate~";
                                        strBatchStringR += "DTP*435*D8*" + hospitalFromDate + "~";
                                        strBatchString += "DTP*435*D8*" + hospitalFromDate + "~";
                                        segmentCount++;
                                    }

                                    if (!string.IsNullOrEmpty(claim.claimInfo.Hospital_To) && !claim.claimInfo.Hospital_To.Equals("1900/01/01"))
                                    {
                                        string[] spltdHospitalTO = claim.claimInfo.Hospital_To.Split('/');
                                        if (spltdHospitalTO.Count() != 3)
                                        {
                                            isErrorInAccident = 3;
                                        }
                                        string hospitalTo = spltdHospitalTO[0] + spltdHospitalTO[1] + spltdHospitalTO[2];
                                        strBatchStringR += "DATE - ADMISSION HOSPITALIZATION ToDate~";
                                        strBatchStringR += "DTP*096*D8*" + hospitalTo + "~";
                                        strBatchString += "DTP*096*D8*" + hospitalTo + "~";
                                        segmentCount++;
                                    }

                                    #endregion


                                    if (isErrorInAccident >= 1)
                                    {
                                        if (isErrorInAccident == 1)
                                        {
                                            errorList.Add("Accident Type is missing.");
                                        }
                                        else if (isErrorInAccident == 2)
                                        {
                                            errorList.Add("State of accident is necessary.");
                                        }
                                        else if (isErrorInAccident == 3)
                                        {
                                            errorList.Add("Format of date of accident is not correct.");
                                        }
                                        else if (isErrorInAccident == 4)
                                        {
                                            errorList.Add("Date of accident is missing.");
                                        }
                                    }


                                    #region PRIOR AUTHORIZATION
                                    if (!string.IsNullOrEmpty(claim.claimInfo.Prior_Authorization))
                                    {
                                        strBatchStringR += "PRIOR AUTHORIZATION~";
                                        strBatchStringR += "REF*G1*" + claim.claimInfo.Prior_Authorization + "~";
                                        strBatchString += "REF*G1*" + claim.claimInfo.Prior_Authorization + "~";
                                        segmentCount++;
                                    }
                                    #endregion

                                    #region PAYER CLAIM CONTROL NUMBER
                                    if (!string.IsNullOrEmpty(claim.claimInfo.Claim_Number))
                                    {
                                        strBatchStringR += "PAYER CLAIM CONTROL NUMBER~";
                                        strBatchStringR += "REF*F8*" + claim.claimInfo.Claim_Number + "~";
                                        strBatchString += "REF*F8*" + claim.claimInfo.Claim_Number + "~";
                                        segmentCount++;
                                    }
                                    #endregion

                                    #region CLINICAL LABORATORY IMPROVEMENT AMENDMENT (CLIA) NUMBER
                                    if (!string.IsNullOrEmpty(claim.claimInfo.Clia_Number))
                                    {
                                        strBatchStringR += "CLINICAL LABORATORY IMPROVEMENT AMENDMENT (CLIA) NUMBER~";
                                        strBatchStringR += "REF*X4*" + claim.claimInfo.Clia_Number + "~";
                                        strBatchString += "REF*X4*" + claim.claimInfo.Clia_Number + "~";
                                        segmentCount++;
                                    }
                                    #endregion

                                    if (!string.IsNullOrEmpty(claim.claimInfo.Additional_Claim_Info))
                                    {
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Additional_Claim_Info) && claim.claimInfo.Additional_Claim_Info.StartsWith("CT") && claim.claimInfo.Additional_Claim_Info.Length > 2)
                                        {
                                            string newValue = claim.claimInfo.Additional_Claim_Info.Substring(2);
                                            if (!string.IsNullOrEmpty(newValue))
                                            {
                                                strBatchStringR += "National Clinical trial Number (NCT)~";
                                                strBatchStringR += "REF*P4*" + newValue + "~";
                                                strBatchString += "REF*P4*" + newValue + "~";
                                                segmentCount++;
                                            }
                                        }
                                        else
                                        {
                                            #region CLAIM NOTE (LUO)
                                            if (!string.IsNullOrEmpty(claim.claimInfo.Additional_Claim_Info))
                                            {
                                                strBatchStringR += "CLAIM NOTE (LUO)";
                                                strBatchStringR += "NTE*ADD*" + claim.claimInfo.Additional_Claim_Info + "~";
                                                strBatchString += "NTE*ADD*" + claim.claimInfo.Additional_Claim_Info + "~";
                                                segmentCount++;
                                            }
                                            #endregion
                                        }
                                    }
                                    #region New:REF - Referral_Number
                                    //var claimid = claim.claim_No;
                                    if (sPDataModels != null && sPDataModels.Any())
                                    {
                                        var referralNumber = sPDataModels[0].REFERRAL_NUMBER;

                                        if (!string.IsNullOrEmpty(referralNumber))
                                        {
                                            strBatchString += "REF*9F*";
                                            strBatchString += $"{referralNumber}~";
                                            segmentCount++;
                                        }
                                    }
                                    #endregion End!

                                    #region HEALTH CARE DIAGNOSIS CODE
                                    strBatchStringR += "HEALTH CARE DIAGNOSIS CODE";
                                    strBatchStringR += "HI*";

                                    strBatchString += "HI*";

                                    // ICD-10 Claim
                                    if ((bool)claim.claimInfo.Icd_10_Claim)
                                    {
                                        strBatchStringR += "ABK:";  // BK=ICD-9 ABK=ICD-10
                                        strBatchString += "ABK:";  // BK=ICD-9 ABK=ICD-10
                                    }
                                    else // ICD-9 Claim
                                    {
                                        strBatchStringR += "BK:";  // BK=ICD-9 ABK=ICD-10 
                                        strBatchString += "BK:";  // BK=ICD-9 ABK=ICD-10 
                                    }

                                    //Adding claim ICDS Diagnosis COdes
                                    int diagCount = 0;
                                    if (claim.claimDiagnosis != null)
                                    {
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code1))
                                        {
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code1);
                                            diagCount++;
                                        }

                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code2))
                                        {
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code2);
                                            diagCount++;
                                        }

                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code3))
                                        {
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code3);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code4))
                                        {
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code4);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code5))
                                        {
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code5);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code6))
                                        {
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code6);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code7))
                                        {
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code7);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code8))
                                        {
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code8);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code9))
                                        {
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code9);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code10))
                                        {
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code10);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code11))
                                        {
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code11);
                                            diagCount++;
                                        }
                                        if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code12))
                                        {
                                            strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code12);
                                            diagCount++;
                                        }
                                    }
                                    if (diagCount == 0)
                                    {
                                        if ((bool)claim.claimInfo.Icd_10_Claim)
                                        {
                                            errorList.Add("HI*ABK:ABF!Claims Diagnosis (ICD-10) are missing.");
                                        }
                                        else
                                        {
                                            errorList.Add("HI*BK:BF!Claims Diagnosis (ICD-9) are missing.");
                                        }


                                    }
                                    strBatchStringR += "~";
                                    strBatchString += "~";
                                    segmentCount++;


                                    #endregion

                                    #endregion

                                    #region LOOP 2310A (REFERRING PROVIDER)
                                    strBatchStringR += "Loop ID - 2310A - Referring Provider Name~";
                                    if (claim.claimInfo.Referring_Physician != null)
                                    {
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Ref_Npi))
                                        {

                                            if (!isAlphaNumeric(claim.claimInfo.Ref_Lname)
                                                    || !isAlphaNumeric(claim.claimInfo.Ref_Fname))
                                            {
                                                errorList.Add("Referring provider’s Name must be Alpha Numeric..");
                                            }
                                            else
                                            {
                                                strBatchStringR += "Referring Provider Name~";
                                                strBatchStringR += "NM1*DN*1*" + claim.claimInfo.Ref_Lname + "*"
                                                        + claim.claimInfo.Ref_Fname + "****XX*"
                                                        + claim.claimInfo.Ref_Npi + "~";

                                                strBatchString += "NM1*DN*1*" + claim.claimInfo.Ref_Lname + "*"
                                                        + claim.claimInfo.Ref_Fname + "****XX*"
                                                        + claim.claimInfo.Ref_Npi + "~";

                                                segmentCount++;
                                            }
                                        }
                                        else
                                        {
                                            errorList.Add("Referring provider’s NPI is missing.");
                                        }
                                    }
                                    #endregion

                                    #region LOOP 2310B (RENDERING PROVIDER)
                                    strBatchStringR += "Loop ID - 2310B - Rendering Provider Name~";
                                    if (claim.claimInfo.Attending_Physician != null)
                                    {
                                        #region RENDERING PROVIDER NAME
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Att_Npi))
                                        {

                                            if (!isAlphaNumeric(claim.claimInfo.Att_Lname)
                                                    && !isAlphaNumeric(claim.claimInfo.Att_Fname))
                                            {
                                                errorList.Add("Rendering provider’s Name must be Alpha Numeric.");
                                            }
                                            else
                                            {
                                                strBatchStringR += "Rendering Provider Name~";
                                                strBatchStringR += "NM1*82*1*" + claim.claimInfo.Att_Lname + "*"
                                                        + claim.claimInfo.Att_Fname + "****XX*"
                                                        + claim.claimInfo.Att_Npi + "~";

                                                strBatchString += "NM1*82*1*" + claim.claimInfo.Att_Lname + "*"
                                                        + claim.claimInfo.Att_Fname + "****XX*"
                                                        + claim.claimInfo.Att_Npi + "~";

                                                segmentCount++;
                                            }

                                        }
                                        else
                                        {
                                            errorList.Add("Rendering Provider NPI Missing.");

                                        }
                                        #endregion

                                        #region RENDERING PROVIDER SPECIALTY INFORMATION

                                        if (!string.IsNullOrEmpty(claim.claimInfo.Att_Taxonomy_Code))
                                        {
                                            strBatchStringR += "Rendering Provider Specialty Information~";
                                            strBatchStringR += "PRV*PE*PXC*" + claim.claimInfo.Att_Taxonomy_Code + "~";
                                            strBatchString += "PRV*PE*PXC*" + claim.claimInfo.Att_Taxonomy_Code + "~"; //5010 CODE CHAGED FROM ZZ TO PXC
                                            segmentCount++;
                                        }
                                        else
                                        {
                                            errorList.Add("Gateway edi require Rendering Provider Taxonomy Code.");
                                        }
                                        #endregion

                                        #region RENDERING PROVIDER SPECIALTY INFORMATION                                
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Att_State_License))
                                        {
                                            strBatchStringR += "Rendering Provider Specialty Information~";
                                            strBatchStringR += "REF*0B*" + claim.claimInfo.Att_State_License + "~";
                                            strBatchString += "REF*0B*" + claim.claimInfo.Att_State_License + "~";
                                            segmentCount++;
                                        }
                                        #endregion

                                    }
                                    else
                                    {
                                        errorList.Add("Rendering Provider Information missing..");
                                    }
                                    #endregion



                                    #region LOOP 2310C (SERVICE FACILITY LOCATION)
                                    strBatchStringR += "LOOP 2310C SERVICE FACILITY LOCATION~";

                                    if (claim.claimInfo.Facility_Code != 0)
                                    {

                                        if (!string.IsNullOrEmpty(claim.claimInfo.Facility_Npi))
                                        {
                                            strBatchStringR += "Service Facility Name~";
                                            strBatchStringR += "NM1*77*2*" + claim.claimInfo.Facility_Name + "*****XX*"
                                                    + claim.claimInfo.Facility_Npi + "~";

                                            strBatchString += "NM1*77*2*" + claim.claimInfo.Facility_Name + "*****XX*"
                                                    + claim.claimInfo.Facility_Npi + "~";
                                        }
                                        else
                                        {
                                            strBatchStringR += "Service Facility Name~";
                                            strBatchStringR += "NM1*77*2*" + claim.claimInfo.Facility_Name + "*****XX*~";
                                            strBatchString += "NM1*77*2*" + claim.claimInfo.Facility_Name + "*****XX*~";
                                        }
                                        segmentCount++;

                                        if (string.IsNullOrEmpty(claim.claimInfo.Facility_Address)
                                                || string.IsNullOrEmpty(claim.claimInfo.Facility_City)
                                                || string.IsNullOrEmpty(claim.claimInfo.Facility_State)
                                                || string.IsNullOrEmpty(claim.claimInfo.Facility_Zip))
                                        {
                                            errorList.Add("Facility's address incomplete.");
                                        }
                                        strBatchStringR += "Service Facility Address~";
                                        strBatchStringR += "N3*" + claim.claimInfo.Facility_Address + "~";
                                        strBatchString += "N3*" + claim.claimInfo.Facility_Address + "~";
                                        segmentCount++;
                                        strBatchStringR += "Service Facility City, State, ZIP~";
                                        strBatchStringR += "N4*" + claim.claimInfo.Facility_City + "*"
                                                + claim.claimInfo.Facility_State + "*";

                                        strBatchString += "N4*" + claim.claimInfo.Facility_City + "*"
                                                + claim.claimInfo.Facility_State + "*";
                                        if (string.IsNullOrEmpty(claim.claimInfo.Facility_Zip))
                                        {
                                            strBatchStringR += "     " + "~";
                                            strBatchString += "     " + "~";
                                        }
                                        else
                                        {
                                            strBatchStringR += claim.claimInfo.Facility_Zip + "~";
                                            strBatchString += claim.claimInfo.Facility_Zip + "~";
                                        }
                                        segmentCount++;
                                    }


                                    #endregion


                                    if (SecondaryIns != null)
                                    {
                                        #region LOOP 2320
                                        strBatchStringR += "LOOP 2320 OTHER SUBSCRIBER INFORMATION~";
                                        using (var ctx = new NPMDBEntities())
                                        {
                                            datamodel = ctx.Claim_Payments
                                            .Where(cp => cp.Claim_No == claim_id && cp.Payment_Source == "1" && ((cp.Deleted ?? false) == false))
                                            .Select(cp => new DataModel
                                            {
                                                Amount_Approved = cp.Amount_Approved,
                                                Amount_Paid = cp.Amount_Paid,
                                                Amount_Adjusted = cp.Amount_Adjusted,
                                                Reject_Type = cp.Reject_Type,
                                                Reject_Amount = cp.Reject_Amount,
                                                Paid_Proc_Code = cp.Paid_Proc_Code,
                                                Charged_Proc_Code = cp.Charged_Proc_Code,
                                                Insurance_Id = cp.Insurance_Id,
                                                ERA_CATEGORY_CODE = cp.ERA_CATEGORY_CODE,
                                                ERA_ADJUSTMENT_CODE = cp.ERA_ADJUSTMENT_CODE,
                                                ERA_Rejection_CATEGORY_CODE = cp.ERA_Rejection_CATEGORY_CODE,
                                                DOS_From = cp.DOS_From,
                                                Dos_To = cp.Dos_To,
                                                payment_source = cp.Payment_Source,
                                                Date_Filing = cp.Date_Filing,
                                                ICN = cp.ICN,
                                            }).ToList();
                                        }


                                        #region OTHER SUBSCRIBER INFORMATION

                                        string SBR02_secondary = "18";

                                        if (primaryIns != null)
                                        {
                                            if (!string.IsNullOrEmpty(primaryIns.GRelationship))
                                            {
                                                switch (primaryIns.GRelationship.ToUpper())
                                                {
                                                    case "C":// Child
                                                        SBR02_secondary = "19";
                                                        break;
                                                    case "P"://SPOUSE
                                                        SBR02_secondary = "01";
                                                        break;
                                                    case "S"://Self
                                                        SBR02_secondary = "18";
                                                        break;
                                                    case "O": // Other
                                                        SBR02_secondary = "G8";
                                                        break;
                                                }
                                            }
                                        }
                                        if (primaryIns != null)
                                        {
                                            strBatchStringR += "THER SUBSCRIBER INFORMATION";
                                            strBatchStringR += "SBR*S*";
                                            strBatchString += "SBR*P*";
                                        }

                                        string PlanNameSec = "", InsPayerTypeCodeSec = "", payerTypeCodeSec = "";

                                        if (!string.IsNullOrEmpty(primaryIns.Insgroup_Name) && primaryIns.Insgroup_Name.Contains("MEDICARE"))
                                        {
                                            if (!string.IsNullOrEmpty(primaryIns.plan_name) && primaryIns.plan_name.ToUpper().Contains("MEDICARE"))
                                            {
                                                PlanNameSec = primaryIns.plan_name;
                                            }
                                            else
                                            {
                                                PlanNameSec = "MEDICARE";
                                            }

                                        // Changing for SBR09 Error by TriZetto
                                        payerTypeCodeSec = primaryIns.insurance_type_code;
                                        // payerTypeCodeSec = "47"; //5010 required in case of medicare is secondary or ter.
                                            /*                        
                                             12	Medicare Secondary Working Aged Beneficiary or Spouse with Employer Group Health Plan
                                             13	Medicare Secondary End Stage Renal Disease
                                             14	Medicare Secondary , No Fault Insurance including Auto is Primary
                                             15	Medicare Secondary Worker’s Compensation
                                             16	Medicare Secondary Public Health Service (PHS) or other Federal Agency
                                             16	Medicare Secondary Public Health Service
                                             41	Medicare Secondary Black Lung
                                             42	Medicare Secondary Veteran’s Administration
                                             43	Medicare Secondary Veteran’s Administration
                                             47	Medicare Secondary, Other Liability Insurance is Primary
                                             */

                                        }

                                        //if (!string.IsNullOrEmpty(SecondaryIns.Insgroup_Name) && SecondaryIns.Insgroup_Name.Contains("MEDICARE"))
                                        //{
                                        //    if (!string.IsNullOrEmpty(SecondaryIns.plan_name) && SecondaryIns.plan_name.ToUpper().Contains("MEDICARE"))
                                        //    {
                                        //        PlanNameSec = SecondaryIns.plan_name;
                                        //    }
                                        //    else
                                        //    {
                                        //        PlanNameSec = "MEDICARE";
                                        //    }

                                        //    payerTypeCodeSec = "47"; //5010 required in case of medicare is secondary or ter.
                                        //    /*                        
                                        //     12	Medicare Secondary Working Aged Beneficiary or Spouse with Employer Group Health Plan
                                        //     13	Medicare Secondary End Stage Renal Disease
                                        //     14	Medicare Secondary , No Fault Insurance including Auto is Primary
                                        //     15	Medicare Secondary Worker’s Compensation
                                        //     16	Medicare Secondary Public Health Service (PHS) or other Federal Agency
                                        //     16	Medicare Secondary Public Health Service
                                        //     41	Medicare Secondary Black Lung
                                        //     42	Medicare Secondary Veteran’s Administration
                                        //     43	Medicare Secondary Veteran’s Administration
                                        //     47	Medicare Secondary, Other Liability Insurance is Primary
                                        //     */

                                        //}
                                        else
                                        {
                                            PlanNameSec = primaryIns.plan_name;
                                            payerTypeCodeSec = primaryIns.insurance_type_code;
                                        }


                                        strBatchStringR += SBR02_secondary + "*" + primaryIns.Group_Number + "*" + PlanNameSec + "*" + InsPayerTypeCodeSec + "****" + payerTypeCodeSec + "~";
                                        strBatchString += SBR02_secondary + "*" + primaryIns.Group_Number + "*" + PlanNameSec + "*" + InsPayerTypeCodeSec + "****" + payerTypeCodeSec + "~";
                                        segmentCount++;


                                        using (var ctx = new NPMDBEntities())
                                        {
                                            var totalAmountPaid = ctx.Claim_Payments
                                    .Where(cp => cp.Claim_No == claimId && cp.Payment_Source == "1" && ((cp.Deleted ?? false) == false))
                                    .Sum(cp => cp.Amount_Paid) ?? 0;

                                            if (!string.IsNullOrEmpty(totalAmountPaid.ToString()))
                                            {
                                                string totalAmountPaidString = totalAmountPaid.ToString("0.##");
                                                strBatchStringR += $"AMT*D*{totalAmountPaidString}~";
                                                strBatchString += $"AMT*D*{totalAmountPaidString}~";
                                                segmentCount++;
                                            }
                                        }

                                        #endregion

                                        #region OTHER INSURANCE COVERAGE INFORMATION

                                        if (!string.IsNullOrEmpty(SecondaryIns.GRelationship)
                                   && SecondaryIns.GRelationship.ToUpper().Equals("S"))
                                        {
                                            strBatchStringR += "OTHER INSURANCE COVERAGE INFORMATION~";
                                            strBatchStringR += "OI***Y*P**Y~"; //- Changed C to P as per 5010
                                            strBatchString += "OI***Y*P**Y~"; //- Changed C to P as per 5010
                                            segmentCount++;

                                        }
                                        else
                                        {
                                            strBatchStringR += "OI***Y*P**Y~"; //- Changed C to P as per 5010
                                            strBatchString += "OI***Y*P**Y~"; //- Changed C to P as per 5010
                                            segmentCount++;
                                        }
                                        #endregion

                                        #endregion

                                        #region LOOP 2330A (OTHER SUBSCRIBER NAME and Address)
                                        strBatchStringR += "Loop ID - 2330A - Other Subscriber Name~";
                                        if (!string.IsNullOrEmpty(primaryIns.GRelationship)
                                    && primaryIns.GRelationship.ToUpper().Trim().Equals("S"))
                                        {
                                            strBatchStringR += "Other Subscriber Name~";
                                            strBatchStringR += "NM1*IL*1*";
                                            strBatchString += "NM1*IL*1*";

                                            if (string.IsNullOrEmpty(claim.claimInfo.Lname) || string.IsNullOrEmpty(claim.claimInfo.Fname))
                                            {
                                                errorList.Add("Self -- Secondary Insurnace'subscriber Last/First Name missing.");
                                            }
                                            else
                                            {
                                                string policy_number;
                                                if (primaryIns != null)
                                                {
                                                    policy_number = primaryIns.Policy_Number.ToUpper();
                                                }
                                                else
                                                    policy_number = SecondaryIns.Policy_Number.ToUpper();
                                                strBatchStringR += claim.claimInfo.Lname + "*"
                                                        + claim.claimInfo.Fname + "*"
                                                        + claim.claimInfo.Mname + "***MI*"
                                                        + policy_number + "~";

                                                strBatchString += claim.claimInfo.Lname + "*"
                                                        + claim.claimInfo.Fname + "*"
                                                        + claim.claimInfo.Mname + "***MI*"
                                                        + policy_number + "~";
                                                segmentCount++;
                                            }
                                            if (string.IsNullOrEmpty(claim.claimInfo.Address)
                                                    || string.IsNullOrEmpty(claim.claimInfo.City)
                                                    || string.IsNullOrEmpty(claim.claimInfo.State)
                                                    || string.IsNullOrEmpty(claim.claimInfo.Zip))
                                            {
                                                errorList.Add("Self -- Subscriber Address incomplete.");
                                            }
                                            else
                                            {
                                                strBatchStringR += "Other Subscriber Address~";
                                                strBatchStringR += "N3*" + claim.claimInfo.Address + "~";
                                                strBatchString += "N3*" + claim.claimInfo.Address + "~";
                                                segmentCount++;

                                                strBatchStringR += "Other Subscriber City, State, ZIP~";
                                                strBatchStringR += "N4*" + claim.claimInfo.City + "*"
                                                        + claim.claimInfo.State + "*";
                                                strBatchString += "N4*" + claim.claimInfo.City + "*"
                                                        + claim.claimInfo.State + "*";
                                                if (string.IsNullOrEmpty(claim.claimInfo.Zip))
                                                {
                                                    strBatchStringR += "     " + "~";
                                                    strBatchString += "     " + "~";
                                                }
                                                else
                                                {
                                                    strBatchStringR += claim.claimInfo.Zip + "~";
                                                    strBatchString += claim.claimInfo.Zip + "~";
                                                }
                                                segmentCount++;
                                            }
                                        }
                                        else
                                        {
                                            strBatchStringR += "Other Subscriber Name~";
                                            strBatchStringR += "NM1*IL*1*";
                                            strBatchString += "NM1*IL*1*";

                                            if (string.IsNullOrEmpty(primaryIns.Glname) || string.IsNullOrEmpty(primaryIns.Gfname))
                                            {
                                                errorList.Add("Primary Insurnace'subscriber Last/First Name missing.");

                                            }
                                            else
                                            {
                                                strBatchStringR += primaryIns.Glname + "*"
                                                        + primaryIns.Gfname + "*"
                                                        + primaryIns.Gmi + "***MI*"
                                                        + primaryIns.Policy_Number.ToUpper() + "~";
                                                strBatchString += primaryIns.Glname + "*"
                                                        + primaryIns.Gfname + "*"
                                                        + primaryIns.Gmi + "***MI*"
                                                        + primaryIns.Policy_Number.ToUpper() + "~";
                                                segmentCount++;
                                            }
                                            if (string.IsNullOrEmpty(primaryIns.Gaddress)
                                                    || string.IsNullOrEmpty(primaryIns.Gcity)
                                                    || string.IsNullOrEmpty(primaryIns.Gstate)
                                                    || string.IsNullOrEmpty(primaryIns.Gzip))
                                            {
                                                errorList.Add("Secondary Subscriber Address incomplete.");
                                            }
                                            else
                                            {
                                                strBatchStringR += "Other Subscriber Address~";
                                                strBatchStringR += "N3*" + primaryIns.Gaddress + "~";
                                                strBatchString += "N3*" + primaryIns.Gaddress + "~";
                                                segmentCount++;

                                                strBatchStringR += "Other Subscriber City, State, ZIP~";

                                                strBatchStringR += "N4*" + primaryIns.Gcity + "*"
                                                        + primaryIns.Gstate + "*";
                                                strBatchString += "N4*" + primaryIns.Gcity + "*"
                                                        + primaryIns.Gstate + "*";
                                                if (string.IsNullOrEmpty(primaryIns.Gzip))
                                                {
                                                    strBatchStringR += "     " + "~";
                                                    strBatchString += "     " + "~";
                                                }
                                                else
                                                {
                                                    strBatchStringR += primaryIns.Gzip + "~";
                                                    strBatchString += primaryIns.Gzip + "~";
                                                }
                                                segmentCount++;
                                            }
                                        }
                                        #endregion


                                        #region LOOP 2330B (OTHER PAYER AND AND ADDRESS)
                                        strBatchStringR += "Loop ID -2330B - Other Payer Name~";
                                        string SecInsPayerName = "";
                                        if (string.IsNullOrEmpty(SecondaryIns.plan_name))
                                        {
                                            errorList.Add("Secondary's payer name missing.");
                                        }
                                        else
                                        {
                                            if (SecondaryIns.Insgroup_Name.Trim().Contains("MEDICARE"))
                                            {
                                                SecInsPayerName = "MEDICARE";
                                            }
                                            else
                                            {
                                                SecInsPayerName = SecondaryIns.plan_name;
                                            }
                                        }
                                        if (primaryIns != null)
                                        {
                                            string paperPayerName = "";
                                            if (!string.IsNullOrEmpty(primaryIns.plan_name) && primaryIns.plan_name.Trim().ToUpper().Equals("MEDICARE"))
                                            {
                                                paperPayerName = "MEDICARE";
                                            }
                                            else
                                            {
                                                paperPayerName = primaryIns.plan_name;
                                            }

                                            paperPayerID = primaryIns.Payer_Number;
                                            if (!string.IsNullOrEmpty(paperPayerID))
                                            {
                                                strBatchStringR += " Other Payer Name~";
                                                strBatchStringR += "NM1*PR*2*" + paperPayerName + "*****PI*" + paperPayerID + "~";
                                                strBatchString += "NM1*PR*2*" + paperPayerName + "*****PI*" + paperPayerID + "~";
                                                segmentCount++;
                                            }
                                            else
                                            {
                                                errorList.Add("Payer id is compulsory in case of Gateway EDI Clearing house.");
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(SecondaryIns.Payer_Number))
                                            {
                                                string secPayerNumber = primaryIns.Payer_Number.Equals(SecondaryIns.Payer_Number) ? SecondaryIns.Payer_Number + "A" : SecondaryIns.Payer_Number;
                                                strBatchStringR += " Other Payer Name~";
                                                strBatchStringR += "NM1*PR*2*" + SecInsPayerName + "*****PI*" + secPayerNumber + "~";
                                                strBatchString += "NM1*PR*2*" + SecInsPayerName + "*****PI*" + secPayerNumber + "~";
                                                segmentCount++;
                                            }
                                            else
                                            {
                                                errorList.Add("Secondary's insurance payer id is compulsory in case of Gateway EDI Clearing house.");
                                            }
                                        }

                                        //Obsolete
                                        //strBatchString += "N3*" + SecondaryIns.Gaddress + "~";
                                        //segmentCount++;
                                        //strBatchString += "N4*" + SecondaryIns.Gcity + "*" + SecondaryIns.Gstate + "*" + SecondaryIns.Gzip.Trim() + "~";
                                        //segmentCount++;

                                        if (primaryIns != null)
                                        {
                                            if (!string.IsNullOrEmpty(primaryIns.Insgroup_Name) && primaryIns.plan_name.Trim().ToUpper().Equals("WORK COMP"))
                                            {
                                                if (string.IsNullOrEmpty(primaryIns.Sub_Empaddress)
                                                        || string.IsNullOrEmpty(primaryIns.Sub_Emp_City)
                                                        || string.IsNullOrEmpty(primaryIns.Sub_Emp_State)
                                                        || string.IsNullOrEmpty(primaryIns.Sub_Emp_Zip))
                                                {
                                                    errorList.Add("Payer is Worker Company, so its subscriber employer’s address is necessary.");

                                                }
                                                strBatchStringR += "Other Payer Address~";
                                                strBatchStringR += "N3*" + primaryIns.Sub_Empaddress + "~";
                                                strBatchString += "N3*" + primaryIns.Sub_Empaddress + "~";
                                                segmentCount++;

                                                strBatchStringR += "Payer City, State, ZIP~";
                                                strBatchStringR += "N4*" + primaryIns.Sub_Emp_City + "*"
                                                        + primaryIns.Sub_Emp_State + "*";

                                                strBatchString += "N4*" + primaryIns.Sub_Emp_City + "*"
                                                        + primaryIns.Sub_Emp_State + "*";
                                                if (!string.IsNullOrEmpty(primaryIns.Sub_Emp_Zip))
                                                {
                                                    strBatchStringR += primaryIns.Sub_Emp_Zip;
                                                    strBatchString += primaryIns.Sub_Emp_Zip;

                                                }
                                                else
                                                {
                                                    strBatchStringR += "     ";
                                                    strBatchString += "     ";
                                                }
                                                strBatchStringR += "~";
                                                strBatchString += "~";
                                                segmentCount++;
                                            }
                                            else
                                            {
                                                if (string.IsNullOrEmpty(primaryIns.Ins_Address)
                                                        || string.IsNullOrEmpty(primaryIns.Ins_City)
                                                        || string.IsNullOrEmpty(primaryIns.Ins_State)
                                                        || string.IsNullOrEmpty(primaryIns.Ins_Zip))
                                                {
                                                    errorList.Add("Payer address incomplete.");
                                                }
                                                strBatchStringR += "Payer Address~";
                                                strBatchStringR += "N3*" + primaryIns.Ins_Address;
                                                strBatchStringR += "~";
                                                strBatchString += "N3*" + primaryIns.Ins_Address;
                                                strBatchString += "~";
                                                segmentCount++;

                                                strBatchStringR += "Payer City, State, ZIP~";
                                                strBatchStringR += "N4*" + primaryIns.Ins_City + "*" + primaryIns.Ins_State + "*";
                                                strBatchStringR += (string.IsNullOrEmpty(primaryIns.Ins_Zip)) ? "     " : primaryIns.Ins_Zip.Trim();
                                                strBatchStringR += "~";
                                                strBatchString += "N4*" + primaryIns.Ins_City + "*" + primaryIns.Ins_State + "*";
                                                strBatchString += (string.IsNullOrEmpty(primaryIns.Ins_Zip)) ? "     " : primaryIns.Ins_Zip.Trim();
                                                strBatchString += "~";
                                                segmentCount++;
                                            }
                                        }
                                        else
                                        {
                                            strBatchStringR += "Payer Address~";
                                            strBatchString += "N3*" + SecondaryIns.Ins_Address + "~";
                                            strBatchString += "N3*" + SecondaryIns.Ins_Address + "~";
                                            segmentCount++;
                                            strBatchStringR += "Payer City, State, ZIP~";
                                            strBatchStringR += "N4*" + SecondaryIns.Ins_City + "*" + SecondaryIns.Ins_State + "*" + SecondaryIns.Ins_Zip.Trim() + "~";
                                            strBatchString += "N4*" + SecondaryIns.Ins_City + "*" + SecondaryIns.Ins_State + "*" + SecondaryIns.Ins_Zip.Trim() + "~";
                                            segmentCount++;
                                        }

                                        #endregion
                                    }
                                    #region New:NM1*DQ - SUPERVISING PROVIDER
                                    //var claimid = claim.claim_No;
                                    //if (sPDataModels != null && sPDataModels.Any())
                                    //{
                                    //    strBatchString += "NM1*DQ*1*" + sPDataModels[0].ProviderLNamer + "*"
                                    //                  + sPDataModels[0].ProviderFNamer + "****XX*"
                                    //                  + claim.claimInfo.Att_Npi + "~";
                                    //    segmentCount++;
                                    //}
                                    // Above code is commented and below is added by Shahzad Khan EDI for live Fixation
                                    if (sPDataModels != null && sPDataModels.Any())
                                    {
                                        var ProviderL = sPDataModels[0].ProviderLNamer;

                                        if ((!string.IsNullOrEmpty(ProviderL) && (claim.claimInfo.Att_Lname != sPDataModels[0].ProviderLNamer && claim.claimInfo.Att_Fname != sPDataModels[0].ProviderFNamer)))
                                        {
                                            strBatchString += "NM1*DQ*1*" + sPDataModels[0].ProviderLNamer + "*"
                                                       + sPDataModels[0].ProviderFNamer + "****XX*"
                                                       + sPDataModels[0].ProviderNpi + "~";
                                            segmentCount++;
                                        }


                                    }
                                    #endregion End!

                                    //---Adding Submit/RESUBMIT CLAIM CPTS-----------
                                    int line_no = 0;
                                    if (claim.claimProcedures != null && claim.claimProcedures.Count() > 0)
                                    {
                                        //                 using (var ctx = new NPMDBEntities())
                                        //                 {
                                        //                     datamodel = ctx.Claim_Payments
                                        //.Where(cp => cp.Claim_No == claim_id)
                                        //.Select(cp => new DataModel
                                        //{
                                        //    Amount_Approved = cp.Amount_Approved,
                                        //    Amount_Paid = cp.Amount_Paid,
                                        //    Amount_Adjusted = cp.Amount_Adjusted,
                                        //    Reject_Type = cp.Reject_Type,
                                        //    Reject_Amount = cp.Reject_Amount,
                                        //    Paid_Proc_Code = cp.Paid_Proc_Code,
                                        //    Charged_Proc_Code = cp.Charged_Proc_Code,
                                        //    Insurance_Id = cp.Insurance_Id,
                                        //    ERA_CATEGORY_CODE = cp.ERA_CATEGORY_CODE,
                                        //    ERA_ADJUSTMENT_CODE = cp.ERA_ADJUSTMENT_CODE,
                                        //    ERA_Rejection_CATEGORY_CODE = cp.ERA_Rejection_CATEGORY_CODE,
                                        //    DOS_From = cp.DOS_From,
                                        //    Dos_To = cp.Dos_To,
                                        //    payment_source = cp.Payment_Source,
                                        //    Date_Filing = cp.Date_Filing,
                                        //    ICN = cp.ICN,

                                        //})
                                        //.ToList();


                                        //                 }
                                        #region ICN 
                                        string ICN = "";
                                        if (datamodel.Count > 0)
                                        {

                                            ICN = datamodel[0].ICN;
                                        }
                                        if (!string.IsNullOrEmpty(ICN))
                                        {
                                            strBatchStringR += "REF*F8 ICN~";
                                            strBatchStringR += "REF*F8" + "*" + ICN + "~";
                                            strBatchString += "REF*F8" + "*" + ICN + "~";
                                            segmentCount++;
                                        }
                                        #endregion
                                        foreach (var proc in claim.claimProcedures)
                                        {

                                            if (claim.claimInfo.Is_Resubmitted && !proc.Is_Resubmitted)
                                            {
                                                continue;
                                            }

                                            line_no = line_no + 1;

                                            #region LOOP 2400
                                            strBatchStringR += "Loop ID -2400 - Service Line Number~";


                                            #region SERVICE LINE       
                                            strBatchStringR += "Service Line Number~";
                                            strBatchStringR += "LX*" + line_no + "~";
                                            strBatchString += "LX*" + line_no + "~";
                                            segmentCount++;
                                            #endregion


                                            #region PROFESSIONAL SERVICE
                                            if (!string.IsNullOrEmpty(claim.claimInfo.Claim_Pos))
                                            {

                                                if (proc.Total_Charges > 0)
                                                {
                                                    string modifiers = "";
                                                    if (!string.IsNullOrEmpty(proc.Mod1.Trim()))
                                                    {
                                                        modifiers += ":" + proc.Mod1.Trim();
                                                    }
                                                    else
                                                    {
                                                        modifiers += ":";
                                                    }
                                                    if (!string.IsNullOrEmpty(proc.Mod2.Trim()))
                                                    {
                                                        modifiers += ":" + proc.Mod2.Trim();
                                                    }
                                                    else
                                                    {
                                                        modifiers += ":";
                                                    }
                                                    if (!string.IsNullOrEmpty(proc.Mod3.Trim()))
                                                    {
                                                        modifiers += ":" + proc.Mod3.Trim();
                                                    }
                                                    else
                                                    {
                                                        modifiers += ":";
                                                    }
                                                    if (!string.IsNullOrEmpty(proc.Mod4.Trim()))
                                                    {
                                                        modifiers += ":" + proc.Mod4.Trim();
                                                    }
                                                    else
                                                    {
                                                        modifiers += ":";
                                                    }

                                                    strBatchStringR += "Professional Service~";
                                                    strBatchStringR += "SV1*HC:" + proc.Proc_Code.Trim() + modifiers + "*"
                                                            + string.Format("{0:0.00}", proc.Total_Charges) + "*UN*"
                                                            + proc.Units + "*"
                                                            + claim.claimInfo.Claim_Pos + "*"
                                                            + "*";

                                                    strBatchString += "SV1*HC:" + proc.Proc_Code.Trim() + modifiers + "*"
                                                            + string.Format("{0:0.00}", proc.Total_Charges) + "*UN*"
                                                            + proc.Units + "*"
                                                            + claim.claimInfo.Claim_Pos + "*"
                                                            + "*";
                                                }
                                                else
                                                {
                                                    errorList.Add("Procedure Code:  " + proc.Proc_Code.Trim() + " has ZERO charges");
                                                }
                                            }
                                            else
                                            {
                                                errorList.Add("Claim's pos code missing");
                                            }

                                            string pointers = "";
                                            if (proc.Dx_Pointer1 > 0)
                                            {
                                                pointers = proc.Dx_Pointer1.ToString();
                                            }
                                            if (proc.Dx_Pointer2 > 0)
                                            {
                                                pointers += ":" + proc.Dx_Pointer2.ToString();
                                            }
                                            if (proc.Dx_Pointer3 > 0)
                                            {
                                                pointers += ":" + proc.Dx_Pointer3.ToString();
                                            }
                                            if (proc.Dx_Pointer4 > 0)
                                            {
                                                pointers += ":" + proc.Dx_Pointer4.ToString();
                                            }
                                            strBatchStringR += pointers + "~";
                                            strBatchString += pointers + "~";
                                            segmentCount++;

                                            #endregion

                                            #region SERVICE Date
                                            strBatchStringR += "Service Date~";
                                            strBatchStringR += "DTP*472*RD8*";
                                            strBatchString += "DTP*472*RD8*";

                                            string[] splittedFROMDOS = proc.DosFrom.Split('/');
                                            string[] splittedTODOS = proc.Dos_To.Split('/');
                                            string Date_Of_Service_FROM = splittedFROMDOS[0] + splittedFROMDOS[1] + splittedFROMDOS[2];
                                            string Date_Of_Service_TO = splittedTODOS[0] + splittedTODOS[1] + splittedTODOS[2];
                                            strBatchStringR += Date_Of_Service_FROM + "-" + Date_Of_Service_TO + "~";
                                            strBatchString += Date_Of_Service_FROM + "-" + Date_Of_Service_TO + "~";
                                            segmentCount++;
                                            #endregion

                                            #region LINE Note



                                            if (DateTime.TryParse(proc.DosFrom, out procDosFrom) && DateTime.TryParse(proc.Dos_To, out procDosTo))
                                            {
                                                if (datamodel.Count > 0)
                                                {
                                                    foreach (var dm in datamodel)
                                                    {
                                                        if (dm.Paid_Proc_Code == proc.Proc_Code
                                                            && dm.DOS_From == procDosFrom
                                                            && dm.Dos_To == procDosTo)
                                                        {
                                                            dataModels.Add(dm);
                                                        }
                                                    }
                                                }

                                            }
                                            else
                                            {
                                            }

                                            #region 2430 LINE ADJUDICATION INFORMATION
                                            strBatchStringR += "2430 LINE ADJUDICATION INFORMATION~";
                                            var paymentSource = "";
                                            using (var ctx = new NPMDBEntities())
                                            {
                                                paymentSource = ctx.Claim_Payments
                                       .Where(cp => cp.Paid_Proc_Code == proc.Proc_Code
                                                    && cp.DOS_From == procDosFrom
                                                    && cp.Dos_To == procDosTo
                                                    && cp.Claim_No == claimId)
                                       .Select(cp => cp.Payment_Source)
                                       .FirstOrDefault();
                                            }

                                            //..check agaisnt B is added by Muhammad Abbas from EDI
                                            if (paymentSource == "1" && SecondaryIns != null && (secStatus == "N" || secStatus == "R" || secStatus == "B"))
                                            {
                                                #region LINE ITEM CONTROL NUMBER (CLAIM PROCEDURES ID)
                                                strBatchStringR += "Line Item Control Number~";
                                                strBatchStringR += "REF*6R*" + proc.Claim_Procedures_Id.ToString() + "~";

                                                strBatchString += "REF*6R*" + proc.Claim_Procedures_Id.ToString() + "~";
                                                segmentCount++;

                                            #endregion

                                            #region LOOP 2410 (DRUG IDENTIFICATION) Adding New for CTP
                                            strBatchStringR += "LOOP - 2410 - DRUG IDENTIFICATION~";

                                            if (!string.IsNullOrEmpty(proc.Ndc_Code))
                                            {
                                                strBatchStringR += "Procdure NDC Code~";
                                                strBatchStringR += "LIN**N4*" + proc.Ndc_Code.Trim() + "~";
                                                strBatchString += "LIN**N4*" + proc.Ndc_Code.Trim() + "~";
                                                segmentCount++;
                                                if (proc.Ndc_Qty > 0)
                                                {
                                                    if (!string.IsNullOrEmpty(proc.Ndc_Measure))
                                                    {
                                                        strBatchStringR += "Procdure  Ndc_Qty Ndc_Measure~";
                                                        strBatchStringR += "CTP****" + proc.Ndc_Qty.ToString() + "*" + proc.Ndc_Measure + "*~";
                                                        strBatchString += "CTP****" + proc.Ndc_Qty.ToString() + "*" + proc.Ndc_Measure + "*~";
                                                        segmentCount++;
                                                    }
                                                    else
                                                    {
                                                        errorList.Add("Procedure NDC Quantity/Qual or Unit Price is missing.");
                                                    }
                                                }

                                            }

                                            #endregion
                                            decimal? totalAmountPaid = 0;
                                                if (dataModels.Count > 0)
                                                {
                                                    foreach (var c in dataModels)
                                                    {
                                                        if (DateTime.TryParse(proc.DosFrom, out procDosFrom_test)
                                                            && DateTime.TryParse(proc.Dos_To, out procDosTo_test)
                                                            && procDosFrom_test == c.DOS_From
                                                            && procDosTo_test == c.Dos_To
                                                            && c.Charged_Proc_Code == proc.Proc_Code) //..this last condition is added by asim mehmood and abbas from edi
                                                        {
                                                            totalAmountPaid += (decimal)c.Amount_Paid;
                                                        }
                                                    }
                                                }
                                                string formattedDate = "";
                                                strBatchStringR += "Adjudication Information";
                                                strBatchStringR += "SVD*";
                                                strBatchStringR += primaryIns.Payer_Number + "*" + totalAmountPaid.Value.ToString("0.##") + "*" + "HC:" + proc.Proc_Code + "**" + proc.Units + "~";
                                                strBatchString += "SVD*";
                                                strBatchString += primaryIns.Payer_Number + "*" + totalAmountPaid.Value.ToString("0.##") + "*" + "HC:" + proc.Proc_Code + "**" + proc.Units + "~";
                                                segmentCount++;
                                                if (dataModels.Count > 0)
                                                {
                                                    //foreach (var c in dataModels)
                                                    //{
                                                    //    Date_Filing = c.Date_Filing;

                                                    //    if (!string.IsNullOrEmpty(c.ERA_ADJUSTMENT_CODE))
                                                    //    {
                                                    //        //Date_Filing = c.Date_Filing;
                                                    //        //formattedDate = GetFormattedDate();
                                                    //        //string[] splittedDATEFILING = c.Date_Filing;
                                                    //        //string Date_Of_Service_TO1 = splittedDATEFILING[0] + splittedDATEFILING[1] + splittedDATEFILING[2];
                                                    //        strBatchString += "CAS*";
                                                    //        strBatchString += c.ERA_CATEGORY_CODE ?? "CO";
                                                    //        strBatchString += "*";
                                                    //        strBatchString += c.ERA_ADJUSTMENT_CODE + "*" + c.Amount_Adjusted.Value.ToString("0.##");
                                                    //        strBatchString += "~";
                                                    //        segmentCount++;
                                                    //    }

                                                    //    if ((!string.IsNullOrEmpty(c.Reject_Type)) && DateTime.TryParse(proc.DosFrom, out procDosFrom_test)
                                                    //            && DateTime.TryParse(proc.Dos_To, out procDosTo_test)
                                                    //            && procDosFrom_test == c.DOS_From
                                                    //            && procDosTo_test == c.Dos_To)
                                                    //    {

                                                    //        strBatchString += "CAS*";
                                                    //        strBatchString += "PR" + "*";
                                                    //        switch (c.Reject_Type)
                                                    //        {
                                                    //            case "A":
                                                    //                c.Reject_Type = "1";
                                                    //                break;
                                                    //            case "06":
                                                    //                c.Reject_Type = "2";
                                                    //                break;
                                                    //            case "c":
                                                    //                c.Reject_Type = "3";
                                                    //                break;
                                                    //            case "CO":
                                                    //                c.Reject_Type = "3";
                                                    //                break;
                                                    //            case "1":
                                                    //                c.Reject_Type = "1";
                                                    //                break;
                                                    //            case "2":
                                                    //                c.Reject_Type = "2";
                                                    //                break;
                                                    //            case "3":
                                                    //                c.Reject_Type = "3";
                                                    //                break;
                                                    //            default:
                                                    //                c.Reject_Type = c.Reject_Type;
                                                    //                break;
                                                    //        }
                                                    //        strBatchString += c.Reject_Type + "*" + c.Reject_Amount.Value.ToString("0.##");
                                                    //        strBatchString += "~";
                                                    //        segmentCount++;
                                                    //    }

                                                    //}
                                                    //..above code is commented by asim mehmood and below foreach is added by asim edi aswell
                                                    foreach (var c in dataModels)
                                                    {
                                                        Date_Filing = c.Date_Filing;

                                                        if (!string.IsNullOrEmpty(c.ERA_ADJUSTMENT_CODE) && !string.IsNullOrEmpty(c.Charged_Proc_Code) && !string.IsNullOrEmpty(proc.Proc_Code))
                                                        {
                                                            if (c.Charged_Proc_Code == proc.Proc_Code)
                                                            {

                                                                //Date_Filing = c.Date_Filing;

                                                                //formattedDate = GetFormattedDate();

                                                                //string[] splittedDATEFILING = c.Date_Filing;

                                                                //string Date_Of_Service_TO1 = splittedDATEFILING[0] + splittedDATEFILING[1] + splittedDATEFILING[2];
                                                                strBatchStringR += "Line adjustment~";

                                                                strBatchStringR += "CAS*";
                                                                strBatchStringR += c.ERA_CATEGORY_CODE ?? "CO";
                                                                strBatchStringR += "*";
                                                                strBatchStringR += c.ERA_ADJUSTMENT_CODE + "*" + c.Amount_Adjusted.Value.ToString("0.##");
                                                                strBatchStringR += "~";

                                                                strBatchString += "CAS*";

                                                                strBatchString += c.ERA_CATEGORY_CODE ?? "CO";

                                                                strBatchString += "*";

                                                                strBatchString += c.ERA_ADJUSTMENT_CODE + "*" + c.Amount_Adjusted.Value.ToString("0.##");

                                                                strBatchString += "~";

                                                                segmentCount++;

                                                            }

                                                        }

                                                        if ((!string.IsNullOrEmpty(c.Reject_Type)) && DateTime.TryParse(proc.DosFrom, out procDosFrom_test)
                                                    && DateTime.TryParse(proc.Dos_To, out procDosTo_test)
                                                    && procDosFrom_test == c.DOS_From
                                                    && procDosTo_test == c.Dos_To
                                                    && c.Charged_Proc_Code == proc.Proc_Code
                                                    && !string.IsNullOrEmpty(c.Charged_Proc_Code)
                                                    && !string.IsNullOrEmpty(proc.Proc_Code)

                                                                )

                                                        {

                                                            if (c.Charged_Proc_Code == proc.Proc_Code)
                                                            {
                                                                strBatchStringR += "Line adjustment~";
                                                                strBatchStringR += "CAS*";
                                                                strBatchStringR += "PR" + "*";

                                                                strBatchString += "CAS*";

                                                                strBatchString += "PR" + "*";

                                                                switch (c.Reject_Type)
                                                                {
                                                                    case "A":

                                                                        c.Reject_Type = "1";

                                                                        break;

                                                                    case "06":

                                                                        c.Reject_Type = "2";

                                                                        break;

                                                                    case "c":

                                                                        c.Reject_Type = "3";

                                                                        break;

                                                                    case "CO":

                                                                        c.Reject_Type = "3";

                                                                        break;

                                                                    case "1":

                                                                        c.Reject_Type = "1";

                                                                        break;

                                                                    case "2":

                                                                        c.Reject_Type = "2";

                                                                        break;

                                                                    case "3":

                                                                        c.Reject_Type = "3";

                                                                        break;

                                                                    default:

                                                                        c.Reject_Type = c.Reject_Type;

                                                                        break;

                                                                }
                                                                strBatchStringR += c.Reject_Type + "*" + c.Reject_Amount.Value.ToString("0.##");

                                                                strBatchStringR += "~";
                                                                strBatchString += c.Reject_Type + "*" + c.Reject_Amount.Value.ToString("0.##");

                                                                strBatchString += "~";

                                                                segmentCount++;

                                                            }

                                                        }

                                                    }

                                                }
                                                strBatchStringR += "Line Adjudication date~";
                                                strBatchStringR += "DTP*573*D8*";
                                                strBatchString += "DTP*573*D8*";
                                                formattedDate = GetFormattedDate();
                                                strBatchStringR += formattedDate;
                                                strBatchStringR += "~";

                                                strBatchString += formattedDate;
                                                strBatchString += "~";
                                                segmentCount++;
                                            }

                                            #endregion


                                            #endregion

                                            #region LINE ITEM CONTROL NUMBER (CLAIM PROCEDURES ID)
                                            if (SecondaryIns == null && (secStatus != "N" || secStatus != "R"))
                                            {
                                                strBatchStringR += "Line Item Control Number LINE ITEM CONTROL NUMBER~";
                                                strBatchStringR += "REF*6R*" + proc.Claim_Procedures_Id.ToString() + "~";
                                                strBatchString += "REF*6R*" + proc.Claim_Procedures_Id.ToString() + "~";
                                                segmentCount++;

                                            }
                                            #endregion

                                            #region LINE Note
                                            if (!string.IsNullOrEmpty(proc.Notes.Trim()))
                                            {
                                                strBatchStringR += "LINE Note~";
                                                strBatchStringR += "NTE*ADD*" + proc.Notes.Trim() + "~";
                                                strBatchString += "NTE*ADD*" + proc.Notes.Trim() + "~";
                                                segmentCount++;
                                            }

                                            #endregion

                                            #endregion


                                            #region LOOP 2410 (DRUG IDENTIFICATION)
                                            //strBatchStringR += "LOOP - 2410 - DRUG IDENTIFICATION~";

                                            //if (!string.IsNullOrEmpty(proc.Ndc_Code))
                                            //{
                                            //    strBatchStringR += "Procdure NDC Code~";
                                            //    strBatchStringR += "LIN**N4*" + proc.Ndc_Code.Trim() + "~";
                                            //    strBatchString += "LIN**N4*" + proc.Ndc_Code.Trim() + "~";
                                            //    segmentCount++;
                                            //    if (proc.Ndc_Qty > 0)
                                            //    {
                                            //        if (!string.IsNullOrEmpty(proc.Ndc_Measure))
                                            //        {
                                            //            strBatchStringR += "Procdure  Ndc_Qty Ndc_Measure~";
                                            //            strBatchStringR += "CTP****" + proc.Ndc_Qty.ToString() + "*" + proc.Ndc_Measure + "*~";
                                            //            strBatchString += "CTP****" + proc.Ndc_Qty.ToString() + "*" + proc.Ndc_Measure + "*~";
                                            //            segmentCount++;
                                            //        }
                                            //        else
                                            //        {
                                            //            errorList.Add("Procedure NDC Quantity/Qual or Unit Price is missing.");
                                            //        }
                                            //    }

                                            //}

                                            #endregion
                                        }
                                    }
                                    if (line_no == 0)
                                    {
                                        errorList.Add("Claim Procedures missing.");
                                    }
                                }
                            }

                        }
                    }


                    if (errorList.Count == 0)
                    {
                        segmentCount += 3;
                        strBatchStringR += "~SE*" + segmentCount + "*0001~";
                        strBatchStringR += "GE * 1 * " + batchId + "~";
                        strBatchStringR += "IEA*1*000000001~";

                        strBatchString += "SE*" + segmentCount + "*0001~GE*1*" + batchId + "~IEA*1*000000001~";

                        objResponse.Status = "Success";
                        objResponse.Response = strBatchString;
                        objResponse.Response1 = strBatchStringR;

                        //using (var w = new StreamWriter(HttpContext.Current.Server.MapPath("/SubmissionFile/" + claim_id + ".txt"), false))
                        //{
                        //    w.WriteLine(strBatchString);
                        //}

                    }
                    else
                    {
                        objResponse.Status = "Error";
                        objResponse.Response = errorList;
                    }


            }
            catch (Exception)
            {
                throw;
            }
            return objResponse;
        }

        public void ParseFile(string file_Data)
        {
            String date_of_interchange = "";
            String batch_process_id = "";
            String batch_status = "";
            String batch_status_detail = "";
            string response_file_path = "";
            string[] Segment = file_Data.Split('~');
            if (Segment.Length > 0)
            {
                for (int i = 0; i < Segment.Length; i++)
                {
                    string[] SubSegments = Segment[i].Split('*');
                    if (SubSegments.Length > 0)
                    {
                        if (SubSegments[0].ToUpper() == "ISA")
                        {
                            if (SubSegments.Length > 8)
                            {
                                date_of_interchange = SubSegments[9];
                            }
                        }
                        else if (SubSegments[0].ToUpper() == "AK1")
                        {
                            if (SubSegments.Length > 2)
                            {
                                batch_process_id = SubSegments[2];
                            }
                        }
                        else if (batch_process_id != null && batch_process_id != "")
                        {
                            if (SubSegments[0].ToUpper() == "AK9")
                            {
                                batch_status = SubSegments[1];
                                if (SubSegments[1].ToUpper() == "A")
                                {
                                    batch_status_detail = "Accepted";
                                }
                                else if (SubSegments[1].ToUpper() == "E")
                                {
                                    batch_status_detail = "Accepted, but errors were noted";
                                }
                                else if (SubSegments[1].ToUpper() == "M")
                                {
                                    batch_status_detail = "Rejected, message authentication code (MAC) failed";
                                }
                                else if (SubSegments[1].ToUpper() == "P")
                                {
                                    batch_status_detail = "Partially accepted, at least one transaction set was rejected";
                                }
                                else if (SubSegments[1].ToUpper() == "R")
                                {
                                    batch_status_detail = "Rejected";
                                }
                                else if (SubSegments[1].ToUpper() == "W")
                                {
                                    batch_status_detail = "Rejected, assurance failed validity tests";
                                }
                                else if (SubSegments[1].ToUpper() == "X")
                                {
                                    batch_status_detail = "Rejected, content after decryption could not be analyzed";
                                }
                            }
                        }
                    }
                }

            }

            string query = "update claim_batch set date_processed='" + date_of_interchange + "',batch_status='" + batch_status + "',batch_status_detail='" + batch_status_detail + "',response_file_path='" + response_file_path + "' where batch_id='" + batch_process_id + "'";
        }
        private string appendDxCodesegmentCount(int diagCount, bool isICD_10, string diagCode)
        {
            string strDiagsegmentCount = "";

            if (!string.IsNullOrEmpty(diagCode))
            {
                if (diagCount == 0)
                {
                    strDiagsegmentCount += diagCode.Trim().Replace(".", "");
                }
                else if (isICD_10)
                {
                    strDiagsegmentCount += "*ABF:" + diagCode.Trim().Replace(".", "");
                    //BF==ICD-9 ABF=ICD-10
                }
                else // ICD-9 Claim
                {
                    strDiagsegmentCount += "*BF:" + diagCode.Trim().Replace(".", "");
                    //BF==ICD-9 ABF=ICD-10
                }

            }

            return strDiagsegmentCount;
        }
        private string appendDxCodesegmentCount837I(int diagCount, bool isICD_10, string diagCode)
        {
            string strDiagsegmentCount = "";
            if (diagCount < 1)
            {
                if (!string.IsNullOrEmpty(diagCode))
                {
                    if (diagCount == 0)
                    {
                        strDiagsegmentCount += diagCode.Trim().Replace(".", "");
                    }
                    else if (isICD_10)
                    {
                        strDiagsegmentCount += "*ABF:" + diagCode.Trim().Replace(".", "");
                        //BF==ICD-9 ABF=ICD-10
                    }
                    else // ICD-9 Claim
                    {
                        strDiagsegmentCount += "*BF:" + diagCode.Trim().Replace(".", "");
                        //BF==ICD-9 ABF=ICD-10
                    }
                }
            }
            return strDiagsegmentCount;
        }
        private bool isAlphaNumeric(string value)
        {
            Regex regxAlphaNum = new Regex("^[a-zA-Z0-9 ]*$");

            return regxAlphaNum.IsMatch(value);
        }
        public ResponseModel GetClaimsSearchModels()
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {

                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        public ResponseModel SearchClaim(ClaimSearchViewModel model)
        {
            ResponseModel resModel = new ResponseModel();
            //List<SP_ClaimsSearch_Result> result = new List<SP_ClaimsSearch_Result>();
            List<SP_ClaimsSearch_EDI_Result> result = new List<SP_ClaimsSearch_EDI_Result>();
            try
            {
                //using (var ctx = new NPMDBEntities())
                //{
                //    result = ctx.SP_ClaimsSearch(model.DOSFrom, model.DOSTo, string.Join(",", model.PatientAccount), string.Join(",", model.Provider), string.Join(",", model.location), model.icd9, model.type, model.status, string.Join(",", model.insurance), model.PracticeCode).ToList();

                //}
                using (var ctx = new NPMDBEntities())
                {
                    result = ctx.SP_ClaimsSearch_EDI(model.DOSFrom, model.DOSTo, string.Join(",", model.PatientAccount), string.Join(",", model.Provider), string.Join(",", model.location), model.icd9, model.type, model.billedTo, model.status, string.Join(",", model.insurance), model.PracticeCode).ToList();

                }
                if (result != null)
                {
                    resModel.Status = "Success";
                    resModel.Response = result;
                }
                else
                {
                    resModel.Status = "No record found";
                }
            }
            catch (Exception ex)
            {
                resModel.Status = ex.ToString();
            }
            return resModel;
        }
        public ResponseModel AddUpdateBatch(BatchCreateViewModel model, long userId)
        {
            DateTime parsedDate;
            if (DateTime.TryParse(model.DateStr, out parsedDate))
            {
                model.Date = parsedDate;
            }
            ResponseModel responseModel = new ResponseModel();
            GenerateBatchName(model);
            claim_batch batch;
            try
            {
                using (var ctx = new NPMDBEntities())
                {

                    if (model.BatchId != 0)
                    {
                        batch = ctx.claim_batch.FirstOrDefault(b => b.batch_id == model.BatchId);
                        if (batch != null)
                        {
                            batch.batch_name = model.BatchName;
                            batch.batch_type = model.BatchType;
                            batch.batch_claim_type = model.batch_claim_type;
                            batch.practice_id = model.PracticeCode;
                            batch.provider_id = model.ProviderCode == -1 ? null : model.ProviderCode;
                            batch.modified_user = userId;
                            batch.date_modified = DateTime.Now;
                            batch.date = model.Date;
                            batch.Submission_Type = model.submission_type;
                            ctx.Entry(batch).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    else
                    {
                        batch = new claim_batch()
                        {

                            batch_id = Convert.ToInt64(ctx.SP_TableIdGenerator("batch_id").FirstOrDefault()),
                            batch_status = "Pending",
                            batch_name = GenerateBatchName(model),
                            batch_type = model.BatchType,
                            batch_claim_type = model.batch_claim_type,
                            practice_id = model.PracticeCode,
                            provider_id = model.ProviderCode == -1 ? null : model.ProviderCode,
                            created_user = userId,
                            date = model.Date,
                            date_created = DateTime.Now,
                            Submission_Type = model.submission_type
                        };
                        ctx.claim_batch.Add(batch);
                    }
                    if (ctx.SaveChanges() > 0)
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = model.BatchId;
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        private string GenerateBatchName(BatchCreateViewModel model)
        {
            string batchName = string.Empty;
            try
            {
                claim_batch batch = null;
                using (var ctx = new NPMDBEntities())
                {
                    if (model.ProviderCode != null && model.ProviderCode != -1)
                    {
                        var provider = ctx.Providers.FirstOrDefault(p => p.Provider_Code == model.ProviderCode && p.Practice_Code == model.PracticeCode);
                        if (provider != null)
                        {
                            if (model.BatchType == "P")
                            {
                                batchName = provider.Provid_LName.Substring(0, 1).ToUpper() + provider.Provid_FName.Substring(0, 1).ToUpper() + "_" + "P" + "_" + model.batch_claim_type + "_" + model.Date.ToString("MMddyyyy") + "_";
                            }
                            else
                                batchName = provider.Provid_LName.Substring(0, 1).ToUpper() + provider.Provid_FName.Substring(0, 1).ToUpper() + "_" + "I" + "_" + model.batch_claim_type + "_" + model.Date.ToString("MMddyyyy") + "_";
                        }
                        batch = ctx.claim_batch.Where(b => b.provider_id == model.ProviderCode && b.practice_id == model.PracticeCode && b.date == model.Date && b.batch_type == model.BatchType).OrderByDescending(d => d.date_created).FirstOrDefault();
                    }
                    else
                    {
                        if (model.BatchType == "P")
                        {
                            batchName = "AL" + "_" + "P" + "_" + model.batch_claim_type + "_" + model.Date.ToString("MMddyyyy") + "_";
                        }
                        else
                            batchName = "AL" + "_" + "I" + "_" + model.batch_claim_type + "_" + model.Date.ToString("MMddyyyy") + "_";
                        batch = ctx.claim_batch.Where(b => b.provider_id == null && b.practice_id == model.PracticeCode && b.date == model.Date  && b.batch_type == model.BatchType).OrderByDescending(d => d.date_created).FirstOrDefault();
                    }
                    int counter = 0;
                    if (batch != null)
                    {
                        counter = Convert.ToInt32(batch.batch_name.Substring(batch.batch_name.LastIndexOf("_") + 1));
                        counter++;
                    }
                    else
                    {
                        counter++;
                    }
                    batchName += model.PracticeCode;
                    batchName += $"_{counter}";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return batchName;
        }
        public ResponseModel GetPendingBatchSelectListErrorCom(long practiceCode, long? providerCode, string practype)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SelectListViewModel> list;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    providerCode = providerCode == -1 ? null : providerCode;
                    if (providerCode == null || providerCode == 0)
                    {
                        list = ctx.claim_batch.Where(b => (b.practice_id == practiceCode && (b.deleted ?? false) == false && (b.batch_lock ?? false) == false && (b.batch_status == "Pending" || b.batch_status == "" || b.batch_status == null) &&
        ((practype == "P" && (b.batch_type == "P" || b.batch_type == null)) ||
         (practype == "I" && b.batch_type == "I") ||
         (practype == "all" && (b.batch_type == "P" || b.batch_type == "I" || b.batch_type == null))))).Select(b => new SelectListViewModel()
         {
             Id = b.batch_id,
             Name = b.batch_id + "|" + b.batch_name
         }).ToList();
                    }
                    else
                    {
                        list = ctx.claim_batch.Where(b => (b.practice_id == practiceCode && (b.deleted ?? false) == false && b.provider_id == providerCode && (b.batch_lock ?? false) == false && (b.batch_status == "Pending" || b.batch_status == "" || b.batch_status == null) &&
        ((practype == "P" && (b.batch_type == "P" || b.batch_type == null)) ||
         (practype == "I" && b.batch_type == "I") ||
         (practype == "all" && (b.batch_type == "P" || b.batch_type == "I" || b.batch_type == null))))).Select(b => new SelectListViewModel()
         {
             Id = b.batch_id,
             Name = b.batch_id + "|" + b.batch_name
         }).ToList();
                    }
                    if (list != null)
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = list;
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        public ResponseModel GetPendingBatchSelectList(long practiceCode, long? providerCode , string practype,string batch_claim_type)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SelectListViewModel> list;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    providerCode = providerCode == -1 ? null : providerCode;
                    if (providerCode == null || providerCode == 0)
                    {
                        list = ctx.claim_batch.Where(b => (b.practice_id == practiceCode && b.batch_claim_type == batch_claim_type && (b.deleted ?? false) == false && (b.batch_lock ?? false) == false && (b.batch_status == "Pending" || b.batch_status == "" || b.batch_status == null) &&
        ((practype == "P" && (b.batch_type == "P" || b.batch_type == null)) ||
         (practype == "I" && b.batch_type == "I") ||
         (practype == "all" && (b.batch_type == "P" || b.batch_type == "I" || b.batch_type == null))))).Select(b => new SelectListViewModel()
         {
             Id = b.batch_id,
             Name = b.batch_id + "|" + b.batch_name
         }).ToList();
                    }
                    else
                    {
                        list = ctx.claim_batch.Where(b => (b.practice_id == practiceCode && b.batch_claim_type == batch_claim_type && (b.deleted ?? false) == false && b.provider_id == providerCode && (b.batch_lock ?? false) == false && (b.batch_status == "Pending" || b.batch_status == "" || b.batch_status == null) &&
        ((practype == "P" && (b.batch_type == "P" || b.batch_type == null)) ||
         (practype == "I" && b.batch_type == "I") ||
         (practype == "all" && (b.batch_type == "P" || b.batch_type == "I" || b.batch_type == null))))).Select(b => new SelectListViewModel()
         {
             Id = b.batch_id,
             Name = b.batch_id + "|" + b.batch_name
         }).ToList();
                    }
                    if (list != null)
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = list;
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        public ResponseModel GetSentBatchSelectList(string searchText, long practiceCode, long? providerCode)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SelectListViewModel> list;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    providerCode = providerCode == -1 ? null : providerCode;
                    if (providerCode == null || providerCode == 0)
                    {
                        list = ctx.claim_batch.Where(b => (b.practice_id == practiceCode && (b.deleted ?? false) == false && (b.batch_lock ?? false) == false && b.batch_status == "Sent") && (b.batch_name.Contains(searchText) ||
                       b.batch_status.Contains(searchText) || b.batch_type.Contains(searchText) || b.batch_id.ToString().Contains(searchText))).Select(b => new SelectListViewModel()
                       {
                           Id = b.batch_id,
                           Name = b.batch_id + "|" + b.batch_name
                       }).ToList();
                    }
                    else
                    {
                        list = ctx.claim_batch.Where(b => (b.practice_id == practiceCode && (b.deleted ?? false) == false && b.provider_id == providerCode && (b.batch_lock ?? false) == false && b.batch_status == "Sent") && (b.batch_name.Contains(searchText) ||
                        b.batch_status.Contains(searchText) || b.batch_type.Contains(searchText) || b.batch_id.ToString().Contains(searchText))).Select(b => new SelectListViewModel()
                        {
                            Id = b.batch_id,
                            Name = b.batch_id + "|" + b.batch_name
                        }).ToList();
                    }
                    if (list != null)
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = list;
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        public ResponseModel GetBatchesDetail(BatchListRequestViewModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            //List<SP_GetBatchDetail_Result> results;
            List<SP_GetBatchDetail_EDI_Result> results;
            //List<SP_GetBatchDetail_Paper_Result> results;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    //results = ctx.SP_GetBatchDetail(model.PracticeCode, model.ProviderCode).OrderByDescending(o => o.batch_id).ToList();
                    //if (results != null)
                    //{
                    //    responseModel.Status = "Success";
                    //    responseModel.Response = results;
                    //}
                    results = ctx.SP_GetBatchDetail_EDI(model.PracticeCode, model.ProviderCode, model.prac_type).OrderByDescending(o => o.batch_id).ToList();
                    //results = ctx.SP_GetBatchDetail_Paper(model.PracticeCode, model.ProviderCode).OrderByDescending(o => o.batch_id).ToList();
                    if (results != null)
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = results;
                    }
                    else
                    {
                        responseModel.Status = "No Record Found";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        public ResponseModel AddInBatch(AddInBatchRequestViewModel model, long userId)
        {

            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var claimsResponse = SearchClaim(new ClaimSearchViewModel()
                    {
                        DOSFrom = null,
                        DOSTo = null,
                        PatientAccount = new List<long>(),
                        PracticeCode = model.PracticeCode,
                        Provider = new List<long>(),
                        icd9 = null,
                        insurance = new List<long>(),
                        location = new List<long>(),
                        status = "unprocessed",
                        type = model.Type
                    });
                    List<SP_ClaimsSearch_EDI_Result> claims = new List<SP_ClaimsSearch_EDI_Result>();
                    //List<SP_ClaimsSearch_Result> claims = new List<SP_ClaimsSearch_Result>();
                    if (claimsResponse.Status == "Success")
                        claims = claimsResponse.Response;
                    for (int i = 0; i < model.ClaimIds.Length; i++)
                    {
                        var claimId = model.ClaimIds[i];
                        var claimBatch = (from c in ctx.Claims
                                          join cbd in ctx.claim_batch_detail on c.Claim_No equals cbd.claim_id
                                          join cb in ctx.claim_batch on cbd.batch_id equals cb.batch_id
                                          where c.Claim_No == claimId && (cbd.deleted ?? false) == false
                                          && cb.batch_status == "Pending"
                                          select cb).FirstOrDefault();

                        //var claimBatch = ctx.claim_batch_detail.FirstOrDefault(c => c.claim_id == claimId && c.practice_id == model.PracticeCode && (c.deleted ?? false) == false);
                        if (claimBatch != null)
                            continue;
                        var claimNo = model.ClaimIds[i];

                        var AllClaimRow = ctx.Claims.FirstOrDefault(c => c.Claim_No == claimNo);


                        var PSO = AllClaimRow;

                        var claimBatchDetail = new claim_batch_detail()
                        {
                            detail_id = Convert.ToInt64(ctx.SP_TableIdGenerator("detail_id").FirstOrDefault()),
                            claim_id = model.ClaimIds[i],
                            practice_id = model.PracticeCode,
                            batch_id = model.BatchId,
                            created_user = userId,
                            date_created = DateTime.Now,
                            //amount_due = claims.Where(c => c.Claim_No == model.ClaimIds[i]).Select(c => c.claim_total).FirstOrDefault(),
                            amount_due = PSO.Claim_Total,
                            Insurance_Id = model.ClaimInsuranceIds[i],
                            Pri_Status = PSO.Pri_Status,
                            Sec_Status = PSO.Sec_Status,
                            Oth_Status = PSO.Oth_Status,
                            Pat_Status = PSO.Pat_Status
                    };
                        ctx.claim_batch_detail.Add(claimBatchDetail);
                    }
                    if (ctx.SaveChanges() > 0)
                    {
                        responseModel.Status = "Success";
                    }
                    else
                    {
                        responseModel.Status = "Selected claims are already present in Claim Batch.";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        public ResponseModel LockBatch(LockBatchRequestViewModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    claim_batch batch = ctx.claim_batch.FirstOrDefault(c => c.batch_id == model.BatchId);
                    if (batch != null)
                    {
                        batch.batch_lock = true;
                        batch.modified_user = model.UserId;
                        batch.date_modified = DateTime.Now;
                        if (ctx.SaveChanges() > 0)
                        {
                            responseModel.Status = "Success";
                        }
                        else
                        {
                            responseModel.Status = "Error";

                        }
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        public ResponseModel UploadBatches(BatchUploadRequest model, long v)
        {
            long? lastPracticeCode = 0;
            //long lastBatchId = 0;
            long lastBatchId = model.BatcheIds[0];
            long lastClaimId = 0;
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    bool isAllClaimsSuccess = true;
                    var batches = (from cb in ctx.claim_batch
                                   join cbd in ctx.claim_batch_detail on cb.batch_id equals cbd.batch_id
                                   join c in ctx.Claims on cbd.claim_id equals c.Claim_No
                                   join p in ctx.Patients on c.Patient_Account equals p.Patient_Account
                                   where model.BatcheIds.Contains(cb.batch_id) && cb.batch_status.ToLower() == "pending"
                                   select new BatchUploadViewModel()
                                   {
                                       BatchId = cb.batch_id,
                                       ClaimId = c.Claim_No,
                                       PatientAccount = p.Patient_Account,
                                       PracticeCode = c.practice_code,
                                       DOS = c.DOS,
                                       PatientName = p.Last_Name + ", " + p.First_Name,
                                       Claim_type = c.Claim_Type,
                                       Submission_Type = cb.Submission_Type,
                                       Batch_claim_Type = cb.batch_claim_type,
                                       Pri_Status = c.Pri_Status,
                                       Sec_Status = c.Sec_Status
                                   }).GroupBy(b => b.BatchId).ToList();
                    //bool hasPaperSubmission = batches.Any(group => group.Any(b => b.Submission_Type.ToLower() == "paper"));
                    //if (hasPaperSubmission != null && hasPaperSubmission == true)
                    //{
                    //    responseModel.Status = "error";
                    //    responseModel.Response = "Paper batch can not be uploaded";
                    //    return responseModel;
                    //}

                    if (batches != null && batches.Count > 0)
                    {
                        foreach (var batch in batches)
                        {
                            bool batchHasError = false;
                            List<BatchClaimSubmissionResponse> responsedPerBatch = new List<BatchClaimSubmissionResponse>();
                            foreach (var claim in batch)
                            {
                                lastPracticeCode = claim.PracticeCode;
                                lastClaimId = claim.ClaimId;
                                if (!batchHasError)
                                {
                                    ResponseModel res = new ResponseModel();
                                    if ((claim.Claim_type == null || claim.Claim_type.ToUpper() == "P") && (claim.Batch_claim_Type == "P" || (claim.Pri_Status == "N" || claim.Pri_Status == "R" || claim.Pri_Status == "B")))
                                    {
                                        res = GenerateBatch_5010_P_P(Convert.ToInt64(claim.PracticeCode), claim.ClaimId);
                                    }

                                    else if ((claim.Claim_type == null || claim.Claim_type.ToUpper() == "P") && (claim.Batch_claim_Type == "S" || (claim.Sec_Status == "N" || claim.Sec_Status == "R" || claim.Sec_Status == "B")))
                                    {
                                        res = GenerateBatch_5010_P_S(Convert.ToInt64(claim.PracticeCode), claim.ClaimId);
                                    }

                                    else
                                    {
                                        res = GenerateBatch_For_Packet_837i_5010_I(Convert.ToInt64(claim.PracticeCode), claim.ClaimId);
                                    }
                                    if (res.Status == "Error")
                                    {
                                        isAllClaimsSuccess = false;
                                        batchHasError = true;
                                        AddUpdateClaimBatchError(claim.BatchId, claim.ClaimId, v, string.Join(";", res.Response), claim.PatientName, claim.PatientAccount, claim.DOS);

                                    }
                                    responsedPerBatch.Add(new BatchClaimSubmissionResponse() { ClaimId = claim.ClaimId, PracticeCode = claim.PracticeCode, response = res.Response, BatchId = claim.BatchId });
                                    //lastPracticeCode = claim.PracticeCode;
                                    //lastBatchId = claim.BatchId;
                                    //lastClaimId = claim.ClaimId;
                                }
                            }
                            if (!batchHasError)
                            {
                                // Update batch status
                                var batchToUpdate = ctx.claim_batch.Where(b => b.batch_id == batch.Key).FirstOrDefault();
                                batchToUpdate.date_uploaded = DateTime.Now;
                                batchToUpdate.batch_status = "Sent";
                                batchToUpdate.uploaded_user = v;
                                batchToUpdate.batch_lock = true;
                                batchToUpdate.Batch_Status999 = "Pending";

                                try
                                {
                                    var responses = responsedPerBatch.Select(r => r.response).ToList();
                                    string stringToWrite = string.Join("\n", responses.Select(r => r));
                                    if (!Directory.Exists(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["ClaimBatchSubmissionPath"])))
                                        Directory.CreateDirectory(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["ClaimBatchSubmissionPath"]));
                                    File.WriteAllText(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["ClaimBatchSubmissionPath"] + "/" + batchToUpdate.batch_name + ".txt"),
                                stringToWrite);
                                    batchToUpdate.file_path = batchToUpdate.batch_name + ".txt";
                                    batchToUpdate.file_generated = true;

                                    // Uploading File to FTP
                                    string fileUploadStatus = "success";
                                    if (!Debugger.IsAttached)
                                    {
                                        fileUploadStatus = UploadFileToFTP(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["ClaimBatchSubmissionPath"] + "/" + batchToUpdate.batch_name + ".txt"), (long)batchToUpdate.practice_id, v, "Upload Batch");
                                    }
                                    //To Update Status of Claim
                                    var claimsToUpdate = batch.Select(bb => bb.ClaimId).ToList<long>();
                                    if (claimsToUpdate.Count() > 0)
                                    {
                                        var claims = ctx.Claims.Where(c => claimsToUpdate.Contains(c.Claim_No));
                                        List<CLAIM_NOTES> cLAIM_NOTEs = new List<CLAIM_NOTES>();
                                        claims.ForEach(c =>
                                        {
                                            if (!string.IsNullOrEmpty(c.Pri_Status) && (c.Pri_Status.ToLower() == "n" || c.Pri_Status.ToLower() == "r"))
                                            {
                                                c.Pri_Status = "B";
                                                cLAIM_NOTEs.Add(new CLAIM_NOTES
                                                {
                                                    Claim_Notes_Id = Convert.ToInt64(ctx.SP_TableIdGenerator("Claim_Notes_Id").FirstOrDefault().ToString()),
                                                    Claim_No = c.Claim_No,
                                                    Note_Detail = $"Claim submitted to Primary Insurance",
                                                    Created_By = v,
                                                    Created_Date = DateTime.Now
                                                });
                                            }
                                            else if (!string.IsNullOrEmpty(c.Sec_Status) && (c.Sec_Status.ToLower() == "n" || c.Sec_Status.ToLower() == "r"))
                                            {
                                                c.Sec_Status = "B";
                                                cLAIM_NOTEs.Add(new CLAIM_NOTES
                                                {
                                                    Claim_Notes_Id = Convert.ToInt64(ctx.SP_TableIdGenerator("Claim_Notes_Id").FirstOrDefault().ToString()),
                                                    Claim_No = c.Claim_No,
                                                    Note_Detail = $"Claim submitted to Secondary Insurance",
                                                    Created_By = v,
                                                    Created_Date = DateTime.Now
                                                });
                                            }
                                            else if (!string.IsNullOrEmpty(c.Oth_Status) && (c.Oth_Status.ToLower() == "n" || c.Oth_Status.ToLower() == "r"))
                                            {
                                                c.Oth_Status = "B";
                                                cLAIM_NOTEs.Add(new CLAIM_NOTES
                                                {
                                                    Claim_Notes_Id = Convert.ToInt64(ctx.SP_TableIdGenerator("Claim_Notes_Id").FirstOrDefault().ToString()),
                                                    Claim_No = c.Claim_No,
                                                    Note_Detail = $"Claim submitted to Other Insurance",
                                                    Created_By = v,
                                                    Created_Date = DateTime.Now
                                                });
                                            }
                                        });
                                        ctx.CLAIM_NOTES.AddRange(cLAIM_NOTEs);
                                    }

                                    ctx.SaveChanges();
                                    if (fileUploadStatus == "error")
                                    {
                                        responseModel.Status = "error";
                                        responseModel.Response = "File generation success, but file uploaded has been failed.";
                                        return responseModel;
                                    }
                                }
                                catch (Exception)
                                {
                                    throw;
                                }
                            }
                        }
                        if (isAllClaimsSuccess)
                        {
                            responseModel.Response = new ExpandoObject();
                            responseModel.Response.Type = 1;
                            responseModel.Response.Message = "Batches has been uploaded successfully.";
                        }
                        else
                        {
                            responseModel.Response = new ExpandoObject();
                            responseModel.Response.Type = 2;
                            responseModel.Response.Message = "An errors occurred while uploading batches, please see \"Batch File Errors\" for error details.";
                        }

                    }
                    else
                    {
                        responseModel.Response = new ExpandoObject();
                        responseModel.Response.Type = 3;
                        responseModel.Response.Message = "No unprocessed batch has been found.";
                    }

                    responseModel.Status = "Success";
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = "error";

                responseModel.Response = ex.ToString();

                responseModel.Response = GetExceptionMessage(ex, lastClaimId);


                string ExceptionMessage = responseModel.Response = ex.ToString();
                string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
                string errorMessage = GetExceptionMessage(ex, lastClaimId);
                responseModel.Response = errorMessage;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO Claim_Batch_Exceptions 
                    (Exception,ErrorMessage, PracticeCode, Batch_ID,Claim_ID, CreatedBy, ModifiedBy) 
                    VALUES (@Exception,@ErrorMessage, @PracticeCode, @BatchId,@Claim_ID, @CreatedBy, @ModifiedBy)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //command.Parameters.AddWithValue("@Exception", ExceptionMessage);
                        //command.Parameters.AddWithValue("@ErrorMessage", errorMessage);
                        //command.Parameters.AddWithValue("@PracticeCode", lastPracticeCode);
                        //command.Parameters.AddWithValue("@BatchId", lastBatchId);
                        //command.Parameters.AddWithValue("@Claim_ID", lastClaimId);
                        //command.Parameters.AddWithValue("@CreatedBy", v);
                        //command.Parameters.AddWithValue("@ModifiedBy", v);

                        command.Parameters.AddWithValue("@Exception", ExceptionMessage ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ErrorMessage", errorMessage ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@PracticeCode", lastPracticeCode ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@BatchId", (object)lastBatchId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Claim_ID", (object)lastClaimId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedBy", (object)v ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ModifiedBy", (object)v ?? DBNull.Value);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

            }
            return responseModel;
        }


        private string GetExceptionMessage(Exception ex, long lastClaimId)
        {
            string message = ex.ToString();

            if (message.Contains("Renci.SshNet.Common.SshAuthenticationException") && message.Contains("password"))
            {
                return "Batch upload failed due to practice FTP password expired.";
            }
            else if (message.Contains("Renci.SshNet.Common.SshException") && message.Contains("60"))
            {
                return "Batch upload failed due to practice FTP Not Found.";
            }
            else if (message.Contains("System.Data.Entity.Core.EntityException") && message.Contains("database"))
            {
                return "Batch upload failed due to DB Level Issue retery again.";
            }
            else if (message.Contains("System.Net.Sockets.SocketException (0x80004005)") && message.Contains("period of time"))
            {
                return "Batch upload failed due to client connection failed.";
            }
            else if (message.Contains("System.InvalidOperationException") && message.Contains("Nullable object must have a value"))
            {
                return $"Batch upload failed due Rejection Type exists But Rejection Amount missing in claim number : {lastClaimId}.";
            }

            //else if (message.Contains("System.InvalidoperationException Nullable object must have a value at system") && message.Contains("Nullable object must have a value"))
            //{
            //    return "Batch upload failed due Rejection Type exists But Rejection Amount missing in claim number.";
            //}
            else if (message.Contains("System.NullReferenceException:object refrence not set to an instance of an oject") && message.Contains("NullReferenceException"))
            {
                return $"Batch upload failed due insrance not exist in insurance list table for claim : {lastClaimId}";
            }
            else if (message.Contains("System.IndexOutOfRangeException") && message.Contains("IndexOutOfRangeException"))
            {
                return $"Batch upload failed due to claim type not defined for Claim no: {lastClaimId}.";
                //return "Batch upload failed due to claim type not defined .";
            }
            //else if (message.Contains("System.IndexOutOfRangeException") && message.Contains("IndexOutOfRangeException"))
            //{
            //    return $"Batch upload failed due to claim type not defined for Claim no: {lastClaimId}.";
            //    //return "Batch upload failed due to claim type not defined .";
            //}

            // Add more known patterns as needed
            else
            {
                return "An unexpected error occurred during batch upload. Please contact support.";
            }
        }
        public ResponseModel GetBatchExceptions(BatchErrorsRequestModel model)
        {
            ResponseModel response = new ResponseModel();
            //List<SP_Search_Claim_Batch_Error_Result> list;
            List<SP_Claim_Batch_Exception_EDI_Result> list;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    //list = ctx.SP_Search_Claim_Batch_Error(model.practiceCode, model.providerCode == -1 ? null : model.providerCode, model.bactchId, model.dateFrom, model.dateTo).ToList();
                    list = ctx.SP_Claim_Batch_Exception_EDI(model.practiceCode, model.providerCode == -1 ? null : model.providerCode, model.bactchId, model.dateFrom, model.dateTo, model.batch_type ?? "P").ToList();
                    if (list != null)
                    {
                        response.Response = list;
                        response.Status = "Success";
                    }
                    else
                    {
                        response.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = ex.ToString();
            }
            return response;
        }

        public ResponseModel GetBatchFileErrors(BatchErrorsRequestModel model)
        {
            ResponseModel response = new ResponseModel();
            //List<SP_Search_Claim_Batch_Error_Result> list;
            List<SP_Search_Claim_Batch_Error_EDI_Result> list;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    //list = ctx.SP_Search_Claim_Batch_Error(model.practiceCode, model.providerCode == -1 ? null : model.providerCode, model.bactchId, model.dateFrom, model.dateTo).ToList();
                    list = ctx.SP_Search_Claim_Batch_Error_EDI(model.practiceCode, model.providerCode == -1 ? null : model.providerCode, model.bactchId, model.dateFrom, model.dateTo, model.batch_type ?? "P").ToList();
                    if (list != null)
                    {
                        response.Response = list;
                        response.Status = "Success";
                    }
                    else
                    {
                        response.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = ex.ToString();
            }
            return response;
        }
        public ResponseModel GetBatchesHistory(BatchesHistoryRequestModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            //List<sp_getBatchHistory_Result> list;
            List<sp_getBatchHistory_EDI_Result> list;
            try
            {
                using (var ctx = new NPMDBEntities())
                {

                    //list = ctx.sp_getBatchHistory(model.Practice_Code, model.Provider_Code == -1 ? null : model.Provider_Code, model.Date_From, model.Date_To, !String.IsNullOrEmpty(model.Date_Type) ? model.Date_Type : null).ToList();
                    list = ctx.sp_getBatchHistory_EDI(model.Practice_Code, model.Provider_Code == -1 ? null : model.Provider_Code, model.Date_From, model.Date_To, !String.IsNullOrEmpty(model.Date_Type) ? model.Date_Type : null , model.prac_type,model.Sub_type).ToList();
                    if (list != null)
                    {
                        responseModel.Response = list;
                        responseModel.Status = "Success";
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        public ResponseModel Get999Report(long practice_code)
        {
            ResponseModel responseModel = new ResponseModel();
            List<sp_get999Report_EDI_Result> list;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    list = ctx.sp_get999Report_EDI(practice_code).ToList();
                    if (list != null)
                    {
                        responseModel.Response = list;
                        responseModel.Status = "Success";
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        //public ResponseModel GetBatcheDetalis(long batchId)
        //{
        //    ResponseModel res = new ResponseModel();
        //    List<GetBatchClaimsDetails_Result> bcdList;
        //    using (var ctx = new NPMDBEntities())
        //    {
        //        try
        //        {
        //            bcdList = ctx.GetBatchClaimsDetails(batchId).ToList();

        //            //var batchDetClaimIdList = (from cl in ctx.Claims
        //            //                           join cbd in ctx.claim_batch_detail on cl.Claim_No equals cbd.claim_id
        //            //                           join ci in ctx.Claim_Insurance  on cbd.claim_id  equals ci.Claim_No
        //            //                           join i in ctx.Insurances  on ci.Insurance_Id equals i.Insurance_Id
        //            //                           join ip in ctx.Insurance_Payers  on i.InsPayer_Id equals ip.Inspayer_Id
        //            //                           where cbd.batch_id == batchId && ci.Claim_No == cbd.claim_id &&  ci.Insurance_Id == cbd.Insurance_Id
        //            //                           select new BatchClaimsDetail()
        //            //                           {
        //            //                               Claim_CSI_Status = cbd.Claim_CSI_Status,
        //            //                               CSI_Request = cbd.CSI_Request,
        //            //                               insurance_Name = ip.Inspayer_Description,
        //            //                               Claim_No = cl.Claim_No,
        //            //                               DOS = (DateTime)cl.DOS,
        //            //                               Billed_Amount = cbd.amount_due == null ? 0 : (decimal)cbd.amount_due,
        //            //                           }).Distinct().ToList();

        //            if (bcdList.Count > 0)
        //            {
        //                res.Status = "Success";
        //                res.Response = bcdList;
        //            }
        //            else
        //            {
        //                res.Status = "No Claims Found";
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            throw;
        //        }

        //    }
        //    return res;
        //}

        public ResponseModel GetBatcheDetalis(long batchId)
        {
            ResponseModel res = new ResponseModel();
            List<GetBatchClaimsDetails_Result> bcdList;
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    bcdList = ctx.GetBatchClaimsDetails(batchId).ToList();

                    //var batchDetClaimIdList = (from cl in ctx.Claims
                    //                           join cbd in ctx.claim_batch_detail on cl.Claim_No equals cbd.claim_id
                    //                           join ci in ctx.Claim_Insurance  on cbd.claim_id  equals ci.Claim_No
                    //                           join i in ctx.Insurances  on ci.Insurance_Id equals i.Insurance_Id
                    //                           join ip in ctx.Insurance_Payers  on i.InsPayer_Id equals ip.Inspayer_Id
                    //                           where cbd.batch_id == batchId && ci.Claim_No == cbd.claim_id &&  ci.Insurance_Id == cbd.Insurance_Id
                    //                           select new BatchClaimsDetail()
                    //                           {
                    //                               Claim_CSI_Status = cbd.Claim_CSI_Status,
                    //                               CSI_Request = cbd.CSI_Request,
                    //                               insurance_Name = ip.Inspayer_Description,
                    //                               Claim_No = cl.Claim_No,
                    //                               DOS = (DateTime)cl.DOS,
                    //                               Billed_Amount = cbd.amount_due == null ? 0 : (decimal)cbd.amount_due,
                    //                           }).Distinct().ToList();

                    if (bcdList.Count > 0)
                    {
                        string responseString2771 = bcdList.FirstOrDefault()?.Response_String_277;
                        if (bcdList.Count > 0)
                        {
                            foreach (var bcd in bcdList)
                            {
                                string responseString277 = bcd?.Response_String_277;

                                // Check if Response_String_277 is not null or empty
                                if (!string.IsNullOrEmpty(responseString277))
                                {

                                    var stcStatusCode = ExtractSTCCodeForStatus(responseString277);
                                    string stcStatus = string.Empty;
                                    if (!string.IsNullOrEmpty(stcStatusCode))
                                    {
                                        stcStatus = GetStatusFromDatabase(stcStatusCode);
                                    }
                                    string CptstcStatus = string.Empty;
                                    var stcDescriptions = new Dictionary<string, string>();

                                    var stcCodes = ExtractCptStcDetails(responseString277);
                                    foreach (var code in stcCodes)
                                    {
                                        var description = GetCptStatusFromDatabase(code);
                                        if (!string.IsNullOrEmpty(description))
                                        {
                                            stcDescriptions[code] = description;
                                        }
                                    }

                                    string stcStatusDescriptionOutput = string.Join(", ", stcDescriptions.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

                                    var StcCategoryDescription = new Dictionary<string, string>();
                                    var stcCode = ExtractCptStcCategoryDetails(responseString277);
                                    foreach (var code in stcCode)
                                    {
                                        var description = GetCptCategorycodeFromDatabase(code);
                                        if (!string.IsNullOrEmpty(description))
                                        {
                                            StcCategoryDescription[code] = description;
                                        }
                                    }
                                    res.STCStatus = stcStatus;

                                    //string stcStatusDescriptionOutput = string.Join(", ", stcDescriptions.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

                                    //var StcCategoryDescription = new Dictionary<string, string>();
                                    //var stcCodeClaimnumber = ExtractClaimNumber(responseString277);
                                    var stcCodeClaimnumber = ExtractClaimNumber(responseString277);

                                    // Assuming `stcStatus` is already set earlier
                                    res.STCStatus = stcStatus;

                                    // Loop through the extracted claim numbers (from the responseString277 or stcCodeClaimnumber)
                                    foreach (var claimNumber in stcCodeClaimnumber)
                                    {
                                        // Call ProcessEdiFileAndUpdateStatus with responseString277 and stcStatus
                                        // This will extract claim number from responseString277 and update the database
                                        ProcessEdiFileAndUpdateStatus(responseString277, stcStatus);

                                        // Optionally, if you want to track descriptions or other results, you could add them to StcCategoryDescription
                                        // For example, if you have a method that returns the description of the claim number

                                    }
                                }
                                else
                                {
                                }
                            }

                            res.Status = "Success";
                            res.Response = bcdList;
                        }


                        //if (!string.IsNullOrEmpty(responseString277))
                        //{

                        //    var stcStatusCode = ExtractSTCCodeForStatus(responseString277);



                        //    if (bcdList.Count > 0)
                        //    {
                        //        List<string> dtpDates = new List<string>();

                        //        var segments = responseString277.Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries);
                        //        foreach (var segment in segments)
                        //        {
                        //            if (segment.StartsWith("DTP*472*RD8*"))
                        //            {
                        //                var parts = segment.Split('*');
                        //                if (parts.Length >= 4)
                        //                {
                        //                    var datePart = parts[3];
                        //                    var date = datePart.Split('-')[0];
                        //                    dtpDates.Add(date);
                        //                }
                        //            }
                        //        }

                        //        string concatenatedDates = string.Join(", ", dtpDates);

                        //        string stcStatus = string.Empty;
                        //        if (!string.IsNullOrEmpty(stcStatusCode))
                        //        {
                        //            stcStatus = GetStatusFromDatabase(stcStatusCode);
                        //        }
                        //        string CptstcStatus = string.Empty;

                        //        var stcDescriptions = new Dictionary<string, string>();

                        //        var stcCodes = ExtractCptStcDetails(responseString277);
                        //        foreach (var code in stcCodes)
                        //        {
                        //            var description = GetCptStatusFromDatabase(code);
                        //            if (!string.IsNullOrEmpty(description))
                        //            {
                        //                stcDescriptions[code] = description;
                        //            }
                        //        }

                        //        string stcStatusDescriptionOutput = string.Join(", ", stcDescriptions.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                        //        var StcCategoryDescription = new Dictionary<string, string>();
                        //        var stcCode = ExtractCptStcCategoryDetails(responseString277);
                        //        foreach (var code in stcCode)
                        //        {
                        //            var description = GetCptCategorycodeFromDatabase(code);
                        //            if (!string.IsNullOrEmpty(description))
                        //            {
                        //                StcCategoryDescription[code] = description;
                        //            }
                        //        }
                        //        //string stcCateogoryDescriptionOutput = string.Join(", ", StcCategoryDescription.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                        //        //var responseTrns = ExtractTRNs(response);
                        //        //string selectedTrn = responseTrns.FirstOrDefault();

                        //        if (!string.IsNullOrEmpty(stcStatusCode))
                        //        {
                        //            stcStatus = GetStatusFromDatabase(stcStatusCode);
                        //        }




                        //        res.STCStatus = stcStatus;

                        //    }
                        //    else
                        //    {
                        //        res.Status = "Success";
                        //        res.Response = bcdList;
                        //        res.STCStatus = stcStatusCode;
                        //    }


                        //}
                        else
                        {
                            res.Status = "No CSI Claims Response Found";
                        }
                        res.Status = "Success";
                        res.Response = bcdList;
                        //res.STCStatus = stcStatusCode;
                    }
                    else
                    {
                        res.Status = "No Claims Found";
                    }
                }
                catch (Exception)
                {
                    throw;
                }

            }
            return res;
        }
        public ResponseModel CheckMedicareClaim(long? claimId, long insurance_id)
        {
            ResponseModel res = new ResponseModel();
            List<GetCSI_BatchDetails_Result> bcdList;
            using (var ctx = new NPMDBEntities())
            {
                res.Response = ctx.USP_CheckMedicareClaim(claimId, insurance_id).FirstOrDefault();
            }
            if (res.Response != null)
            {
                res.Status = "Success";
            }
            else
            {
                res.Status = "Error";
            }

                return res;
        }
        public ResponseModel GetCSIBatcheResponseDetalis(long batch_id, long claimId)
        {
            ResponseModel res = new ResponseModel();
            List<GetCSI_BatchDetails_Result> bcdList;
            using (var ctx = new NPMDBEntities())
            {
                res.Response = ctx.CSI_Batch.Where(r => r.Claim_Number == claimId && r.Batch_Id == batch_id).Select(r => new
                {
                    r.Response_String_277,
                    r.Batch_Status999,
                    r.Status_277
                })
    .FirstOrDefault();
            }
                res.Status = "Success";

            return res;
        }
        public ResponseModel GetCSIBatcheDetalisStatus(long claimId)
        {
            ResponseModel res = new ResponseModel();
            List<GetCSI_BatchDetails_Result> bcdList;
            using (var ctx = new NPMDBEntities())
            {
                res.Response = ctx.CSI_Batch.Where(r => r.Claim_Number == claimId).Select(r => new
                {
                    r.Response_String_277,
                    r.Batch_Status999,
                    r.Status_277
                })
    .FirstOrDefault();
            }
            if (res.Response != null)
            {
                res.Status = "Success";
            }
            else
            {
                res.Status = "Error";
            }

            return res;
        }
        public ResponseModel GetCSIBatcheDetalis(int batchId, string claimId)
        {
            ResponseModel res = new ResponseModel();
            List<GetCSI_BatchDetails_Result> bcdList;
            using (var ctx = new NPMDBEntities())
            {
                try
                {

                    bcdList = ctx.GetCSI_BatchDetails(batchId, claimId).ToList();

                    string responseString277 = bcdList.FirstOrDefault()?.Response_String_277;

                    if (!string.IsNullOrEmpty(responseString277))
                    {
                        var stcCodeCategory = ExtractSTCCodeCategory(responseString277);
                        var stcDescription = ExtractSTCCode(responseString277);
                        var stcStatusCode = ExtractSTCCodeForStatus(responseString277);
                        var CptstcStatusCode = ExtractCptStcDetails(responseString277);


                        if (bcdList.Count > 0)
                        {
                            List<string> dtpDates = new List<string>();

                            var segments = responseString277.Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var segment in segments)
                            {
                                if (segment.StartsWith("DTP*472*RD8*"))
                                {
                                    var parts = segment.Split('*');
                                    if (parts.Length >= 4)
                                    {
                                        var datePart = parts[3];
                                        var date = datePart.Split('-')[0];
                                        dtpDates.Add(date);
                                    }
                                }
                            }

                            string concatenatedDates = string.Join(", ", dtpDates);

                            string stcStatusCategoryDescription = string.Empty;
                            if (!string.IsNullOrEmpty(stcCodeCategory))
                            {
                                stcStatusCategoryDescription = GetDescriptionFromDatabaseForCategory(stcCodeCategory);
                            }

                            if (!string.IsNullOrEmpty(stcCodeCategory))
                            {
                                stcStatusCategoryDescription = GetDescriptionFromDatabaseForCategory(stcCodeCategory);
                            }
                            string stcStatusDescription = string.Empty;
                            if (!string.IsNullOrEmpty(stcDescription))
                            {
                                stcStatusDescription = GetDescriptionFromDatabase(stcDescription);
                            }
                            string stcStatus = string.Empty;
                            if (!string.IsNullOrEmpty(stcStatusCode))
                            {
                                stcStatus = GetStatusFromDatabase(stcStatusCode);
                            }
                            string CptstcStatus = string.Empty;

                            var stcDescriptions = new Dictionary<string, string>();

                            var stcCodes = ExtractCptStcDetails(responseString277);
                            foreach (var code in stcCodes)
                            {
                                var description = GetCptStatusFromDatabase(code);
                                if (!string.IsNullOrEmpty(description))
                                {
                                    stcDescriptions[code] = description;
                                }
                            }

                            string stcStatusDescriptionOutput = string.Join(", ", stcDescriptions.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                            var StcCategoryDescription = new Dictionary<string, string>();
                            var stcCode = ExtractCptStcCategoryDetails(responseString277);
                            foreach (var code in stcCode)
                            {
                                var description = GetCptCategorycodeFromDatabase(code);
                                if (!string.IsNullOrEmpty(description))
                                {
                                    StcCategoryDescription[code] = description;
                                }
                            }
                            //string stcCateogoryDescriptionOutput = string.Join(", ", StcCategoryDescription.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                            //var responseTrns = ExtractTRNs(response);
                            //string selectedTrn = responseTrns.FirstOrDefault();

                            if (!string.IsNullOrEmpty(stcStatusCode))
                            {
                                stcStatus = GetStatusFromDatabase(stcStatusCode);
                            }



                            res.Status = "Success";
                            res.Response = bcdList;
                            res.AdditionalInfo = responseString277; // Assign it to a new variable in your result object
                            //res.stcCodeCategory = stcCodeCategory = ExtractSTCCodeCategory(responseString277);
                            //res.stcDescription = stcStatusCategoryDescription;
                            //res.stcStatusCode = ExtractSTCCodeForStatus(responseString277);
                            //res.CptstcStatusCode = ExtractSTCCodeForStatus(responseString277);
                            res.STCStatus = stcStatus;
                            res.STCDescription = stcStatusDescription;
                            res.stcStatusCategoryDescription = stcStatusCategoryDescription;
                            res.StcStatusDescription = stcStatusDescriptionOutput;
                            res.DTPDates = concatenatedDates;
                            //res.DTPDates = concatenatedDates,
                            //res.StcCategoryDescription = stcCateogoryDescriptionOutput
                        }
                        else
                        {
                            res.Status = "Success";
                            res.Response = bcdList;
                        }


                    }
                    else
                    {
                        res.Status = "No CSI Claims Response Found";
                    }
                }
                catch (Exception)
                {
                    throw;
                }

            }

            return res;
        }
        #region Code For Descerption code
        private string GetDescriptionFromDatabase(string code)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            string description = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT Description FROM Packet277CA_ClaimStatusCategoryCodes WHERE Code = @Code";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Code", code);
                    connection.Open();

                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        description = result.ToString();
                    }
                }
            }

            return description;
        }
        private string GetStatusFromDatabase(string code)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            string description = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT Internal_Status FROM Packet277CA_ClaimStatusCategoryCodes WHERE Code = @Code";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Code", code);
                    connection.Open();

                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        description = result.ToString();
                    }
                }
            }

            return description;
        }
        private string ExtractSTCCode(string responseString277)
        {
            var lines = responseString277.Split('~');

            foreach (var line in lines)
            {
                if (line.StartsWith("STC*"))
                {
                    var segments = line.Split('*');
                    if (segments.Length > 1)
                    {
                        var stcParts = segments[1].Split(':');
                        if (stcParts.Length > 1)
                        {
                            return stcParts[0];
                        }
                    }
                }
            }

            return null;
        }
        private string ExtractSTCCodeForStatus(string responseString277)
        {
            var lines = responseString277.Split('~');

            foreach (var line in lines)
            {
                if (line.StartsWith("STC*"))
                {
                    var segments = line.Split('*');
                    if (segments.Length > 1)
                    {
                        var stcParts = segments[1].Split(':');
                        if (stcParts.Length > 1)
                        {
                            return stcParts[0];
                        }
                    }
                }
            }

            return null;
        }
        private string ExtractSTCCodeCategory(string responseString277)
        {
            var lines = responseString277.Split('~');

            foreach (var line in lines)
            {
                if (line.StartsWith("STC*"))
                {
                    var segments = line.Split('*');
                    if (segments.Length > 2)
                    {
                        var stcParts = segments[1].Split(':');
                        return stcParts[1];
                    }
                }
            }

            return null;
        }
        private List<string> ExtractCptStcDetails(string responseString277)
        {
            var stcCodes = new List<string>();
            var segments = responseString277.Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i].StartsWith("STC"))
                {
                    var stcSegment = segments[i];
                    var stcParts = stcSegment.Split('*');
                    if (stcParts.Length > 1)
                    {
                        var code = stcParts[1].Split(':')[0];
                        if (!stcCodes.Contains(code))
                        {
                            stcCodes.Add(code);
                        }
                    }
                }
            }

            return stcCodes;
        }
        private string GetDescriptionFromDatabaseForCategory(string code)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            string description = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT Description FROM Packet277CA_ClaimStatusCodes WHERE Code = @Code";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Code", code);
                    connection.Open();

                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        description = result.ToString();
                    }
                }
            }

            return description;
        }
        private string GetCptStatusFromDatabase(string code)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            string description = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT Description FROM Packet277CA_ClaimStatusCategoryCodes WHERE Code = @Code";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Code", code);
                    connection.Open();

                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        description = result.ToString();
                    }
                }
            }

            return description;
        }
        private List<string> ExtractCptStcCategoryDetails(string responseString277)
        {
            var stcCode = new List<string>();
            var segments = responseString277.Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i].StartsWith("STC"))
                {
                    var stcSegment = segments[i];
                    var stcParts = stcSegment.Split('*');
                    if (stcParts.Length > 1)
                    {
                        var code = stcParts[1].Split(':')[1];
                        if (!stcCode.Contains(code))
                        {
                            stcCode.Add(code);
                        }
                    }
                }
            }

            return stcCode;
        }
        private string GetCptCategorycodeFromDatabase(string code)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            string description = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT Description FROM Packet277CA_ClaimStatusCodes WHERE Code = @Code";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Code", code);
                    connection.Open();

                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        description = result.ToString();
                    }
                }
            }

            return description;
        }
        //private List<string> ExtractTRNs(COREEnvelopeRealTimeResponse response)
        //{
        //    string responseString = response.responseString277;
        //    List<string> trns = new List<string>();
        //    var pattern = @"TRN\*2\*(\d+)";
        //    var matches = Regex.Matches(responseString, pattern);

        //    foreach (System.Text.RegularExpressions.Match match in matches)
        //    {
        //        if (match.Groups.Count > 1)
        //        {
        //            trns.Add(match.Groups[1].Value);
        //        }
        //    }

        //    return trns;
        //}
        #endregion Code For Descerption code

        public string UploadFileToFTP(string source, long practiceCode, long userId, string caller = "")
        {
            try
            {
                NPMLogger.GetInstance().Info($"Upload Batch File to FTP Called by user '{userId}' from {caller}");
                var practInfo = _practiceService.GetPracticeFTPInfo(practiceCode, FTPType.EDI);
                if (practInfo != null)
                {
                    using (SftpClient client = new SftpClient(practInfo.Host, practInfo.Port, practInfo.Username, practInfo.Password))
                    {
                        client.Connect();
                        NPMLogger.GetInstance().Info($"Practice '{practiceCode}' connection successs");
                        if (client.IsConnected)
                        {
                            client.ChangeDirectory(practInfo.Destination);
                            using (FileStream fs = new FileStream(source, FileMode.Open))
                            {
                                client.BufferSize = 4 * 1024;
                                client.UploadFile(fs, Path.GetFileName(source));
                                NPMLogger.GetInstance().Info($"{source} File uploaded to Practice {practiceCode} FTP by user {userId} from {caller}");
                            }
                        }
                        else
                        {
                            NPMLogger.GetInstance().Info($"Connection failed/lost to FTP of '{practiceCode}'");
                        }
                        return "success";
                    }
                }
                else
                {
                    NPMLogger.GetInstance().Info($"FTP Information not found in database for practice {practiceCode}");
                    return "error";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ResponseModel GetBatchFilePath(long batchId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var batch = ctx.claim_batch.FirstOrDefault(b => b.batch_id == batchId);
                    if (batch != null && !string.IsNullOrEmpty(batch.file_path))
                    {
                        response.Status = "success";
                        response.Response = batch.file_path;
                    }
                    else
                    {
                        response.Status = "error";
                        response.Response = "No batch or batch file found";
                    }
                    return response;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void AddUpdateClaimBatchError(long batchId, long claimNo, long userId, string errorResponse, string patientName, long? patientAccount, DateTime? Dos)
        {
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var claimBatchError = ctx.claim_batch_error.FirstOrDefault(cbe => cbe.batch_id == batchId && cbe.claim_id == claimNo);
                    if (claimBatchError != null)
                    {
                        claimBatchError.dos = Dos;
                        claimBatchError.error = errorResponse;
                        ctx.Entry(claimBatchError).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        claim_batch_error error = new claim_batch_error()
                        {
                            id = Convert.ToInt64(ctx.SP_TableIdGenerator("claim_batch_error_id").FirstOrDefault()),
                            batch_id = batchId,
                            claim_id = claimNo,
                            created_user = userId,
                            date_created = DateTime.Now,
                            deleted = false,
                            dos = Dos,
                            error = errorResponse,
                            patient_id = patientAccount,
                            patient_name = patientName
                        };
                        ctx.claim_batch_error.Add(error);
                    }
                    ctx.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ResponseModel RegenerateBatchFile(RegenerateBatchFileRequestModel model, long userId)
        {
            // error codes
            // success : claims processing completed
            // error : Exception
            // 1 : Some claims has errors (Get user confirmation to process only perfect claims)
            // 2 : All claims has errors (Can't regenerate and upload batch file)
            // 3 : Batch Can't be regenerated and uploaded, batch has no valid claim
            // 4 : Claims has errors while file generation
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    bool isAllClaimsSuccess = true;
                    IDictionary<long, string> errors = new Dictionary<long, string>();
                    var claims = ctx.GetBatchClaims(model.Batch_Id).ToList();
                    // Check if any claim errors
                    claims.ForEach(claim =>
                    {
                        // claim is deleted
                        if (claim.Deleted)
                        {
                            errors.Add(claim.Claim_No, "Claim is deleted.");
                        }
                        // Amount due is less than zero
                        else if (claim.Amt_Due <= 0)
                        {
                            errors.Add(claim.Claim_No, "Amount due is 0.");
                        }
                        else if (claim.Is_Payment_Posted == 1)
                        {
                            errors.Add(claim.Claim_No, "Payment posted.");
                        }
                    });
                    if (!model.Confirmation && errors.Count() > 0 && errors.Count() >= claims.Count())
                    {
                        responseModel.Status = "2";
                        responseModel.Response = errors;
                    }
                    else if (!model.Confirmation && errors.Count() > 0)
                    {
                        responseModel.Status = "1";
                        responseModel.Response = errors;
                    }
                    else if (model.Confirmation || errors.Count() == 0)
                    {
                        // Process claims
                        var claimsToProcess = claims.Where(c => c.Amt_Due > 0 && c.Deleted == false && c.Is_Payment_Posted == 0).ToList();
                        if (claimsToProcess.Count() == 0)
                        {
                            responseModel.Status = "3";
                        }
                        else
                        {
                            bool batchHasError = false;
                            List<BatchClaimSubmissionResponse> responsedPerBatch = new List<BatchClaimSubmissionResponse>();
                            foreach (var claim in claimsToProcess)
                            {
                                if (!batchHasError)
                                {
                                    ResponseModel res = new ResponseModel();
                                    var b_Type = ctx.claim_batch.Where(cb => cb.batch_id == model.Batch_Id)
                                        .Select(cb => new
                                        {
                                            BatchType = cb.batch_type,
                                            BatchClaimType = cb.batch_claim_type
                                        })
                                        .FirstOrDefault();

                                    var submitedto = ctx.claim_batch_detail.Where(cbd => cbd.batch_id == model.Batch_Id && cbd.claim_id == claim.Claim_No)
                                        .Select(cbd => new
                                        {
                                            Pri_Status = cbd.Pri_Status,
                                            Sec_Status = cbd.Sec_Status,
                                        })
                                        .FirstOrDefault();

                                    if ((b_Type.BatchType == "P" || b_Type.BatchType == null) && (b_Type.BatchClaimType == "P" || (submitedto.Pri_Status == "N" || submitedto.Pri_Status == "R" || submitedto.Pri_Status == "B")))
                                    {
                                        res = GenerateBatch_5010_P_P(Convert.ToInt64(model.Practice_Code), claim.Claim_No);
                                    }
                                   else if ((b_Type.BatchType == "P" || b_Type.BatchType == null) && (b_Type.BatchClaimType == "S" || (submitedto.Pri_Status == "N" || submitedto.Pri_Status == "R" || submitedto.Pri_Status == "B")))
                                    {
                                        res = GenerateBatch_5010_P_S(Convert.ToInt64(model.Practice_Code), claim.Claim_No);
                                    }
                                    else
                                    {
                                        res = GenerateBatch_For_Packet_837i_5010_I(Convert.ToInt64(model.Practice_Code), claim.Claim_No);
                                    }
                                    if (res.Status == "Error")
                                    {
                                        isAllClaimsSuccess = false;
                                        batchHasError = true;
                                        AddUpdateClaimBatchError(model.Batch_Id, claim.Claim_No, userId, string.Join(";", res.Response), claim.patient_name, claim.Patient_Account, claim.DOS);
                                    }
                                    responsedPerBatch.Add(new BatchClaimSubmissionResponse()
                                    {
                                        ClaimId = claim.Claim_No,
                                        PracticeCode = model.Practice_Code,
                                        response = res.Response,
                                        BatchId = model.Batch_Id
                                    });
                                }
                            }
                            if (!batchHasError)
                            {
                                // Update batch status
                                var batchToUpdate = ctx.claim_batch.Where(b => b.batch_id == model.Batch_Id).FirstOrDefault();
                                batchToUpdate.date_uploaded = DateTime.Now;
                                batchToUpdate.batch_status = "Sent";
                                batchToUpdate.uploaded_user = userId;
                                batchToUpdate.batch_lock = true;
                                try
                                {
                                    var responses = responsedPerBatch.Select(r => r.response).ToList();
                                    string stringToWrite = string.Join("\n", responses.Select(r => r));
                                    if (!Directory.Exists(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["ClaimBatchSubmissionPath"])))
                                        Directory.CreateDirectory(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["ClaimBatchSubmissionPath"]));
                                    File.WriteAllText(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["ClaimBatchSubmissionPath"] + "/" + batchToUpdate.batch_name + ".txt"),
                                stringToWrite);
                                    batchToUpdate.file_path = batchToUpdate.batch_name + ".txt";
                                    batchToUpdate.file_generated = true;
                                    ctx.SaveChanges();

                                    //Uploading File to FTP
                                    string fileUploadStatus = "success";
                                    if (!Debugger.IsAttached)
                                    {
                                        fileUploadStatus = UploadFileToFTP(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["ClaimBatchSubmissionPath"] + "/" + batchToUpdate.batch_name + ".txt"), model.Practice_Code, userId, "Regenerate Batch File");
                                    }
                                    if (fileUploadStatus == "error")
                                    {
                                        responseModel.Status = "error";
                                        responseModel.Response = "File generation success, but file uploaded has been failed.";
                                        return responseModel;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    responseModel.Status = "error";
                                    responseModel.Response = ex.ToString();
                                    return responseModel;
                                }
                            }
                        }
                        if (isAllClaimsSuccess)
                        {
                            responseModel.Status = "success";
                            responseModel.Response = "Batches has been uploaded successfully.";
                        }
                        else
                        {
                            responseModel.Status = "4";
                            responseModel.Response = "An errors occurred while uploading batches, please see \"Batch File Errors\" for error details.";
                        }
                    }
                }
                // Some claims has errors, but user confirms to regenerate and upload
                return responseModel;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public ResponseModel Read()
        {
            try
            {
                var ediStream = File.OpenRead(HostingEnvironment.MapPath($"~/DocumentDirectory/payment.txt"));
                List<IEdiItem> ediItems;
                using (var ediReader = new X12Reader(ediStream, "EdiFabric.Examples.X12.Templates.V5010.NoValidation"))
                    ediItems = ediReader.ReadToEnd().ToList();
                var transactions = ediItems.OfType<TS835>();
                List<era> eras = new List<era>();
                foreach (var transaction in transactions)
                {
                    if (transaction.HasErrors)
                    {
                        var errors = transaction.ErrorContext.Flatten();
                    }
                    else
                    {
                        var _era = new era()
                        {
                            // transaction
                            transaction_handling_code = transaction.BPR_FinancialInformation.TransactionHandlingCode_01,
                            check_amount = Convert.ToDecimal(transaction.BPR_FinancialInformation.TotalPremiumPaymentAmount_02),
                            check_date = new DateTime(long.Parse(transaction.BPR_FinancialInformation.CheckIssueorEFTEffectiveDate_16)),
                            check_number = transaction.TRN_ReassociationTraceNumber.CurrentTransactionTraceNumber_02,
                            production_date = new DateTime(long.Parse(transaction.DTM_ProductionDate.Date_02)),
                            // Payer
                            payer_identifier_qualifier = transaction.AllN1.Loop1000A.N1_PayerIdentification.EntityIdentifierCode_01,
                            payer_identifier = transaction.TRN_ReassociationTraceNumber.OriginatingCompanyIdentifier_03,
                            //payer_contact_information =
                            payer_name = transaction.AllN1.Loop1000A.N1_PayerIdentification.PremiumPayerName_02,
                            payer_address = transaction.AllN1.Loop1000A.N3_PayerAddress.ResponseContactAddressLine_01,
                            payer_state = transaction.AllN1.Loop1000A.N4_PayerCity_State_ZIPCode.AdditionalPatientInformationContactStateCode_02,
                            payer_city = transaction.AllN1.Loop1000A.N4_PayerCity_State_ZIPCode.AdditionalPatientInformationContactCityName_01,
                            payer_zip = transaction.AllN1.Loop1000A.N4_PayerCity_State_ZIPCode.AdditionalPatientInformationContactPostalZoneorZIPCode_03,
                            //Payee
                            payee_name = transaction.AllN1.Loop1000B.N1_PayeeIdentification.PremiumPayerName_02,
                            payee_identifier_code_qualifier = transaction.AllN1.Loop1000B.N1_PayeeIdentification.IdentificationCodeQualifier_03,
                            payee_identifier_code = transaction.AllN1.Loop1000B.N1_PayeeIdentification.EntityIdentifierCode_01,
                            payee_address = transaction.AllN1.Loop1000B.N3_PayeeAddress.ResponseContactAddressLine_01 + ' ' + transaction.AllN1.Loop1000B.N3_PayeeAddress.ResponseContactAddressLine_02,
                            payee_city = transaction.AllN1.Loop1000B.N4_PayeeCity_State_ZIPCode.AdditionalPatientInformationContactCityName_01,
                            payee_state = transaction.AllN1.Loop1000B.N4_PayeeCity_State_ZIPCode.AdditionalPatientInformationContactStateCode_02,
                            payee_zip = transaction.AllN1.Loop1000B.N4_PayeeCity_State_ZIPCode.AdditionalPatientInformationContactPostalZoneorZIPCode_03,
                            additional_payee_identifier_code_qualifier = transaction.AllN1.Loop1000B.REF_PayeeAdditionalIdentification[0].ReferenceIdentificationQualifier_01,
                            additional_payee_identifier_code = transaction.AllN1.Loop1000B.REF_PayeeAdditionalIdentification[0].MemberGrouporPolicyNumber_02,
                            // Provider
                            //provider_summary=
                            payer_business_contact_information = JsonConvert.SerializeObject(transaction.AllN1.Loop1000A.AllPER.PER_PayerBusinessContactInformation),
                            payer_technical_contact_information = JsonConvert.SerializeObject(transaction.AllN1.Loop1000A.AllPER.PER_PayerTechnicalContactInformation),

                        };
                        var _era_claim = new era_claim()
                        {
                            //        //era_claim_id= system generated
                            //        //era_id= foreign key from era
                            //claim_id=
                            //        //patient_id
                            //        //claim_billed_amount
                            //        //claim_paid_amount
                            //        //patient_responsibility
                            //        //claim_filing_indicator_code
                            //        //payer_claim_control_number
                            //        //claim_adj_amount
                            //        //claim_adj_codes
                            //        //claim_remark_codes
                            //        //patient_fname
                            //        //patient_lname
                            //        //patient_mname
                            //        //patient_identifier_qualfier
                            //        //patient_identifier
                            //        //subscriber_entity_type
                            //        //subscriber_identifier_qualfier
                            //        //subscriber_identifier
                            //        //subscriber_fname
                            //        //subscriber_lname
                            //        //subscriber_mname
                            //        //rendering_provider_lname
                            //        //rendering_provider_fname
                            //        //rendering_provider_identifier_qualifier
                            //        //rendering_provider_identifier
                            //        //claim_statement_period_start
                            //        //claim_statemnent_period_end
                            //        //coverage_amount
                            //        //claim_interest
                            //        //non_convered_estimated_NE
                            //        //claim_status_code
                            //        //claim_supplemental_information_quantity_qualifier
                            //        //claim_supplemental_information_quantity
                            //        //patient_responsibility_reason_code
                            //        //practice_id
                            //        //mapped_by
                            //        //date_mapped
                            //        //posted
                            //        //posted_by
                            //        //date_posted
                            //        //created_user
                            //        //client_date_created
                            //        //modified_user
                            //        //client_date_modified
                            //        //date_created
                            //        //date_modified
                            //        //system_ip
                            //        //deleted
                            //        //corrected_insured_entity_type
                            //        //corrected_insured_lname
                            //        //corrected_insured_fname
                            //        //corrected_insured_mname
                            //        //corrected_insured_identifier_qualifier
                            //        //corrected_insured_identifier
                            //        //mapped_insurance_id
                            //        //crossover_carrier_name

                        };
                    }
                }
                return new ResponseModel()
                {
                    Status = "sucess",
                    Response = transactions
                };
            }
            catch (Exception error)
            {
                return new ResponseModel()
                {
                    Status = "error",
                    Response = error?.Message
                };
            }

        }
        public ResponseModel SearchERA(ERASearchRequestModel model)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                model.checkNo = string.IsNullOrEmpty(model.checkNo) || string.IsNullOrWhiteSpace(model.checkNo) ? null : model.checkNo.Trim();
                model.checkAmount = string.IsNullOrEmpty(model.checkAmount) || string.IsNullOrWhiteSpace(model.checkAmount) ? null : model.checkAmount.Trim();
                model.dateFrom = string.IsNullOrEmpty(model.dateFrom) || string.IsNullOrWhiteSpace(model.dateFrom) ? null : model.dateFrom.Trim();
                model.dateTo = string.IsNullOrEmpty(model.dateTo) || string.IsNullOrWhiteSpace(model.dateTo) ? null : model.dateTo.Trim();
                model.patientAccount = string.IsNullOrEmpty(model.patientAccount) || string.IsNullOrWhiteSpace(model.patientAccount) ? null : model.patientAccount.Trim();
                model.icnNo = string.IsNullOrEmpty(model.icnNo) || string.IsNullOrWhiteSpace(model.icnNo) ? null : model.icnNo;
                model.status = !string.IsNullOrEmpty(model.status) && model.status.ToLower() == "all" ? null : model.status;
                model.dateType = !string.IsNullOrEmpty(model.dateType) && !string.IsNullOrWhiteSpace(model.dateType) ? model.dateType.ToUpper() : null;
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.SP_ERASEARCH(null, model.checkNo, model.checkAmount, model.dateFrom, model.dateTo, model.patientAccount, model.icnNo, model.status, model.dateType, model.practiceCode).ToList();
                    res.Status = "success";
                    res.Response = results;
                }
            }
            catch (Exception e)
            {
                res.Status = "error";
                res.Response = e.Message;
            }
            return res;
        }
        public ResponseModel EraSummary(EraSummaryRequest model)
        {
            ResponseModel res = new ResponseModel();
            res.Response = new ExpandoObject();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var eraDetails = ctx.SP_ERASEARCH(model.eraId, null, null, null, null, null, null, null, null, null).ToList();
                    var eraClaims_sp_result = ctx.SP_ERACLAIMDETAIL(null, model.eraId, null, null, null).ToList();
                    var glossary = ctx.SP_ERAADJCODEGLOSSARY(model.eraId).ToList();
                    List<object> claimsList = new List<object>();
                    dynamic claim = "";
                    if (eraDetails != null && eraClaims_sp_result != null)
                    {

                        var claimDetails = eraClaims_sp_result.GroupBy(c => c.PATIENTACCOUNTNUMBER).Select(group => new
                        {
                            claims = group.Select(v =>
                            {
                                double DEDCUTIBLEAMT = 0;
                                double COINSAMT = 0;
                                double COPAYAMT = 0;
                                switch (v.PRTYPE)
                                {
                                    case "1":
                                        DEDCUTIBLEAMT = double.Parse(v.PATIENTRESPONSIBILITY);
                                        break;
                                    case "2":
                                        COINSAMT = double.Parse(v.PATIENTRESPONSIBILITY);
                                        break;
                                    case "3":
                                        COPAYAMT = double.Parse(v.PATIENTRESPONSIBILITY);
                                        break;
                                }

                                switch (v.PRTYPE1)
                                {
                                    case "1":
                                        DEDCUTIBLEAMT = double.Parse(v.PRESAMT1);
                                        break;
                                    case "2":
                                        COINSAMT = double.Parse(v.PRESAMT1);
                                        break;
                                    case "3":
                                        COPAYAMT = double.Parse(v.PRESAMT1);
                                        break;
                                }

                                switch (v.PRTYPE2)
                                {
                                    case "1":
                                        DEDCUTIBLEAMT = double.Parse(v.PRESAMT2);
                                        break;
                                    case "2":
                                        COINSAMT = double.Parse(v.PRESAMT2);
                                        break;
                                    case "3":
                                        COPAYAMT = double.Parse(v.PRESAMT2);
                                        break;
                                }

                                var PATIENTRESPONSIBILITYREASONCODE = "";
                                var PRTYPE = "";
                                var PRCode = "";

                                double PATIENTRESPONSIBILITY = 0;

                                var ADJCODE1 = "";
                                var ADJCODE2 = "";
                                string ADJAMT1 = "";

                                if (v.ADJCODE1 == "PR")
                                {
                                    PATIENTRESPONSIBILITYREASONCODE = v.ADJCODE1 + '-' + v.ADJCODE2;
                                    PRCode = v.ADJCODE2;
                                    switch (v.ADJCODE2)
                                    {
                                        case "1":
                                            PRTYPE = "DEDCUTIBLE";
                                            break;
                                        case "2":
                                            PRTYPE = "COINS";
                                            break;
                                        case "3":
                                            PRTYPE = "COPAY";
                                            break;
                                    }
                                    PATIENTRESPONSIBILITY = double.Parse(v.ADJAMT1);
                                }
                                else
                                {
                                    if (v.ADJCODE3 != null)
                                    {
                                        ADJCODE1 = v.ADJCODE1 + '-' + v.ADJCODE2 + ',' + v.ADJCODE1 + '-' + v.ADJCODE3;
                                    }
                                    else
                                    {
                                        ADJCODE1 = v.ADJCODE1 + '-' + v.ADJCODE2;
                                    }
                                    ADJCODE2 = v.ADJCODE2;
                                    ADJAMT1 = double.Parse(v.ADJAMT1).ToString();
                                }

                                if (v.PATIENTRESPONSIBILITYREASONCODE == "PR" && v.PATIENTRESPONSIBILITYREASONCODE != null)
                                {
                                    PATIENTRESPONSIBILITYREASONCODE = v.PATIENTRESPONSIBILITYREASONCODE + '-' + v.PRTYPE;
                                    PRCode = v.PRTYPE;
                                    switch (v.PRTYPE)
                                    {
                                        case "1":
                                            PRTYPE = "DEDCUTIBLE";
                                            break;
                                        case "2":
                                            PRTYPE = "COINS";
                                            break;
                                        case "3":
                                            PRTYPE = "COPAY";
                                            break;
                                    }
                                    PATIENTRESPONSIBILITY = double.Parse(v.PATIENTRESPONSIBILITY);
                                }
                                else
                                {
                                    if (v.PATIENTRESPONSIBILITYREASONCODE != null)
                                    {
                                        ADJCODE1 = ADJCODE1 + "," + v.PATIENTRESPONSIBILITYREASONCODE + "-" + v.PRTYPE;
                                        ADJCODE2 = ADJCODE2 + "," + v.PRTYPE;
                                        ADJAMT1 = ADJAMT1 + ',' + v.PATIENTRESPONSIBILITY;
                                    }
                                }



                                return new
                                {
                                    LOOPID = v.loopid,
                                    ERAID = v.ERAID,
                                    CLAIMNO = v.CLAIMNO,
                                    PATIENTNAME = v.PATIENTNAME,
                                    INSUREDNAME = v.INSUREDNAME,
                                    CLAIMSTATUS = v.CLAIMSTATUS,
                                    CLAIMPAYMENTAMOUNT = v.CLAIMPAYMENTAMOUNT,
                                    CLAIMADJAMT = v.CLAIMADJAMT,
                                    CLAIMADJCODES = v.CLAIMADJCODES,
                                    CLAIMREMARKCODES = v.CLAIMREMARKCODES,
                                    MEMBERIDENTIFICATION_ = v.MEMBERIDENTIFICATION_,
                                    INSUREDMEMBERIDENTIFICATION_ = v.INSUREDMEMBERIDENTIFICATION_,
                                    PATIENTACCOUNTNUMBER = v.PATIENTACCOUNTNUMBER,
                                    RENNDERINGPROVIDER = v.RENNDERINGPROVIDER,
                                    RENDERINGNPI = v.RENDERINGNPI,
                                    PAYERCLAIMCONTROLNUMBERICN_ = v.PAYERCLAIMCONTROLNUMBERICN_,
                                    PATIENTRESPONSIBILITY = PATIENTRESPONSIBILITY,
                                    PATIENTRESPONSIBILITYREASONCODE = PATIENTRESPONSIBILITYREASONCODE,
                                    PATIENTGROUP_ = v.PATIENTGROUP_,
                                    BEGINSERVICEDATE = v.BEGINSERVICEDATE,
                                    ENDSERVICEDATE = v.ENDSERVICEDATE,
                                    PAIDUNITS = v.PAIDUNITS,
                                    PROCCODE = v.PROCCODE,
                                    MODI = v.MODI,
                                    BILLEDAMOUNT = double.Parse(v.BILLEDAMOUNT),
                                    ALLOWEDAMOUNT = double.Parse(v.ALLOWEDAMOUNT),
                                    PRTYPE = PRTYPE,
                                    ADJCODE1 = ADJCODE1,
                                    ADJCODE2 = ADJCODE2,
                                    ADJCODE3 = v.ADJCODE3,
                                    ADJAMT1 = ADJAMT1,
                                    ADJAMT2 = double.Parse(v.ADJAMT2),
                                    ADJUCODE1 = v.ADJUCODE1,
                                    ADJUCODE2 = v.ADJUCODE2,
                                    ADJUCODE3 = v.ADJUCODE3,
                                    PROVIDERPAID = double.Parse(v.PROVIDERPAID),
                                    DEDUCTAMOUNT = DEDCUTIBLEAMT != 0 ? DEDCUTIBLEAMT : 0.00,
                                    COINSAMOUNT = COINSAMT != 0 ? COINSAMT : 0.00,
                                    COPAYAMOUNT = COPAYAMT != 0 ? COPAYAMT : 0.00,
                                    OTHERADJUSTMENT = (double.Parse(v.ADJAMT2) != 0) ? (ADJAMT1 + ',' + double.Parse(v.ADJAMT2).ToString()) : (ADJAMT1 != "" ? ADJAMT1 : "00"),
                                    PLB_CODE = v.PLB_CODE,
                                    PLBCODE_DESCRIPTION = v.PLB_DESCRIPTION,
                                    plbClm = v.CLAIMNO,
                                    PLB_AMOUNT = v.PLB_AMOUNT,
                                    PRTYPE1 = v.PRTYPE1,
                                    PRESAMT1 = v.PRESAMT1,
                                    PRTYPE2 = v.PRTYPE2,
                                    PRESAMT2 = v.PRESAMT2,
                                    adjfirst = v.ADJCODE1,
                                    ADJCODE4 = v.ADJCODE4,
                                    ADJAMT3 = v.ADJAMT3,
                                    ADJCODE5 = v.ADJCODE5,
                                    ADJCODE6 = v.ADJCODE6,
                                    Claim_RMK_code = v.Claim_RMK_code,
                                    Procedure_RMK_code = v.Procedure_RMK_code

                                };
                            }),

                            claimsTotal = new
                            {
                                BILLEDAMOUNT = group.Sum(v => double.Parse(v.BILLEDAMOUNT)),
                                ALLOWEDAMOUNT = group.Sum(v => double.Parse(v.ALLOWEDAMOUNT)),
                                DEDUCTAMOUNT = group.Sum(v =>
                                {
                                    double DEDCUTIBLEAMT = 0;
                                    switch (v.PRTYPE)
                                    {
                                        case "1":
                                            DEDCUTIBLEAMT = double.Parse(v.PATIENTRESPONSIBILITY);
                                            break;
                                    }

                                    switch (v.PRTYPE1)
                                    {
                                        case "1":
                                            DEDCUTIBLEAMT = double.Parse(v.PRESAMT1);
                                            break;
                                    }

                                    switch (v.PRTYPE2)
                                    {
                                        case "1":
                                            DEDCUTIBLEAMT = double.Parse(v.PRESAMT2);
                                            break;
                                    }

                                    return DEDCUTIBLEAMT;




                                    //double PATIENTRESPONSIBILITY = 0;
                                    //if (v.PATIENTRESPONSIBILITYREASONCODE == "PR" && v.PRTYPE != null && v.PRTYPE == "1")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.PATIENTRESPONSIBILITY);
                                    //}

                                    //if (v.ADJCODE1 == "PR" && v.ADJCODE2 != null && v.ADJCODE2 == "1")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.ADJAMT1);
                                    //}
                                    //return PATIENTRESPONSIBILITY;
                                }),
                                //DEDUCTAMOUNT = group.Sum(v => v.PRTYPE != null && v.PRTYPE == "1" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00),
                                COINSAMOUNT = group.Sum(v =>
                                {
                                    double COINSAMT = 0;
                                    switch (v.PRTYPE)
                                    {
                                        case "2":
                                            COINSAMT = double.Parse(v.PATIENTRESPONSIBILITY);
                                            break;
                                    }

                                    switch (v.PRTYPE1)
                                    {
                                        case "2":
                                            COINSAMT = double.Parse(v.PRESAMT1);
                                            break;
                                    }

                                    switch (v.PRTYPE2)
                                    {
                                        case "2":
                                            COINSAMT = double.Parse(v.PRESAMT2);
                                            break;
                                    }
                                    return COINSAMT;


                                    //double PATIENTRESPONSIBILITY = 0;
                                    //if (v.PATIENTRESPONSIBILITYREASONCODE == "PR" && v.PRTYPE1 != null && v.PRTYPE1 == "2")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.PRESAMT1);
                                    //}

                                    //if (v.ADJCODE1 == "PR" && v.ADJCODE2 != null && v.ADJCODE2 == "2")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.ADJAMT1);
                                    //}
                                    //return PATIENTRESPONSIBILITY;
                                }),
                                //COINSAMOUNT = group.Sum(v => v.PRTYPE != null && v.PRTYPE == "2" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00),
                                COPAYAMOUNT = group.Sum(v =>
                                {
                                    double COPAYAMT = 0;
                                    switch (v.PRTYPE)
                                    {
                                        case "3":
                                            COPAYAMT = double.Parse(v.PATIENTRESPONSIBILITY);
                                            break;
                                    }

                                    switch (v.PRTYPE1)
                                    {
                                        case "3":
                                            COPAYAMT = double.Parse(v.PRESAMT1);
                                            break;
                                    }

                                    switch (v.PRTYPE2)
                                    {
                                        case "3":
                                            COPAYAMT = double.Parse(v.PRESAMT2);
                                            break;
                                    }

                                    return COPAYAMT;



                                    //double PATIENTRESPONSIBILITY = 0;
                                    //if (v.PATIENTRESPONSIBILITYREASONCODE == "PR" && v.PRTYPE2 != null && v.PRTYPE2 == "3")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.PRESAMT2);
                                    //}

                                    //if (v.ADJCODE1 == "PR" && v.ADJCODE2 != null && v.ADJCODE2 == "3")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.ADJAMT1);
                                    //}
                                    //return PATIENTRESPONSIBILITY;
                                }),


                                //OTHERADJUSTMENT = group.Sum(v => double.Parse(v.ADJAMT1) + double.Parse(v.ADJAMT2)),


                                OTHERADJUSTMENT = group.Sum(v =>
                                {
                                    double ADJAMT1 = 0;

                                    if (v.PATIENTRESPONSIBILITYREASONCODE != "PR" && v.PATIENTRESPONSIBILITYREASONCODE != null)
                                    {
                                        ADJAMT1 = double.Parse(v.PATIENTRESPONSIBILITY);
                                    }

                                    if (v.ADJCODE1 != "PR")
                                    {
                                        ADJAMT1 += double.Parse(v.ADJAMT1);
                                    }

                                    double sum = 0;
                                    if (v.ADJAMT3 != null)
                                    {
                                        sum = ADJAMT1 + double.Parse(v.ADJAMT2) + double.Parse(v.ADJAMT3);
                                        return sum;
                                    }
                                    else if (v.ADJAMT2 != null)
                                    {
                                        sum = ADJAMT1 + double.Parse(v.ADJAMT2);
                                        return sum;
                                    }



                                    //double adjamt1Value = 0.0;
                                    //if (!string.IsNullOrEmpty(ADJAMT1) && double.TryParse(ADJAMT1, out adjamt1Value))
                                    //{
                                    //    return adjamt1Value + double.Parse(v.ADJAMT2);
                                    //}

                                    return sum;
                                }),

                                PROVIDERPAID = group.Sum(v => double.Parse(v.PROVIDERPAID))
                            }
                        });

                        if (eraClaims_sp_result != null && eraClaims_sp_result.Count() > 0)
                        {
                            var checkTotal = eraClaims_sp_result.GroupBy(c => c.ERAID).Select(group => new
                            {
                                BILLEDAMOUNT = group.Sum(v => double.Parse(v.BILLEDAMOUNT)),
                                ALLOWEDAMOUNT = group.Sum(v => double.Parse(v.ALLOWEDAMOUNT)),

                                //DEDUCTAMOUNT = group.Sum(v => v.PRTYPE != null && v.PRTYPE == "1" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00),
                                //COINSAMOUNT = group.Sum(v => v.PRTYPE != null && v.PRTYPE == "2" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00),
                                //COPAYAMOUNT = group.Sum(v => v.PRTYPE != null && v.PRTYPE == "3" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00),
                                DEDUCTAMOUNT = group.Sum(v =>
                                {
                                    double DEDCUTIBLEAMT = 0;
                                    switch (v.PRTYPE)
                                    {
                                        case "1":
                                            DEDCUTIBLEAMT = double.Parse(v.PATIENTRESPONSIBILITY);
                                            break;
                                    }

                                    switch (v.PRTYPE1)
                                    {
                                        case "1":
                                            DEDCUTIBLEAMT = double.Parse(v.PRESAMT1);
                                            break;
                                    }

                                    switch (v.PRTYPE2)
                                    {
                                        case "1":
                                            DEDCUTIBLEAMT = double.Parse(v.PRESAMT2);
                                            break;
                                    }

                                    return DEDCUTIBLEAMT;


                                    //double PATIENTRESPONSIBILITY = 0;
                                    //if (v.PATIENTRESPONSIBILITYREASONCODE == "PR" && v.PRTYPE != null && v.PRTYPE == "1")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.PATIENTRESPONSIBILITY);
                                    //}

                                    //if (v.ADJCODE1 == "PR" && v.ADJCODE2 != null && v.ADJCODE2 == "1")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.ADJAMT1);
                                    //}
                                    //return PATIENTRESPONSIBILITY;
                                }),
                                //DEDUCTAMOUNT = group.Sum(v => v.PRTYPE != null && v.PRTYPE == "1" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00),
                                COINSAMOUNT = group.Sum(v =>
                                {
                                    double COINSAMT = 0;
                                    switch (v.PRTYPE)
                                    {
                                        case "2":
                                            COINSAMT = double.Parse(v.PATIENTRESPONSIBILITY);
                                            break;
                                    }

                                    switch (v.PRTYPE1)
                                    {
                                        case "2":
                                            COINSAMT = double.Parse(v.PRESAMT1);
                                            break;
                                    }

                                    switch (v.PRTYPE2)
                                    {
                                        case "2":
                                            COINSAMT = double.Parse(v.PRESAMT2);
                                            break;
                                    }
                                    return COINSAMT;


                                    //double PATIENTRESPONSIBILITY = 0;
                                    //if (v.PATIENTRESPONSIBILITYREASONCODE == "PR" && v.PRTYPE1 != null && v.PRTYPE1 == "2")
                                    //{
                                    //    PATIENTRESPONSIBILITY =  double.Parse(v.PRESAMT1);
                                    //}

                                    //if (v.ADJCODE1 == "PR" && v.ADJCODE2 != null && v.ADJCODE2 == "2")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.ADJAMT1);
                                    //}
                                    //return PATIENTRESPONSIBILITY;
                                }),
                                //COINSAMOUNT = group.Sum(v => v.PRTYPE != null && v.PRTYPE == "2" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00),
                                COPAYAMOUNT = group.Sum(v =>
                                {
                                    double COPAYAMT = 0;
                                    switch (v.PRTYPE)
                                    {
                                        case "3":
                                            COPAYAMT = double.Parse(v.PATIENTRESPONSIBILITY);
                                            break;
                                    }

                                    switch (v.PRTYPE1)
                                    {
                                        case "3":
                                            COPAYAMT = double.Parse(v.PRESAMT1);
                                            break;
                                    }

                                    switch (v.PRTYPE2)
                                    {
                                        case "3":
                                            COPAYAMT = double.Parse(v.PRESAMT2);
                                            break;
                                    }

                                    return COPAYAMT;


                                    //double PATIENTRESPONSIBILITY = 0;
                                    //if (v.PATIENTRESPONSIBILITYREASONCODE == "PR" && v.PRTYPE2 != null && v.PRTYPE2 == "3")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.PRESAMT2);
                                    //}

                                    //if (v.ADJCODE1 == "PR" && v.ADJCODE2 != null && v.ADJCODE2 == "3")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.ADJAMT1);
                                    //}
                                    //return PATIENTRESPONSIBILITY;
                                }),
                                //OTHERADJUSTMENT = group.Sum(v => double.Parse(v.ADJAMT1) + double.Parse(v.ADJAMT2)),
                                OTHERADJUSTMENT = group.Sum(v =>
                                {
                                    string ADJAMT1 = "";

                                    if (v.PATIENTRESPONSIBILITYREASONCODE != "PR")
                                    {
                                        ADJAMT1 = v.PATIENTRESPONSIBILITYREASONCODE;
                                    }

                                    if (v.ADJAMT1 != "PR")
                                    {
                                        ADJAMT1 += v.ADJAMT1;
                                    }

                                    double adjamt1Value = 0.0;
                                    if (!string.IsNullOrEmpty(ADJAMT1) && double.TryParse(ADJAMT1, out adjamt1Value) && !string.IsNullOrEmpty(v.ADJAMT3))
                                    {
                                        return adjamt1Value + double.Parse(v.ADJAMT2) + double.Parse(v.ADJAMT3);
                                    }
                                    else if (!string.IsNullOrEmpty(ADJAMT1) && double.TryParse(ADJAMT1, out adjamt1Value))
                                    {
                                        return adjamt1Value + double.Parse(v.ADJAMT2);
                                    }

                                    return double.Parse(v.ADJAMT2);
                                }),
                                PROVIDERPAID = group.Sum(v => double.Parse(v.PROVIDERPAID))
                            }).Single();
                            res.Response.checkTotal = checkTotal;
                        }
                        else
                        {
                            res.Response.checkTotal = new
                            {
                                BILLEDAMOUNT = 0.00,
                                ALLOWEDAMOUNT = 0.00,
                                DEDUCTAMOUNT = 0.00,
                                COINSAMOUNT = 0.00,
                                COPAYAMOUNT = 0.00,
                                OTHERADJUSTMENT = 0.00,
                                PROVIDERPAID = 0.00
                            };
                        }


                        decimal testing = 0;
                        foreach (var i in eraClaims_sp_result)
                        {
                            //testing += i.plbAmt.Value;
                            if (i.PLB_CODE == "WO" && i.PLB_AMOUNT.Value > 0)
                            {
                                testing += i.PLB_AMOUNT.Value;
                            }
                            if (i.PLB_CODE == "L6" && i.PLB_AMOUNT.Value < 0)
                            {
                                testing += i.PLB_AMOUNT.Value;
                            }

                        }
                        eraDetails[0].ProviderAdjAmt = testing.ToString();




                        //jsdhjfhs
                        var ProviderAdjDetail = eraClaims_sp_result.GroupBy(c => c.PATIENTACCOUNTNUMBER).Select(group => new
                        {
                            claims = group.Select(v =>
                            {
                                return new
                                {
                                    LOOPID = v.loopid,
                                    ERAID = v.ERAID,
                                    PLB_CODE = v.PLB_CODE,
                                    PLBCODE_DESCRIPTION = v.PLB_DESCRIPTION,
                                    plbClm = v.CLAIMNO,
                                    PLB_AMOUNT = v.PLB_AMOUNT
                                };
                            }),


                        });

                        var ProviderAdjDetail2 = eraClaims_sp_result.Select(c =>
                        {

                            return new
                            {
                                LOOPID = c.loopid,
                                ERAID = c.ERAID,
                                PLB_CODE = c.PLB_CODE,
                                PLBCODE_DESCRIPTION = c.PLB_DESCRIPTION,
                                plbClm = c.CLAIMNO,
                                PLB_AMOUNT = c.PLB_AMOUNT
                            };


                        });




                        //sdfsdjfhsjkfh

                        res.Status = "success";
                        res.Response.era = eraDetails;
                        res.Response.eraClaims = claimDetails;
                        res.Response.glossary = glossary;
                        res.Response.ProviderAdjDetail = ProviderAdjDetail2;
                    }
                    else
                    {
                        res.Status = "invalid-era-id";
                        res.Response = "No ERA found with id " + model.eraId;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }
        public ResponseModel ViewEraSummary(ViewERASummaryRequest model)
        {
            ResponseModel res = new ResponseModel();
            res.Response = new ExpandoObject();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var eraDetails = ctx.SP_ERASEARCH(model.eraId, null, null, null, null, null, null, null, null, null).ToList();
                    //var eraClaims_sp_result = ctx.SP_ERACLAIMDETAIL(null, model.eraId, null, null, null).ToList();
                    var eraClaims_sp_result = ctx.SP_ERACLAIMDETAIL_VIEW_EDI(model.claim_no, model.eraId, null, null, null, model.CHECK_NO).ToList();
                    //var glossary = ctx.SP_ERAADJCODEGLOSSARY(model.eraId).ToList();
                    var glossary = ctx.SP_ERAADJCODEGLOSSARY_VIEW_EDI(model.eraId, model.claim_no).ToList();
                    List<object> claimsList = new List<object>();
                    dynamic claim = "";
                    if (eraDetails != null && eraClaims_sp_result != null)
                    {

                        var claimDetails = eraClaims_sp_result.GroupBy(c => c.PATIENTACCOUNTNUMBER).Select(group => new
                        {
                            claims = group.Select(v =>
                            {
                                double DEDCUTIBLEAMT = 0;
                                double COINSAMT = 0;
                                double COPAYAMT = 0;
                                switch (v.PRTYPE)
                                {
                                    case "1":
                                        DEDCUTIBLEAMT = double.Parse(v.PATIENTRESPONSIBILITY);
                                        break;
                                    case "2":
                                        COINSAMT = double.Parse(v.PATIENTRESPONSIBILITY);
                                        break;
                                    case "3":
                                        COPAYAMT = double.Parse(v.PATIENTRESPONSIBILITY);
                                        break;
                                }

                                switch (v.PRTYPE1)
                                {
                                    case "1":
                                        DEDCUTIBLEAMT = double.Parse(v.PRESAMT1);
                                        break;
                                    case "2":
                                        COINSAMT = double.Parse(v.PRESAMT1);
                                        break;
                                    case "3":
                                        COPAYAMT = double.Parse(v.PRESAMT1);
                                        break;
                                }

                                switch (v.PRTYPE2)
                                {
                                    case "1":
                                        DEDCUTIBLEAMT = double.Parse(v.PRESAMT2);
                                        break;
                                    case "2":
                                        COINSAMT = double.Parse(v.PRESAMT2);
                                        break;
                                    case "3":
                                        COPAYAMT = double.Parse(v.PRESAMT2);
                                        break;
                                }

                                var PATIENTRESPONSIBILITYREASONCODE = "";
                                var PRTYPE = "";
                                var PRCode = "";

                                double PATIENTRESPONSIBILITY = 0;

                                var ADJCODE1 = "";
                                var ADJCODE2 = "";
                                string ADJAMT1 = "";

                                if (v.ADJCODE1 == "PR")
                                {
                                    PATIENTRESPONSIBILITYREASONCODE = v.ADJCODE1 + '-' + v.ADJCODE2;
                                    PRCode = v.ADJCODE2;
                                    switch (v.ADJCODE2)
                                    {
                                        case "1":
                                            PRTYPE = "DEDCUTIBLE";
                                            break;
                                        case "2":
                                            PRTYPE = "COINS";
                                            break;
                                        case "3":
                                            PRTYPE = "COPAY";
                                            break;
                                    }
                                    PATIENTRESPONSIBILITY = double.Parse(v.ADJAMT1);
                                }
                                else
                                {
                                    if (v.ADJCODE3 != null)
                                    {
                                        ADJCODE1 = v.ADJCODE1 + '-' + v.ADJCODE2 + ',' + v.ADJCODE1 + '-' + v.ADJCODE3;
                                    }
                                    else
                                    {
                                        ADJCODE1 = v.ADJCODE1 + '-' + v.ADJCODE2;
                                    }
                                    ADJCODE2 = v.ADJCODE2;
                                    ADJAMT1 = double.Parse(v.ADJAMT1).ToString();
                                }

                                if (v.PATIENTRESPONSIBILITYREASONCODE == "PR" && v.PATIENTRESPONSIBILITYREASONCODE != null)
                                {
                                    PATIENTRESPONSIBILITYREASONCODE = v.PATIENTRESPONSIBILITYREASONCODE + '-' + v.PRTYPE;
                                    PRCode = v.PRTYPE;
                                    switch (v.PRTYPE)
                                    {
                                        case "1":
                                            PRTYPE = "DEDCUTIBLE";
                                            break;
                                        case "2":
                                            PRTYPE = "COINS";
                                            break;
                                        case "3":
                                            PRTYPE = "COPAY";
                                            break;
                                    }
                                    PATIENTRESPONSIBILITY = double.Parse(v.PATIENTRESPONSIBILITY);
                                }
                                else
                                {
                                    if (v.PATIENTRESPONSIBILITYREASONCODE != null)
                                    {
                                        ADJCODE1 = ADJCODE1 + "," + v.PATIENTRESPONSIBILITYREASONCODE + "-" + v.PRTYPE;
                                        ADJCODE2 = ADJCODE2 + "," + v.PRTYPE;
                                        ADJAMT1 = ADJAMT1 + ',' + v.PATIENTRESPONSIBILITY;
                                    }
                                }



                                return new
                                {
                                    LOOPID = v.loopid,
                                    ERAID = v.ERAID,
                                    CLAIMNO = v.CLAIMNO,
                                    PATIENTNAME = v.PATIENTNAME,
                                    INSUREDNAME = v.INSUREDNAME,
                                    CLAIMSTATUS = v.CLAIMSTATUS,
                                    CLAIMPAYMENTAMOUNT = v.CLAIMPAYMENTAMOUNT,
                                    CLAIMADJAMT = v.CLAIMADJAMT,
                                    CLAIMADJCODES = v.CLAIMADJCODES,
                                    CLAIMREMARKCODES = v.CLAIMREMARKCODES,
                                    MEMBERIDENTIFICATION_ = v.MEMBERIDENTIFICATION_,
                                    INSUREDMEMBERIDENTIFICATION_ = v.INSUREDMEMBERIDENTIFICATION_,
                                    PATIENTACCOUNTNUMBER = v.PATIENTACCOUNTNUMBER,
                                    RENNDERINGPROVIDER = v.RENNDERINGPROVIDER,
                                    RENDERINGNPI = v.RENDERINGNPI,
                                    PAYERCLAIMCONTROLNUMBERICN_ = v.PAYERCLAIMCONTROLNUMBERICN_,
                                    PATIENTRESPONSIBILITY = PATIENTRESPONSIBILITY,
                                    PATIENTRESPONSIBILITYREASONCODE = PATIENTRESPONSIBILITYREASONCODE,
                                    PATIENTGROUP_ = v.PATIENTGROUP_,
                                    BEGINSERVICEDATE = v.BEGINSERVICEDATE,
                                    ENDSERVICEDATE = v.ENDSERVICEDATE,
                                    PAIDUNITS = v.PAIDUNITS,
                                    PROCCODE = v.PROCCODE,
                                    MODI = v.MODI,
                                    BILLEDAMOUNT = double.Parse(v.BILLEDAMOUNT),
                                    ALLOWEDAMOUNT = double.Parse(v.ALLOWEDAMOUNT),
                                    PRTYPE = PRTYPE,
                                    ADJCODE1 = ADJCODE1,
                                    ADJCODE2 = ADJCODE2,
                                    ADJCODE3 = v.ADJCODE3,
                                    ADJAMT1 = ADJAMT1,
                                    ADJAMT2 = double.Parse(v.ADJAMT2),
                                    ADJUCODE1 = v.ADJUCODE1,
                                    ADJUCODE2 = v.ADJUCODE2,
                                    ADJUCODE3 = v.ADJUCODE3,
                                    PROVIDERPAID = double.Parse(v.PROVIDERPAID),
                                    DEDUCTAMOUNT = DEDCUTIBLEAMT != 0 ? DEDCUTIBLEAMT : 0.00,
                                    COINSAMOUNT = COINSAMT != 0 ? COINSAMT : 0.00,
                                    COPAYAMOUNT = COPAYAMT != 0 ? COPAYAMT : 0.00,
                                    OTHERADJUSTMENT = (double.Parse(v.ADJAMT2) != 0) ? (ADJAMT1 + ',' + double.Parse(v.ADJAMT2).ToString()) : (ADJAMT1 != "" ? ADJAMT1 : "00"),
                                    PLB_CODE = v.PLB_CODE,
                                    PLBCODE_DESCRIPTION = v.PLB_DESCRIPTION,
                                    plbClm = v.CLAIMNO,
                                    PLB_AMOUNT = v.PLB_AMOUNT,
                                    PRTYPE1 = v.PRTYPE1,
                                    PRESAMT1 = v.PRESAMT1,
                                    PRTYPE2 = v.PRTYPE2,
                                    PRESAMT2 = v.PRESAMT2,
                                    adjfirst = v.ADJCODE1,
                                    ADJCODE4 = v.ADJCODE4,
                                    ADJAMT3 = v.ADJAMT3,
                                    ADJCODE5 = v.ADJCODE5,
                                    ADJCODE6 = v.ADJCODE6,
                                    Claim_RMK_code = v.Claim_RMK_code,
                                    Procedure_RMK_code = v.Procedure_RMK_code

                                };
                            }),

                            claimsTotal = new
                            {
                                BILLEDAMOUNT = group.Sum(v => double.Parse(v.BILLEDAMOUNT)),
                                ALLOWEDAMOUNT = group.Sum(v => double.Parse(v.ALLOWEDAMOUNT)),
                                DEDUCTAMOUNT = group.Sum(v =>
                                {
                                    double DEDCUTIBLEAMT = 0;
                                    switch (v.PRTYPE)
                                    {
                                        case "1":
                                            DEDCUTIBLEAMT = double.Parse(v.PATIENTRESPONSIBILITY);
                                            break;
                                    }

                                    switch (v.PRTYPE1)
                                    {
                                        case "1":
                                            DEDCUTIBLEAMT = double.Parse(v.PRESAMT1);
                                            break;
                                    }

                                    switch (v.PRTYPE2)
                                    {
                                        case "1":
                                            DEDCUTIBLEAMT = double.Parse(v.PRESAMT2);
                                            break;
                                    }

                                    return DEDCUTIBLEAMT;




                                    //double PATIENTRESPONSIBILITY = 0;
                                    //if (v.PATIENTRESPONSIBILITYREASONCODE == "PR" && v.PRTYPE != null && v.PRTYPE == "1")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.PATIENTRESPONSIBILITY);
                                    //}

                                    //if (v.ADJCODE1 == "PR" && v.ADJCODE2 != null && v.ADJCODE2 == "1")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.ADJAMT1);
                                    //}
                                    //return PATIENTRESPONSIBILITY;
                                }),
                                //DEDUCTAMOUNT = group.Sum(v => v.PRTYPE != null && v.PRTYPE == "1" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00),
                                COINSAMOUNT = group.Sum(v =>
                                {
                                    double COINSAMT = 0;
                                    switch (v.PRTYPE)
                                    {
                                        case "2":
                                            COINSAMT = double.Parse(v.PATIENTRESPONSIBILITY);
                                            break;
                                    }

                                    switch (v.PRTYPE1)
                                    {
                                        case "2":
                                            COINSAMT = double.Parse(v.PRESAMT1);
                                            break;
                                    }

                                    switch (v.PRTYPE2)
                                    {
                                        case "2":
                                            COINSAMT = double.Parse(v.PRESAMT2);
                                            break;
                                    }
                                    return COINSAMT;


                                    //double PATIENTRESPONSIBILITY = 0;
                                    //if (v.PATIENTRESPONSIBILITYREASONCODE == "PR" && v.PRTYPE1 != null && v.PRTYPE1 == "2")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.PRESAMT1);
                                    //}

                                    //if (v.ADJCODE1 == "PR" && v.ADJCODE2 != null && v.ADJCODE2 == "2")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.ADJAMT1);
                                    //}
                                    //return PATIENTRESPONSIBILITY;
                                }),
                                //COINSAMOUNT = group.Sum(v => v.PRTYPE != null && v.PRTYPE == "2" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00),
                                COPAYAMOUNT = group.Sum(v =>
                                {
                                    double COPAYAMT = 0;
                                    switch (v.PRTYPE)
                                    {
                                        case "3":
                                            COPAYAMT = double.Parse(v.PATIENTRESPONSIBILITY);
                                            break;
                                    }

                                    switch (v.PRTYPE1)
                                    {
                                        case "3":
                                            COPAYAMT = double.Parse(v.PRESAMT1);
                                            break;
                                    }

                                    switch (v.PRTYPE2)
                                    {
                                        case "3":
                                            COPAYAMT = double.Parse(v.PRESAMT2);
                                            break;
                                    }

                                    return COPAYAMT;



                                    //double PATIENTRESPONSIBILITY = 0;
                                    //if (v.PATIENTRESPONSIBILITYREASONCODE == "PR" && v.PRTYPE2 != null && v.PRTYPE2 == "3")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.PRESAMT2);
                                    //}

                                    //if (v.ADJCODE1 == "PR" && v.ADJCODE2 != null && v.ADJCODE2 == "3")
                                    //{
                                    //    PATIENTRESPONSIBILITY = double.Parse(v.ADJAMT1);
                                    //}
                                    //return PATIENTRESPONSIBILITY;
                                }),


                                //OTHERADJUSTMENT = group.Sum(v => double.Parse(v.ADJAMT1) + double.Parse(v.ADJAMT2)),


                                OTHERADJUSTMENT = group.Sum(v =>
                                {
                                    double ADJAMT1 = 0;

                                    if (v.PATIENTRESPONSIBILITYREASONCODE != "PR" && v.PATIENTRESPONSIBILITYREASONCODE != null)
                                    {
                                        ADJAMT1 = double.Parse(v.PATIENTRESPONSIBILITY);
                                    }

                                    if (v.ADJCODE1 != "PR")
                                    {
                                        ADJAMT1 += double.Parse(v.ADJAMT1);
                                    }

                                    double sum = 0;
                                    if (v.ADJAMT3 != null)
                                    {
                                        sum = ADJAMT1 + double.Parse(v.ADJAMT2) + double.Parse(v.ADJAMT3);
                                        return sum;
                                    }
                                    else if (v.ADJAMT2 != null)
                                    {
                                        sum = ADJAMT1 + double.Parse(v.ADJAMT2);
                                        return sum;
                                    }



                                    //double adjamt1Value = 0.0;
                                    //if (!string.IsNullOrEmpty(ADJAMT1) && double.TryParse(ADJAMT1, out adjamt1Value))
                                    //{
                                    //    return adjamt1Value + double.Parse(v.ADJAMT2);
                                    //}

                                    return sum;
                                }),

                                PROVIDERPAID = group.Sum(v => double.Parse(v.PROVIDERPAID))
                            }
                        });

                        if (eraClaims_sp_result != null && eraClaims_sp_result.Count() > 0)
                        {

                            //res.Response.checkTotal = checkTotal;
                        }
                        else
                        {
                            res.Response.checkTotal = new
                            {
                                BILLEDAMOUNT = 0.00,
                                ALLOWEDAMOUNT = 0.00,
                                DEDUCTAMOUNT = 0.00,
                                COINSAMOUNT = 0.00,
                                COPAYAMOUNT = 0.00,
                                OTHERADJUSTMENT = 0.00,
                                PROVIDERPAID = 0.00
                            };
                        }


                        decimal testing = 0;
                        foreach (var i in eraClaims_sp_result)
                        {
                            //testing += i.plbAmt.Value;
                            if (i.PLB_CODE == "WO" && i.PLB_AMOUNT.Value > 0)
                            {
                                testing += i.PLB_AMOUNT.Value;
                            }
                            if (i.PLB_CODE == "L6" && i.PLB_AMOUNT.Value < 0)
                            {
                                testing += i.PLB_AMOUNT.Value;
                            }

                        }
                        eraDetails[0].ProviderAdjAmt = testing.ToString();




                        //jsdhjfhs
                        var ProviderAdjDetail = eraClaims_sp_result.GroupBy(c => c.PATIENTACCOUNTNUMBER).Select(group => new
                        {
                            claims = group.Select(v =>
                            {
                                return new
                                {
                                    LOOPID = v.loopid,
                                    ERAID = v.ERAID,
                                    PLB_CODE = v.PLB_CODE,
                                    PLBCODE_DESCRIPTION = v.PLB_DESCRIPTION,
                                    plbClm = v.CLAIMNO,
                                    PLB_AMOUNT = v.PLB_AMOUNT
                                };
                            }),


                        });

                        var ProviderAdjDetail2 = eraClaims_sp_result.Select(c =>
                        {

                            return new
                            {
                                LOOPID = c.loopid,
                                ERAID = c.ERAID,
                                PLB_CODE = c.PLB_CODE,
                                PLBCODE_DESCRIPTION = c.PLB_DESCRIPTION,
                                plbClm = c.CLAIMNO,
                                PLB_AMOUNT = c.PLB_AMOUNT
                            };


                        });




                        //sdfsdjfhsjkfh

                        res.Status = "success";
                        res.Response.era = eraDetails;
                        res.Response.eraClaims = claimDetails;
                        res.Response.glossary = glossary;
                        res.Response.ProviderAdjDetail = ProviderAdjDetail2;
                    }
                    else
                    {
                        res.Status = "invalid-era-id";
                        res.Response = "No ERA found with id " + model.eraId;
                    }
                }
            }
            catch (Exception)
            {
                res.Status = "Error";
                res.Response = "No ERA found with id " + model.eraId;
                //throw;
            }
            return res;
        }




        //public ResponseModel EraSummary(EraSummaryRequest model)
        //{
        //    ResponseModel res = new ResponseModel();
        //    res.Response = new ExpandoObject();
        //    try
        //    {
        //        using (var ctx = new NPMDBEntities())
        //        {
        //            var eraDetails = ctx.SP_ERASEARCH(model.eraId, null, null, null, null, null, null, null, null, null).FirstOrDefault(    );
        //            var eraClaims_sp_result = ctx.SP_ERACLAIMDETAIL(null, model.eraId, null, null, null).ToList();
        //            var glossary = ctx.SP_ERAADJCODEGLOSSARY(model.eraId).ToList();
        //            if (eraDetails != null && eraClaims_sp_result != null)
        //            {
        //                var claimDetails = eraClaims_sp_result.GroupBy(c => c.PATIENTACCOUNTNUMBER).Select(group => new
        //                {
        //                    claims = group.Select(v => new
        //                    {
        //                        ERAID = v.ERAID,
        //                        CLAIMNO = v.CLAIMNO,
        //                        PATIENTNAME = v.PATIENTNAME,
        //                        INSUREDNAME = v.INSUREDNAME,
        //                        CLAIMSTATUS = v.CLAIMSTATUS,
        //                        CLAIMPAYMENTAMOUNT = v.CLAIMPAYMENTAMOUNT,
        //                        CLAIMADJAMT = v.CLAIMADJAMT,
        //                        CLAIMADJCODES = v.CLAIMADJCODES,
        //                        CLAIMREMARKCODES = v.CLAIMREMARKCODES,
        //                        MEMBERIDENTIFICATION_ = v.MEMBERIDENTIFICATION_,
        //                        INSUREDMEMBERIDENTIFICATION_ = v.INSUREDMEMBERIDENTIFICATION_,
        //                        PATIENTACCOUNTNUMBER = v.PATIENTACCOUNTNUMBER,
        //                        RENNDERINGPROVIDER = v.RENNDERINGPROVIDER,
        //                        RENDERINGNPI = v.RENDERINGNPI,
        //                        PAYERCLAIMCONTROLNUMBERICN_ = v.PAYERCLAIMCONTROLNUMBERICN_,
        //                        PATIENTRESPONSIBILITY = v.PATIENTRESPONSIBILITY,
        //                        PATIENTRESPONSIBILITYREASONCODE = v.PATIENTRESPONSIBILITYREASONCODE,
        //                        PATIENTGROUP_ = v.PATIENTGROUP_,
        //                        BEGINSERVICEDATE = v.BEGINSERVICEDATE,
        //                        ENDSERVICEDATE = v.ENDSERVICEDATE,
        //                        PAIDUNITS = v.PAIDUNITS,
        //                        PROCCODE = v.PROCCODE,
        //                        MODI = v.MODI,
        //                        BILLEDAMOUNT = double.Parse(v.BILLEDAMOUNT),
        //                        ALLOWEDAMOUNT = double.Parse(v.ALLOWEDAMOUNT),
        //                        PRTYPE = v.PRTYPE,
        //                        ADJCODE1 = v.ADJCODE1,
        //                        ADJCODE2 = v.ADJCODE2,
        //                        ADJCODE3 = v.ADJCODE3,
        //                        ADJAMT1 = double.Parse(v.ADJAMT1),
        //                        ADJAMT2 = double.Parse(v.ADJAMT2),
        //                        ADJUCODE1 = v.ADJUCODE1,
        //                        ADJUCODE2 = v.ADJUCODE2,
        //                        ADJUCODE3 = v.ADJUCODE3,
        //                        PROVIDERPAID = double.Parse(v.PROVIDERPAID),
        //                        DEDUCTAMOUNT = v.PRTYPE != null && v.PRTYPE.ToLower() == "deduct" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00,
        //                        COINSAMOUNT = v.PRTYPE != null && v.PRTYPE.ToLower() == "coins" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00,
        //                        COPAYAMOUNT = v.PRTYPE != null && v.PRTYPE.ToLower() == "copay" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00,
        //                        OTHERADJUSTMENT = double.Parse(v.ADJAMT1) + double.Parse(v.ADJAMT2),
        //                    }),
        //                    claimsTotal = new
        //                    {
        //                        BILLEDAMOUNT = group.Sum(v => double.Parse(v.BILLEDAMOUNT)),
        //                        ALLOWEDAMOUNT = group.Sum(v => double.Parse(v.ALLOWEDAMOUNT)),
        //                        DEDUCTAMOUNT = group.Sum(v => v.PRTYPE != null && v.PRTYPE.ToLower() == "deduct" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00),
        //                        COINSAMOUNT = group.Sum(v => v.PRTYPE != null && v.PRTYPE.ToLower() == "coins" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00),
        //                        COPAYAMOUNT = group.Sum(v => v.PRTYPE != null && v.PRTYPE.ToLower() == "copay" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00),
        //                        OTHERADJUSTMENT = group.Sum(v => double.Parse(v.ADJAMT1) + double.Parse(v.ADJAMT2)),
        //                        PROVIDERPAID = group.Sum(v => double.Parse(v.PROVIDERPAID))
        //                    }
        //                });
        //                if (eraClaims_sp_result != null && eraClaims_sp_result.Count() > 0)
        //                {
        //                    var checkTotal = eraClaims_sp_result.GroupBy(c => c.ERAID).Select(group => new
        //                    {
        //                        BILLEDAMOUNT = group.Sum(v => double.Parse(v.BILLEDAMOUNT)),
        //                        ALLOWEDAMOUNT = group.Sum(v => double.Parse(v.ALLOWEDAMOUNT)),
        //                        DEDUCTAMOUNT = group.Sum(v => v.PRTYPE != null && v.PRTYPE.ToLower() == "deduct" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00),
        //                        COINSAMOUNT = group.Sum(v => v.PRTYPE != null && v.PRTYPE.ToLower() == "coins" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00),
        //                        COPAYAMOUNT = group.Sum(v => v.PRTYPE != null && v.PRTYPE.ToLower() == "copay" ? double.Parse(v.PATIENTRESPONSIBILITY) : 0.00),
        //                        OTHERADJUSTMENT = group.Sum(v => double.Parse(v.ADJAMT1) + double.Parse(v.ADJAMT2)),
        //                        PROVIDERPAID = group.Sum(v => double.Parse(v.PROVIDERPAID))
        //                    }).Single();
        //                    res.Response.checkTotal = checkTotal;
        //                }
        //                else
        //                {
        //                    res.Response.checkTotal = new
        //                    {
        //                        BILLEDAMOUNT = 0.00,
        //                        ALLOWEDAMOUNT = 0.00,
        //                        DEDUCTAMOUNT = 0.00,
        //                        COINSAMOUNT = 0.00,
        //                        COPAYAMOUNT = 0.00,
        //                        OTHERADJUSTMENT = 0.00,
        //                        PROVIDERPAID = 0.00
        //                    };
        //                }

        //                res.Status = "success";
        //                res.Response.era = eraDetails;
        //                res.Response.eraClaims = claimDetails;
        //                res.Response.glossary = glossary;
        //            }
        //            else
        //            {
        //                res.Status = "invalid-era-id";
        //                res.Response = "No ERA found with id " + model.eraId;
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return res;
        //}

        public ResponseModel ERAClaimSummary(claimsummaryrequest model)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var eraDetails = ctx.SP_ERASEARCH(model.eraId, null, null, null, null, null, null, null, null, null).FirstOrDefault();
                    var eraClaims = ctx.SP_ERACLAIMDETAIL(null, model.eraId, null, null, null).ToList();
                    if (eraDetails != null && eraClaims != null)
                    {
                        res.Status = "success";
                        res.Response = new ExpandoObject();
                        res.Response.era = eraDetails;
                        res.Response.eraClaims = eraClaims;
                    }
                    else
                    {
                        res.Status = "invalid-era-id";
                        res.Response = "No ERA found with id " + model.eraId;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }

        public ResponseModel ApplyERA(ApplyERARequestModel req, long userId)
        {


            ResponseModel res = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var status = ctx.SP_ERACLAIMSAUTOPOST(req.eraId, string.Join(",", req.claims), userId, req.depositDate);
                    var eraDetails = ctx.SP_ERASEARCH(req.eraId, null, null, null, null, null, null, null, null, null).FirstOrDefault();
                    var eraClaims = ctx.SP_ERACLAIMDETAIL(null, req.eraId, null, null, null).ToList();
                    res.Response = new ExpandoObject();
                    res.Response.era = eraDetails;
                    res.Response.eraClaims = eraClaims;
                    res.Status = "success";
                    if (res.Status == "success")
                    {
                        var updatebit = ctx.USP_UpdateSyncedClaim_ERA(req.eraId);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return res;
        }

        public ResponseModel ERAClaimsOverPayment(claimsummaryrequest model)
        {


            ResponseModel res = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var eraOverpaidClaims = ctx.USP_ERAClaimOverPayment(model.eraId).ToList();
                    res.Response = new ExpandoObject();
                    res.Response = eraOverpaidClaims;
                    res.Status = "success";
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return res;
        }


        public ResponseModel AutoPost(ERAAutoPostRequestModel request, long userId)
        {




            ResponseModel res = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var status = ctx.SP_ERAAUTOPOST(request.id, userId, request.depositDate);
                    var eraDetails = ctx.SP_ERASEARCH(request.id, null, null, null, null, null, null, null, null, null).FirstOrDefault();
                    var eraClaims = ctx.SP_ERACLAIMDETAIL(null, request.id, null, null, null).ToList();
                    res.Response = new ExpandoObject();
                    res.Response.era = eraDetails;
                    res.Response.eraClaims = eraClaims;
                    res.Status = "success";
                    if (res.Status == "success")
                    {
                        var updatebit = ctx.USP_UpdateSyncedClaim_ERA(request.id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }
        #region Generate_Packet_837i_File!


        public ResponseModel GenerateBatch_For_Packet_837i_5010_I(long practice_id, long claim_id)
        {
            //var res = read_Tasks_And_Insert("Opera scheduled Autoupdate 1675961446");
            ResponseModel objResponse = new ResponseModel();
            try
            {
                string strBatchString = "";
                int segmentCount = 0;
                List<string> errorList;

                //string billingOrganizationName = "practiceName";//practiceName
                string sumbitterId = "";
                string submitterCompanyName = "";
                string submitterContactPerson = "";
                string submitterCompanyEmail = "";
                string submitterCompanyPhone = "";
                string batchId = "";
                long subId = 0;

                errorList = new List<string>();

                List<spGetBatchCompanyDetails_Result> batchCompanyInfo = null;
                List<spGetBatchClaimsInfo_Result> batchClaimInfo = null;
                List<spGetBatchClaimsDiagnosis_Result> batchClaimDiagnosis = null;
                List<spGetBatchClaimsProcedurestest_Result> batchClaimProcedures = null;
                List<spGetBatchClaimsInsurancesInfo_Result> insuraceInfo = null;
                //List<USP_GetReasonId_Result> getReasonId = null;
                //List<Operating_Test> OT = null;
                List<Claim_Insurance> claim_Insurances = null;
                List<SPDataModel> sPDataModels = null;
                ClaimsDataModel claim_Result = null;
                //var response2 = getSpResult(claim_id.ToString(), "P");
                //sPDataModels = getSpResult2(claim_id.ToString(), "P");


                //operatingPhysicianTesting operatingPhysicianTesting = new operatingPhysicianTesting();

                //ClaimIns ci = new ClaimIns();

                List<ClaimSubmissionModel> claimSubmissionInfo = new List<ClaimSubmissionModel>();

                using (var ctx = new NPMDBEntities())
                {
                    batchCompanyInfo = ctx.spGetBatchCompanyDetails(practice_id.ToString()).ToList();
                }

                if (batchCompanyInfo != null && batchCompanyInfo.Count > 0)
                {
                    sumbitterId = batchCompanyInfo[0].Submitter_Id;
                    submitterCompanyName = batchCompanyInfo[0].Company_Name;
                    submitterContactPerson = batchCompanyInfo[0].Contact_Person;
                    submitterCompanyEmail = batchCompanyInfo[0].Company_Email;
                    submitterCompanyPhone = batchCompanyInfo[0].Company_Phone;
                }

                if (string.IsNullOrEmpty(sumbitterId))
                {
                    errorList.Add("Patient Submitter ID is missing.");
                }
                if (string.IsNullOrEmpty(submitterCompanyName))
                {
                    errorList.Add("Company ClearingHouse information is missing.");
                }
                if (string.IsNullOrEmpty(submitterCompanyEmail) && string.IsNullOrEmpty(submitterCompanyPhone))
                {
                    errorList.Add("Submitter Contact Information is Missing.");
                }

                if (errorList.Count == 0)
                {
                    using (var ctx = new NPMDBEntities())
                    {
                        batchClaimInfo = ctx.spGetBatchClaimsInfo(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                        batchClaimDiagnosis = ctx.spGetBatchClaimsDiagnosis(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                        batchClaimProcedures = ctx.spGetBatchClaimsProcedurestest(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                        insuraceInfo = ctx.spGetBatchClaimsInsurancesInfo(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                        claim_Result = GetClaimsData(claim_id.ToString());
                        //getReasonId = ctx.USP_GetReasonId(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                        //OT = ctx.Operating_Test.Where(r => r.claim_no == claim_id).ToList();
                        claim_Insurances = ctx.Claim_Insurance.Where(c => c.Claim_No == claim_id).ToList();
                        var originalclaim = ctx.Claims.Where(x => x.Claim_No == claim_id).FirstOrDefault();
                        //ci.POS = originalclaim.Pos;
                        sPDataModels = getSpResult(claim_id.ToString(), "P").ToList();

                    }
                    int[] billArray = sPDataModels[0].Type_of_Bill.ToString()
                              .ToCharArray()
                              .Select(c => int.Parse(c.ToString()))
                              .ToArray();
                    foreach (var claim in batchClaimInfo)
                    {

                        if (claim.Patient_Id == null)
                        {
                            errorList.Add("Patient identifier is missing. DOS:" + claim.Dos);
                        }
                        else if (claim.Billing_Physician == null)
                        {
                            errorList.Add("Billing Physician identifier is missing. DOS:" + claim.Dos);
                        }


                        IEnumerable<spGetBatchClaimsInsurancesInfo_Result> claimInsurances = (from ins in insuraceInfo
                                                                                              where ins.Claim_No == claim.Claim_No
                                                                                              select ins).ToList();

                        spGetBatchClaimsDiagnosis_Result claimDiagnosis = (from spGetBatchClaimsDiagnosis_Result diag in batchClaimDiagnosis
                                                                           where diag.Claim_No == claim.Claim_No
                                                                           select diag).FirstOrDefault();

                        //USP_GetReasonId_Result reason = (from USP_GetReasonId_Result resId in getReasonId
                        //                                where resId.Appointment_Id == claim.Appointment_Id
                        //                                select resId).FirstOrDefault();

                        IEnumerable<SPDataModel> data = (from SPDataModel resId in sPDataModels
                                                         where resId.clm == claim.Claim_No.ToString()
                                                         select resId).ToList();

                        IEnumerable<spGetBatchClaimsProcedurestest_Result> claimProcedures = (from spGetBatchClaimsProcedurestest_Result proc in batchClaimProcedures
                                                                                              where proc.Claim_No == claim.Claim_No
                                                                                              select proc).ToList();







                        ClaimSubmissionModel claimSubmissionModel = new ClaimSubmissionModel();
                        claimSubmissionModel.claim_No = claim.Claim_No;
                        claimSubmissionModel.claimInfo = claim;
                        claimSubmissionModel.claimInsurance = claimInsurances as List<spGetBatchClaimsInsurancesInfo_Result>;
                        claimSubmissionModel.claimDiagnosis = claimDiagnosis as spGetBatchClaimsDiagnosis_Result;
                        //claimSubmissionModel.getReasonId = reason as USP_GetReasonId_Result;
                        claimSubmissionModel.sPDataModel = data as List<SPDataModel>;
                        claimSubmissionModel.claimProcedures = claimProcedures as List<spGetBatchClaimsProcedurestest_Result>;



                        List<uspGetBatchClaimsProviderPayersDataFromUSP_Result> claimBillingProviderPayerInfo;
                        foreach (var ins in claimInsurances)
                        {
                            if (ins.Insurace_Type.Trim().ToUpper().Equals("P") && ins.Inspayer_Id != null)//primary
                            {

                                using (var ctx = new NPMDBEntities())
                                {
                                    claimBillingProviderPayerInfo = ctx.uspGetBatchClaimsProviderPayersDataFromUSP(ins.Inspayer_Id.ToString(), claim.Claim_No.ToString(), "CLAIM_ID").ToList();

                                    if (claimBillingProviderPayerInfo != null && claimBillingProviderPayerInfo.Count > 0)
                                    {
                                        claimSubmissionModel.claimBillingProviderPayer = claimBillingProviderPayerInfo[0];
                                    }
                                }
                                break;
                            }
                        }

                        /*
                         * Assign Other objects of hospital claim
                         *  
                         * 
                         * */
                        claimSubmissionInfo.Add(claimSubmissionModel);

                    }

                    if (claimSubmissionInfo.Count > 0)
                    {
                        batchId = claimSubmissionInfo[0].claim_No.ToString(); // Temporariy ... will be populated by actual batch id.
                        //using (var ctx = new NPMDBEntities())
                        //{
                        //    subId = Convert.ToInt64(ctx.SP_TableIdGenerator_Test("InterchangeControlNumber").FirstOrDefault());
                        //    var recordToUpdate = ctx.claim_batch_detail_test.Where(c => c.claim_id == claim_id && c.batch_id == batchid).ToList();
                        //    if (recordToUpdate != null)
                        //    {
                        //        foreach (var c in recordToUpdate)
                        //        {
                        //            c.interchangeid = subId;
                        //        }
                        //        ctx.SaveChanges();
                        //    }
                        //}
                        string dateTime_yyMMdd = DateTime.Now.ToString("yyMMdd");
                        string dateTime_yyyyMMdd = DateTime.Now.ToString("yyyyMMdd");
                        string dateTime_HHmm = DateTime.Now.ToString("HHmm");

                        // ISA02 Authorization Information AN 10 - 10 R
                        string authorizationInfo = string.Empty.PadRight(10);// 10 characters

                        //ISA04 Security Information AN 10-10 R
                        string securityInfo = string.Empty.PadRight(10);// 10 characters

                        segmentCount = 0;

                        #region ISA Header
                        // INTERCHANGE CONTROL HEADER
                        strBatchString = "ISA*";
                        strBatchString += "00*" + authorizationInfo + "*00*" + securityInfo + "*ZZ*" + sumbitterId.PadRight(15) + "*ZZ*263923727000000*";
                        strBatchString += dateTime_yyMMdd + "*";
                        strBatchString += dateTime_HHmm + "*";
                        strBatchString += $"^*00501*000000001*0*P*:~";
                        //segmentCount++;
                        //FUNCTIONAL GROUP HEADER
                        strBatchString += "GS*HC*" + sumbitterId + "*263923727*";
                        strBatchString += dateTime_yyyyMMdd + "*";
                        strBatchString += dateTime_HHmm + "*";
                        strBatchString += batchId.ToString() + "*X*005010X223A2~";  //-->005010X223A1 is used for Packet 837i(Instittutional File)-->5010 GS08 Changed from 004010X098A1 to 005010X222 in 5010
                                                                                    // need to send batch_id in GS06 instead of 16290 so that can be traced from 997 response file
                                                                                    //segmentCount++;
                                                                                    //TRANSACTION SET HEADER
                        strBatchString += "ST*837*0001*005010X223A2~";  //-->5010 new element addedd. ST03 Implementation Convention Reference (005010X222)
                        segmentCount++;
                        //BEGINNING OF HIERARCHICAL TRANSACTION
                        strBatchString += $"BHT*0019*00*000000001*";
                        strBatchString += dateTime_yyyyMMdd + "*";
                        strBatchString += dateTime_HHmm + "*";
                        strBatchString += "CH~";
                        segmentCount++;

                        #endregion

                        #region LOOP 1000A (Sumbitter Information)


                        #region Submitter Company Name
                        strBatchString += "NM1*41*2*";  //-->5010 NM103  Increase from 35 - 60
                        strBatchString += submitterCompanyName; // -->5010 NM104  Increase from 25 - 35
                        strBatchString += "*****46*" + sumbitterId;// -->5010 New element added NM112 Name Last or Organization Name 1-60
                        strBatchString += "~";
                        segmentCount++;
                        #endregion

                        #region SUBMITTER EDI CONTACT INFORMATION
                        strBatchString += "PER*IC*";
                        if (!string.IsNullOrEmpty(submitterContactPerson))
                        {
                            strBatchString += submitterContactPerson;
                        }

                        if (!string.IsNullOrEmpty(submitterCompanyPhone))
                        {
                            strBatchString += "*TE*" + submitterCompanyPhone.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim();

                        }
                        if (!string.IsNullOrEmpty(submitterCompanyEmail))
                        {
                            strBatchString += "*EM*" + submitterCompanyEmail;
                        }
                        strBatchString += "~";
                        segmentCount++;
                        #endregion

                        #endregion

                        #region LOOP 1000B (RECEIVER NAME)
                        strBatchString += "NM1*40*2*263923727000000*****46*" + sumbitterId + "~";
                        segmentCount++;
                        #endregion

                        int HL = 1;


                        foreach (var claim in claimSubmissionInfo)
                        {
                            var claimid = claim.claim_No;
                            List<SPDataModel> sPClaimData = new List<SPDataModel>();
                            sPClaimData = claim.sPDataModel.Where(p => p.clm == claimid.ToString()).ToList();
                            long patientId = (long)claim.claimInfo.Patient_Id;
                            long claimId = claim.claimInfo.Claim_No;
                            string DOS = claim.claimInfo.Dos;
                            string patientName = claim.claimInfo.Lname + ", " + claim.claimInfo.Fname;

                            string paperPayerID = "";
                            string Billing_Provider_NPI = "";
                            string TaxonomyCode = "";
                            string FederalTaxID = "";
                            string FederalTaxIDType = "";

                            string box_33_type = "";

                            #region Check If Payer Validation Expires
                            // check if payer validation expires

                            if (claim.claimBillingProviderPayer != null)
                            {
                                if (string.IsNullOrEmpty(claim.claimBillingProviderPayer.Validation_Expiry_Date.ToString()) && claim.claimBillingProviderPayer.Validation_Expiry_Date.ToString() != "01/01/1900")
                                {

                                    string validationExpriyDate = claim.claimBillingProviderPayer.Validation_Expiry_Date.ToString();
                                    DateTime dtExpiry = DateTime.Parse(validationExpriyDate);
                                    DateTime dtToday = new DateTime();

                                    if (DateTime.Compare(dtExpiry, dtToday) >= 0) // expires
                                    {
                                        errorList.Add("VALIDATION EXPIRED : Provider validation with the Payer has been expired.");
                                    }

                                }
                            }
                            #endregion

                            #region Provider NPI/Group NPI on the basis of Box 33 Type . Group or Individual | Federal Tax ID | Box33                         
                            if (claim.claimBillingProviderPayer != null)
                            {
                                if (!string.IsNullOrEmpty(claim.claimBillingProviderPayer.Provider_Identification_Number_Type)
                                    && !string.IsNullOrEmpty(claim.claimBillingProviderPayer.Provider_Identification_Number))
                                {

                                    FederalTaxIDType = claim.claimBillingProviderPayer.Provider_Identification_Number_Type;
                                    FederalTaxID = claim.claimBillingProviderPayer.Provider_Identification_Number;
                                }

                                if (!string.IsNullOrEmpty(claim.claimBillingProviderPayer.Box_33_Type))
                                {
                                    box_33_type = claim.claimBillingProviderPayer.Box_33_Type;
                                }
                            }
                            if (string.IsNullOrEmpty(FederalTaxIDType) || string.IsNullOrEmpty(FederalTaxID))
                            {
                                FederalTaxIDType = claim.claimInfo.Federal_Taxidnumbertype;
                                FederalTaxID = claim.claimInfo.Federal_Taxid;
                            }



                            if (string.IsNullOrEmpty(box_33_type))
                            {
                                switch (FederalTaxIDType)
                                {
                                    case "EIN": // Group
                                        box_33_type = "GROUP";
                                        break;
                                    case "SSN": // Individual
                                        box_33_type = "INDIVIDUAL";
                                        break;
                                }
                            }
                            switch (box_33_type)
                            {
                                case "GROUP": // Group  
                                    if (!string.IsNullOrEmpty(claim.claimInfo.Bl_Group_Npi))
                                    {
                                        Billing_Provider_NPI = claim.claimInfo.Bl_Group_Npi;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimInfo.Grp_Taxonomy_Id))
                                    {
                                        TaxonomyCode = claim.claimInfo.Grp_Taxonomy_Id;
                                    }
                                    break;
                                case "INDIVIDUAL": // Individual
                                    if (!string.IsNullOrEmpty(claim.claimInfo.Bl_Npi))
                                    {
                                        Billing_Provider_NPI = claim.claimInfo.Bl_Npi;
                                    }

                                    if (!string.IsNullOrEmpty(claim.claimInfo.Taxonomy_Code))
                                    {
                                        TaxonomyCode = claim.claimInfo.Taxonomy_Code;
                                    }
                                    break;
                            }
                            #endregion

                            #region LOOP 2000A

                            #region BILLING PROVIDER HIERARCHICAL LEVEL

                            strBatchString += "HL*" + HL + "**";
                            strBatchString += "20*1~";
                            segmentCount++;

                            #endregion

                            #region BILLING PROVIDER SPECIALTY INFORMATION
                            if (sPClaimData != null && sPClaimData.Count > 0)
                            {
                                if (!string.IsNullOrEmpty(sPClaimData[0].Taxonomy_Code))
                                {
                                    strBatchString += "PRV*BI*PXC*" + sPClaimData[0].Taxonomy_Code + "~";
                                    segmentCount++;
                                }
                            }
                            #endregion

                            //#region New: Foreign Currency Information!
                            //strBatchString += "CUR*how many data elements would be inserted in this segment~";
                            //segmentCount++;
                            //#endregion End!

                            #endregion

                            #region LOOP 2010AA (Billing Provider Information)

                            #region Billing Provider Name

                            switch (box_33_type)
                            {
                                case "GROUP": // Group                                                        
                                    if (!string.IsNullOrEmpty(submitterCompanyName))
                                    {

                                        strBatchString += "NM1*85*2*";
                                        strBatchString += submitterCompanyName + "*****XX*";

                                    }
                                    else
                                    {
                                        errorList.Add("2010AA - Billing Provider Organization Name Missing.");
                                    }

                                    if (!string.IsNullOrEmpty(Billing_Provider_NPI))
                                    {
                                        strBatchString += Billing_Provider_NPI;
                                    }
                                    else
                                    {
                                        errorList.Add("2010AA - Billing Provider Group NPI Missing.");
                                    }
                                    break;
                                case "INDIVIDUAL": // Individual  
                                    if (!string.IsNullOrEmpty(claim.claimInfo.Bl_Lname)
                                            && string.IsNullOrEmpty(claim.claimInfo.Bl_Fname))
                                    {

                                        strBatchString += "NM1*85*1*";
                                        strBatchString += claim.claimInfo.Bl_Lname + "*" + claim.claimInfo.Bl_Fname + "*" + claim.claimInfo.Bl_Mi + "***XX*";

                                    }
                                    else
                                    {
                                        errorList.Add("2010AA - Billing Provider Name Missing.");
                                    }

                                    if (!string.IsNullOrEmpty(Billing_Provider_NPI))
                                    {
                                        strBatchString += Billing_Provider_NPI;
                                    }
                                    else
                                    {
                                        errorList.Add("2010AA - Billing Provider Individual NPI Missing.");
                                    }

                                    break;
                            }
                            strBatchString += "~";
                            segmentCount++;

                            #endregion

                            #region BILLING PROVIDER ADDRESS

                            switch (box_33_type)
                            {
                                case "GROUP": // Group                                                                               
                                    if (string.IsNullOrEmpty(claim.claimInfo.Bill_Address_Grp.Trim())
                                            || string.IsNullOrEmpty(claim.claimInfo.Bill_City_Grp.Trim())
                                            || string.IsNullOrEmpty(claim.claimInfo.Bill_State_Grp.Trim())
                                            || string.IsNullOrEmpty(claim.claimInfo.Bill_Zip_Grp.Trim()))
                                    {
                                        errorList.Add("BILLING ADDRESS ! Billing Provider Group Address is Missing.");
                                    }
                                    else
                                    {
                                        strBatchString += "N3*";
                                        strBatchString += claim.claimInfo.Bill_Address_Grp.Trim() + "~";
                                        segmentCount++;
                                        strBatchString += "N4*";
                                        strBatchString += claim.claimInfo.Bill_City_Grp.Trim() + "*";
                                        strBatchString += claim.claimInfo.Bill_State_Grp.Trim() + "*";
                                        if (string.IsNullOrEmpty(claim.claimInfo.Bill_Zip_Grp.Trim()))
                                        {
                                            strBatchString += "     ";
                                        }
                                        else
                                        {
                                            strBatchString += claim.claimInfo.Bill_Zip_Grp.Trim() + "~";
                                        }
                                        segmentCount++;
                                    }
                                    break;
                                case "INDIVIDUAL": // Individual  

                                    if (string.IsNullOrEmpty(claim.claimInfo.Bl_Address.Trim())
                                           || string.IsNullOrEmpty(claim.claimInfo.Bl_City.Trim())
                                           || string.IsNullOrEmpty(claim.claimInfo.Bl_State.Trim())
                                           || string.IsNullOrEmpty(claim.claimInfo.Bl_Zip.Trim()))
                                    {
                                        errorList.Add("BILLING ADDRESS ! Billing Provider Individual Address is Missing.");
                                    }
                                    else
                                    {
                                        strBatchString += "N3*";
                                        strBatchString += claim.claimInfo.Bl_Address.Trim() + "~";
                                        segmentCount++;
                                        strBatchString += "N4*";
                                        strBatchString += claim.claimInfo.Bl_City.Trim() + "*";
                                        strBatchString += claim.claimInfo.Bl_State.Trim() + "*";
                                        if (string.IsNullOrEmpty(claim.claimInfo.Bl_Zip.Trim()))
                                        {
                                            strBatchString += "     ";
                                        }
                                        else
                                        {
                                            strBatchString += claim.claimInfo.Bl_Zip.Trim() + "~";
                                        }
                                        segmentCount++;

                                    }

                                    break;
                            }


                            #endregion

                            #region BILLING PROVIDER Tax Identification
                            // hcfa box 25.. 
                            if (!string.IsNullOrEmpty(FederalTaxIDType) && !string.IsNullOrEmpty(FederalTaxID))
                            {
                                if (FederalTaxIDType.Equals("EIN"))
                                {
                                    strBatchString += "REF*EI*";
                                }
                                else if (FederalTaxIDType.Equals("SSN"))
                                {
                                    strBatchString += "REF*SY*";
                                }
                                strBatchString += FederalTaxID + "~";
                                segmentCount++;
                            }
                            else
                            {
                                errorList.Add("Billing provider federal tax id number/type missing.");
                            }

                            #endregion

                            #region BILLING PROVIDER CONTACT INFORMATION
                            switch (FederalTaxIDType)
                            {
                                case "EIN":
                                    if (!string.IsNullOrEmpty(submitterCompanyName)
                                            && !string.IsNullOrEmpty(claim.claimInfo.Phone_No))
                                    {
                                        strBatchString += "PER*IC*" + submitterCompanyName;
                                        strBatchString += "*TE*" + claim.claimInfo.Phone_No.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim() + "~";
                                        segmentCount++;
                                    }
                                    else
                                    {
                                        errorList.Add("Billing Provider Contact Information Missing.");

                                    }
                                    break;
                                case "SSN":
                                    if (!string.IsNullOrEmpty(claim.claimInfo.Bl_Lname)
                                            && !string.IsNullOrEmpty(claim.claimInfo.Phone_No))
                                    {
                                        strBatchString += "PER*IC*" + claim.claimInfo.Bl_Lname;
                                        strBatchString += "*TE*" + claim.claimInfo.Phone_No.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim() + "~";
                                        segmentCount++;
                                    }
                                    else
                                    {
                                        errorList.Add("Billing Provider Contact Information Missing.");
                                    }
                                    break;
                            }
                            #endregion

                            #endregion

                            #region LOOP 2010AB (PAY-TO ADDRESS NAME)
                            switch (box_33_type)
                            {
                                case "GROUP": // Group                                                                               
                                    if (!string.IsNullOrEmpty(claim.claimInfo.Pay_To_Address_Grp.Trim())
                                            || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_City_Grp.Trim())
                                            || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_State_Grp.Trim())
                                            || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_Zip_Grp.Trim()))
                                    {

                                        if (string.IsNullOrEmpty(claim.claimInfo.Pay_To_Address_Grp.Trim())
                                                || string.IsNullOrEmpty(claim.claimInfo.Pay_To_City_Grp.Trim())
                                                || string.IsNullOrEmpty(claim.claimInfo.Pay_To_State_Grp.Trim()))
                                        {
                                            errorList.Add("2010AB : Pay to Provider Group Address is incomplete.");
                                        }
                                        else
                                        {
                                            switch (FederalTaxIDType)
                                            {
                                                case "EIN":
                                                    strBatchString += "NM1*87*2~";
                                                    segmentCount++;
                                                    break;
                                                case "SSN":
                                                    strBatchString += "NM1*87*1~";
                                                    segmentCount++;
                                                    break;
                                            }

                                            strBatchString += "N3*";
                                            strBatchString += claim.claimInfo.Pay_To_Address_Grp + "~";
                                            segmentCount++;

                                            strBatchString += "N4*";
                                            strBatchString += claim.claimInfo.Pay_To_City_Grp.Trim() + "*";
                                            strBatchString += claim.claimInfo.Pay_To_State_Grp + "*";
                                            if (string.IsNullOrEmpty(claim.claimInfo.Pay_To_Zip_Grp.Trim()))
                                            {
                                                strBatchString += "     ";
                                            }
                                            else
                                            {
                                                strBatchString += claim.claimInfo.Pay_To_Zip_Grp.Trim() + "~";
                                            }
                                            segmentCount++;

                                        }
                                    }
                                    break;
                                case "INDIVIDUAL": // Individual  
                                    if (!string.IsNullOrEmpty(claim.claimInfo.Pay_To_Address.Trim())
                                            || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_City.Trim())
                                            || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_State.Trim())
                                            || !string.IsNullOrEmpty(claim.claimInfo.Pay_To_Zip.Trim()))
                                    {

                                        if (string.IsNullOrEmpty(claim.claimInfo.Pay_To_Address.Trim())
                                                || string.IsNullOrEmpty(claim.claimInfo.Pay_To_City.Trim())
                                                || string.IsNullOrEmpty(claim.claimInfo.Pay_To_State.Trim()))
                                        {
                                            errorList.Add("2010AB : Pay to Provider Individual Address is incomplete");
                                        }
                                        else
                                        {
                                            switch (FederalTaxIDType)
                                            {
                                                case "EIN":
                                                    strBatchString += "NM1*87*2~";
                                                    segmentCount++;
                                                    break;
                                                case "SSN":
                                                    strBatchString += "NM1*87*1~";
                                                    segmentCount++;
                                                    break;
                                            }

                                            strBatchString += "N3*";
                                            strBatchString += claim.claimInfo.Pay_To_Address + "~";
                                            segmentCount++;

                                            strBatchString += "N4*";
                                            strBatchString += claim.claimInfo.Pay_To_City.Trim() + "*";
                                            strBatchString += claim.claimInfo.Pay_To_State + "*";
                                            if (string.IsNullOrEmpty(claim.claimInfo.Pay_To_Zip.Trim()))
                                            {
                                                strBatchString += "     ";
                                            }
                                            else
                                            {
                                                strBatchString += claim.claimInfo.Pay_To_Zip.Trim() + "~";
                                            }
                                            segmentCount++;

                                        }
                                    }
                                    break;
                            }

                            #endregion


                            int P = HL;
                            HL = HL + 1;
                            int CHILD = 0;

                            string SBR02 = "18";


                            //---Extract Primar Secondary and Other Insurance Information before processing-----------
                            spGetBatchClaimsInsurancesInfo_Result primaryIns = null;
                            spGetBatchClaimsInsurancesInfo_Result SecondaryIns = null;
                            spGetBatchClaimsInsurancesInfo_Result otherIns = null;

                            if (claim.claimInsurance != null && claim.claimInsurance.Count > 0)
                            {
                                foreach (var ins in claim.claimInsurance)
                                {
                                    switch (ins.Insurace_Type.ToUpper().Trim())
                                    {
                                        case "P":
                                            primaryIns = ins;
                                            break;
                                        case "S":
                                            SecondaryIns = ins;
                                            break;
                                        case "O":
                                            otherIns = ins;
                                            break;
                                    }
                                }
                            }

                            //--End

                            if (claim.claimInsurance == null || claim.claimInsurance.Count == 0)
                            {
                                errorList.Add("Patient Insurance Information is missing.");
                            }
                            else if (primaryIns == null)
                            {
                                errorList.Add("Patient Primary Insurance Information is missing.");
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(primaryIns.GRelationship)
                           && primaryIns.GRelationship.Trim().ToUpper().Equals("S"))
                                {
                                    primaryIns.Glname = claim.claimInfo.Lname;
                                    primaryIns.Gfname = claim.claimInfo.Fname;
                                    primaryIns.Gmi = claim.claimInfo.Mname;
                                    primaryIns.Gaddress = claim.claimInfo.Address;
                                    primaryIns.Gcity = claim.claimInfo.City;
                                    primaryIns.Gdob = claim.claimInfo.Dob;
                                    primaryIns.Ggender = claim.claimInfo.Gender.ToString();
                                    primaryIns.Gstate = claim.claimInfo.State;
                                    primaryIns.Gzip = claim.claimInfo.Zip;
                                }


                                if (!primaryIns.GRelationship.Trim().ToUpper().Equals("S") && primaryIns.Guarantor_Id == null)
                                {
                                    errorList.Add("Subscriber information is missing.");

                                }
                                if (primaryIns.Inspayer_Id == null)
                                {
                                    errorList.Add("Payer's information is missing.");
                                }

                                if (
                                    !primaryIns.GRelationship.Trim().ToUpper().Equals("S")
                                    && !string.IsNullOrEmpty(primaryIns.GRelationship))
                                {
                                    SBR02 = "";
                                    CHILD = 1;
                                }


                                #region LOOP 2000B

                                #region HL: SUBSCRIBER HIERARCHICAL LEVEL
                                strBatchString += "HL*";
                                strBatchString += HL + "*" + P + "*";
                                strBatchString += "22*" + CHILD + "~";
                                segmentCount++;
                                #endregion

                                #region SBR: SUBSCRIBER INFORMATION
                                strBatchString += "SBR*";
                                if (primaryIns != null)
                                {
                                    strBatchString += "P";
                                }
                                else if (SecondaryIns != null)
                                {
                                    strBatchString += "S";
                                }
                                else if (otherIns != null)
                                {
                                    strBatchString += "T";
                                }
                                strBatchString += "*";
                                string groupNo = "";
                                string planName = "";
                                string payerTypeCode = "";

                                if (!string.IsNullOrEmpty(primaryIns.Group_Number))
                                {
                                    groupNo = primaryIns.Group_Number;
                                }
                                else
                                {
                                    groupNo = "";
                                }





                                if (!string.IsNullOrEmpty(primaryIns.Insgroup_Name) && primaryIns.Insgroup_Name.Equals("MEDICARE"))
                                {
                                    if (!string.IsNullOrEmpty(primaryIns.plan_name) && primaryIns.plan_name.ToUpper().Contains("MEDICARE"))
                                    {
                                        planName = primaryIns.plan_name;
                                    }
                                    else
                                    {
                                        planName = "MEDICARE";
                                    }
                                }
                                else
                                {
                                    planName = primaryIns.plan_name;
                                }

                                // MISSING [To Do]
                                //payerTypeCode = primaryIns.getPayertype_code()
                                payerTypeCode = primaryIns.insurance_type_code;


                                //---------***********************************-------------
                                strBatchString += SBR02 + "*" + groupNo + "*" + planName + "*****" + payerTypeCode + "~";
                                segmentCount++;
                                #endregion

                                #endregion

                                #region LOOP 2000BA (SUBSCRIBER Information)

                                strBatchString += "NM1*IL*1*";
                                if ((string.IsNullOrEmpty(primaryIns.Glname)
                                || string.IsNullOrEmpty(primaryIns.Gfname))
                                && string.IsNullOrEmpty(primaryIns.GRelationship)
                                && !primaryIns.GRelationship.Trim().ToUpper().Equals("S"))
                                {
                                    errorList.Add("Subscriber Last/First Name missing.");
                                }

                                //Entering Subscriber Information if Relationship is SELF-----
                                if (SBR02.Equals("18"))
                                {
                                    if (!isAlphaNumeric(claim.claimInfo.Lname)
                                        || !isAlphaNumeric(claim.claimInfo.Fname)
                                        )
                                    {
                                        errorList.Add("Subscriber Name must be Alpha Numeric.");
                                    }
                                    else
                                    {

                                        strBatchString += claim.claimInfo.Lname + "*"
                                                + claim.claimInfo.Fname + "*"
                                                + claim.claimInfo.Mname + "***MI*"
                                                + primaryIns.Policy_Number.ToUpper() + "~";
                                        segmentCount++;

                                    }

                                    if (string.IsNullOrEmpty(claim.claimInfo.Address)
                                        || string.IsNullOrEmpty(claim.claimInfo.City)
                                         || string.IsNullOrEmpty(claim.claimInfo.State)
                                         || string.IsNullOrEmpty(claim.claimInfo.Zip))
                                    {
                                        errorList.Add("Patient Address is incomplete.");
                                    }
                                    else
                                    {
                                        strBatchString += "N3*" + claim.claimInfo.Address + "~";
                                        segmentCount++;
                                        strBatchString += "N4*" + claim.claimInfo.City + "*"
                                                + claim.claimInfo.State + "*";
                                        strBatchString += (!string.IsNullOrEmpty(claim.claimInfo.Zip) ? claim.claimInfo.Zip : "     ") + "~";
                                        segmentCount++;
                                    }


                                    strBatchString += "DMG*D8*";
                                    if (string.IsNullOrEmpty(claim.claimInfo.Dob))
                                    {
                                        errorList.Add("Patient DOB is missing.");
                                    }
                                    else
                                    {
                                        strBatchString += !string.IsNullOrEmpty(claim.claimInfo.Dob) ? claim.claimInfo.Dob.Split('/')[0] + claim.claimInfo.Dob.Split('/')[1] + claim.claimInfo.Dob.Split('/')[2] : "";
                                        strBatchString += "*";
                                    }
                                    if (string.IsNullOrEmpty(claim.claimInfo.Gender.ToString()))
                                    {
                                        errorList.Add("Patient Gender is missing.");
                                    }
                                    else
                                    {
                                        strBatchString += claim.claimInfo.Gender.ToString();

                                    }
                                    strBatchString += "~";
                                    segmentCount++;
                                } //--END
                                else //---Entering Subscriber Information In case of other than SELF---------
                                {
                                    strBatchString += primaryIns.Glname + "*"
                                            + primaryIns.Gfname + "*"
                                            + primaryIns.Gmi + "***MI*"
                                            + primaryIns.Policy_Number.ToUpper() + "~";
                                    segmentCount++;

                                    if (string.IsNullOrEmpty(primaryIns.Gaddress)
                                       || string.IsNullOrEmpty(primaryIns.Gcity)
                                        || string.IsNullOrEmpty(primaryIns.Gstate)
                                        || string.IsNullOrEmpty(primaryIns.Gzip))
                                    {
                                        errorList.Add("Subscriber Address is incomplete.");
                                    }
                                    else
                                    {
                                        strBatchString += "N3*" + primaryIns.Gaddress + "~";
                                        segmentCount++;
                                        strBatchString += "N4*" + primaryIns.Gcity + "*"
                                                + primaryIns.Gstate + "*";
                                        strBatchString += (string.IsNullOrEmpty(primaryIns.Gzip) ? primaryIns.Gzip : "     ") + "~";
                                        segmentCount++;
                                    }


                                    strBatchString += "DMG*D8*";
                                    if (string.IsNullOrEmpty(primaryIns.Gdob))
                                    {
                                        errorList.Add("Subscriber DOB is missing.");
                                    }
                                    else
                                    {
                                        strBatchString += string.IsNullOrEmpty(primaryIns.Gdob) ? primaryIns.Gdob.Split('/')[0] + primaryIns.Gdob.Split('/')[1] + primaryIns.Gdob.Split('/')[2] : "";
                                        strBatchString += "*";
                                    }

                                    if (string.IsNullOrEmpty(primaryIns.Ggender))
                                    {
                                        errorList.Add("Subscriber Gender is missing.");
                                    }
                                    else
                                    {
                                        strBatchString += primaryIns.Ggender;

                                    }
                                    strBatchString += "~";
                                    segmentCount++;
                                }

                                #endregion
                                #region LOOP 2010BA (SUBSCRIBER SECONDARY IDENTIFICATION)
                                if (sPClaimData != null && sPClaimData.Count > 0 && !string.IsNullOrEmpty(primaryIns.Policy_Number))
                                {

                                    if (string.IsNullOrEmpty(sPClaimData[0].SSN?.ToString()))
                                    {
                                        //errorList.Add("SUBSCRIBER SECONDARY IDENTIFICATION SSN MISSING.");
                                    }
                                    else
                                    {

                                        strBatchString += "REF*SY*" + sPClaimData[0].SSN + "~";
                                        segmentCount++;
                                    }

                                }
                                #endregion

                                #region LOOP 2010BB (PAYER INFORMATION)

                                if (string.IsNullOrEmpty(primaryIns.plan_name))
                                {
                                    errorList.Add("Payer name missing.");

                                }
                                string paperPayerName = "";
                                if (!string.IsNullOrEmpty(primaryIns.plan_name) && primaryIns.plan_name.Trim().ToUpper().Equals("MEDICARE"))
                                {
                                    paperPayerName = "MEDICARE";
                                }
                                else
                                {
                                    paperPayerName = primaryIns.plan_name;
                                }

                                paperPayerID = primaryIns.Payer_Number;
                                if (!string.IsNullOrEmpty(paperPayerID))
                                {
                                    strBatchString += "NM1*PR*2*" + paperPayerName + "*****PI*" + paperPayerID + "~";
                                    segmentCount++;
                                }
                                else
                                {
                                    errorList.Add("Payer id is compulsory in case of Gateway EDI Clearing house.");
                                }

                                #region LOOP 2010BB (PAYER ADDRESS)

                                if (!string.IsNullOrEmpty(primaryIns.Insgroup_Name) && primaryIns.plan_name.Trim().ToUpper().Equals("WORK COMP"))
                                {
                                    if (string.IsNullOrEmpty(primaryIns.Sub_Empaddress)
                                            || string.IsNullOrEmpty(primaryIns.Sub_Emp_City)
                                            || string.IsNullOrEmpty(primaryIns.Sub_Emp_State)
                                            || string.IsNullOrEmpty(primaryIns.Sub_Emp_Zip))
                                    {
                                        errorList.Add("Payer is Worker Company, so its subscriber employer’s address is necessary.");

                                    }
                                    strBatchString += "N3*" + primaryIns.Sub_Empaddress + "~";
                                    segmentCount++;

                                    strBatchString += "N4*" + primaryIns.Sub_Emp_City + "*"
                                            + primaryIns.Sub_Emp_State + "*";
                                    if (!string.IsNullOrEmpty(primaryIns.Sub_Emp_Zip))
                                    {
                                        strBatchString += primaryIns.Sub_Emp_Zip;

                                    }
                                    else
                                    {
                                        strBatchString += "     ";
                                    }
                                    strBatchString += "~";
                                    segmentCount++;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(primaryIns.Ins_Address)
                                            || string.IsNullOrEmpty(primaryIns.Ins_City)
                                            || string.IsNullOrEmpty(primaryIns.Ins_State)
                                            || string.IsNullOrEmpty(primaryIns.Ins_Zip))
                                    {
                                        errorList.Add("Payer address incomplete.");
                                    }
                                    strBatchString += "N3*" + primaryIns.Ins_Address;
                                    strBatchString += "~";
                                    segmentCount++;

                                    strBatchString += "N4*" + primaryIns.Ins_City + "*" + primaryIns.Ins_State + "*";
                                    strBatchString += (string.IsNullOrEmpty(primaryIns.Ins_Zip)) ? "     " : primaryIns.Ins_Zip.Trim();
                                    strBatchString += "~";
                                    segmentCount++;
                                }



                                //if(claim.sPDataModel != null && claim.sPDataModel.Count > 0)
                                //{
                                //    foreach (var data in claim.sPDataModel)
                                //    {
                                //        if (!string.IsNullOrEmpty(data.INSURANCE_ADDRESS))
                                //        {
                                //            strBatchString += "N3*" + data.INSURANCE_ADDRESS + "~";
                                //            segmentCount++;
                                //        }


                                //        #region LOOP 2010BB (PAYER CITY, STATE, ZIP CODE)
                                //        if (!string.IsNullOrEmpty(data.INSURANCE_CITY))
                                //        {
                                //            strBatchString += "N4*" + data.INSURANCE_CITY + "*"
                                //                                + data.INSURANCE_STATE + "*"
                                //                                + data.INSURANCE_ZIP + "*";
                                //            segmentCount++;
                                //        }
                                //    }
                                //}
                                //#endregion
                                #endregion

                                #region REF BILLING PROVIDER SECONDARY IDENTIFICATION!

                                //strBatchString += "REF*G2*" + primaryIns.Sub_Empaddress + "~";
                                //segmentCount++;

                                #endregion End!

                                #region RemoveD Segments N3 and N4

                                //if (!string.IsNullOrEmpty(primaryIns.Insgroup_Name) && primaryIns.plan_name.Trim().ToUpper().Equals("WORK COMP"))
                                //{
                                //    if (string.IsNullOrEmpty(primaryIns.Sub_Empaddress)
                                //            || string.IsNullOrEmpty(primaryIns.Sub_Emp_City)
                                //            || string.IsNullOrEmpty(primaryIns.Sub_Emp_State)
                                //            || string.IsNullOrEmpty(primaryIns.Sub_Emp_Zip))
                                //    {
                                //        errorList.Add("Payer is Worker Company, so its subscriber employer’s address is necessary.");

                                //    }
                                //    strBatchString += "N3*" + primaryIns.Sub_Empaddress + "~";
                                //    segmentCount++;

                                //    strBatchString += "N4*" + primaryIns.Sub_Emp_City + "*"
                                //            + primaryIns.Sub_Emp_State + "*";
                                //    if (!string.IsNullOrEmpty(primaryIns.Sub_Emp_Zip))
                                //    {
                                //        strBatchString += primaryIns.Sub_Emp_Zip;

                                //    }
                                //    else
                                //    {
                                //        strBatchString += "     ";
                                //    }
                                //    strBatchString += "~";
                                //    segmentCount++;
                                //}
                                //else
                                //{
                                //    if (string.IsNullOrEmpty(primaryIns.Ins_Address)
                                //            || string.IsNullOrEmpty(primaryIns.Ins_City)
                                //            || string.IsNullOrEmpty(primaryIns.Ins_State)
                                //            || string.IsNullOrEmpty(primaryIns.Ins_Zip))
                                //    {
                                //        errorList.Add("Payer address incomplete.");
                                //    }
                                //    strBatchString += "N3*" + primaryIns.Ins_Address;
                                //    strBatchString += "~";
                                //    segmentCount++;

                                //    strBatchString += "N4*" + primaryIns.Ins_City + "*" + primaryIns.Ins_State + "*";
                                //    strBatchString += (string.IsNullOrEmpty(primaryIns.Ins_Zip)) ? "     " : primaryIns.Ins_Zip.Trim();
                                //    strBatchString += "~";
                                //    segmentCount++;
                                //}

                                #endregion End!

                                #endregion

                                #region LOOP 2000C (PATIENT HIERARCHICAL LEVEL)

                                if (!string.IsNullOrEmpty(primaryIns.GRelationship)
                                   && !primaryIns.GRelationship.ToUpper().Trim().Equals("S"))
                                {

                                    #region LOOP 2000C

                                    #region HL : (PATIENT HIERARCHICAL LEVEL)
                                    int PHL = HL;
                                    HL++;
                                    strBatchString += "HL*" + HL + "*" + PHL + "*23*0~";
                                    segmentCount++;
                                    #endregion


                                    #region PAT : (PATIENT RELATIONAL INFORMATION)
                                    strBatchString += "PAT*";
                                    String temp = "";
                                    if (string.IsNullOrEmpty(primaryIns.GRelationship))
                                    {
                                        errorList.Add("Subscriber relationship is missing.");
                                    }
                                    else
                                    {
                                        if (primaryIns.GRelationship.Trim().ToUpper().Equals("S"))
                                        {
                                            temp = "18";
                                        }
                                        else if (primaryIns.GRelationship.Trim().ToUpper().Equals("P"))
                                        {
                                            temp = "01";
                                        }
                                        else if (primaryIns.GRelationship.Trim().ToUpper().Equals("C"))
                                        {
                                            temp = "19";
                                        }
                                        else if (primaryIns.GRelationship.Trim().ToUpper().Equals("O"))
                                        {
                                            temp = "G8";
                                        }
                                    }

                                    strBatchString += temp + "****D8***~";
                                    segmentCount++;
                                    #endregion

                                    #endregion


                                    #region LOOP 2010CA

                                    #region PATIENT NAME INFORMATION
                                    strBatchString += "NM1*QC*1*";

                                    //----ENTERING PATIENT INFORMATION NOW------------
                                    strBatchString += claim.claimInfo.Lname + "*";
                                    strBatchString += claim.claimInfo.Fname + "*";
                                    strBatchString += claim.claimInfo.Mname + "***MI*";
                                    if (string.IsNullOrEmpty(primaryIns.Policy_Number))
                                    {
                                        errorList.Add("Subscriber policy number  missing.");
                                    }
                                    strBatchString += primaryIns.Policy_Number.ToUpper() + "~";
                                    segmentCount++;
                                    strBatchString += "N3*" + claim.claimInfo.Address.Trim() + "~";
                                    segmentCount++;
                                    strBatchString += "N4*" + claim.claimInfo.City.Trim() + "*" + claim.claimInfo.State.Trim() + "*"
                                            + claim.claimInfo.Zip.Trim() + "~";
                                    segmentCount++;

                                    if (string.IsNullOrEmpty(claim.claimInfo.Gender.ToString()))
                                    {
                                        errorList.Add("Patient gender missing.");
                                    }

                                    strBatchString += "DMG*D8*" + claim.claimInfo.Dob.Split('/')[0] + claim.claimInfo.Dob.Split('/')[1] + claim.claimInfo.Dob.Split('/')[2] + "*" + claim.claimInfo.Gender.ToString() + "~";
                                    segmentCount++;
                                    #endregion

                                    #endregion

                                }

                                #endregion

                                //HL++;

                                #region LOOP 2300
                                strBatchString += "CLM*" + claim.claim_No + "*";

                                decimal total_amount = 0;

                                if (claim.claimInfo.Is_Resubmitted)
                                {
                                    foreach (var proc in claim.claimProcedures)
                                    {
                                        if (proc.Is_Resubmitted)
                                        {
                                            total_amount = total_amount + (decimal)proc.Total_Charges;
                                        }
                                    }

                                }
                                else
                                {
                                    total_amount = claim.claimInfo.Claim_Total;
                                }


                                //string ClaimFrequencyCode = (bool)claim.claimInfo.Is_Corrected ? claim.claimInfo.RSCode.ToString() : "1";
                                string PatFirstVisitDatesegmentCount = "";

                                strBatchString += string.Format("{0:0.00}", total_amount) + "***" + billArray[0] + billArray[1] + ":A:" + billArray[2] + "**A*Y*Y~"; // 5010
                                segmentCount++;
                                #region LOOP 2300 (DATE - DISCHARGE)
                                int isErrorInAccident = 0;
                                if (sPClaimData != null && sPClaimData.Count > 0)
                                {

                                    if (!string.IsNullOrEmpty(claim.sPDataModel[0].HOSPital_FROM.ToString()) && !claim.sPDataModel[0].HOSPital_FROM.Equals(DateTime.Parse("1900/01/01")))
                                    {
                                        if (claim.sPDataModel[0].Dischargehour.Length == 1)
                                        {
                                            claim.sPDataModel[0].Dischargehour = "0" + claim.sPDataModel[0].Dischargehour;
                                        }
                                        strBatchString += $"DTP*096*TM*{claim.sPDataModel[0].Dischargehour}00~";
                                        segmentCount++;
                                    }
                                    #region LOOP 2300 (STATEMENT DATES)
                                    if (!string.IsNullOrEmpty(sPClaimData[0].HOSPital_FROM.ToString()) && !sPClaimData[0].HOSPital_FROM.Equals(DateTime.Parse("1900/01/01")) &&
                                            !string.IsNullOrEmpty(sPClaimData[0].HOSPital_TO.ToString()) && !sPClaimData[0].HOSPital_TO.Equals(DateTime.Parse("1900/01/01")))
                                    {
                                        string[] spltdHospitalFromDate = claim.claimInfo.Hospital_From.Split('/');
                                        string[] spltdHospitalToDate = claim.claimInfo.Hospital_To.Split('/');
                                        string hospitalFromDate = "";
                                        string hospitalToDate = "";

                                        if (spltdHospitalFromDate.Length == 3)
                                        {
                                            hospitalFromDate = spltdHospitalFromDate[0] + spltdHospitalFromDate[1] + spltdHospitalFromDate[2];
                                        }

                                        if (spltdHospitalToDate.Length == 3)
                                        {
                                            hospitalToDate = spltdHospitalToDate[0] + spltdHospitalToDate[1] + spltdHospitalToDate[2];
                                        }

                                        if (!string.IsNullOrEmpty(hospitalFromDate) && !string.IsNullOrEmpty(hospitalToDate))
                                        {
                                            strBatchString += "DTP*434*RD8*" + hospitalFromDate + "-" + hospitalToDate + "~";
                                        }
                                        else
                                        {
                                            isErrorInAccident = 3;
                                        }
                                        segmentCount++;
                                    }
                                    #endregion
                                }
                                #endregion

                                #region Accident Info
                                isErrorInAccident = 0;

                                if (!string.IsNullOrEmpty(claim.claimInfo.Accident_Type))
                                {

                                    switch (claim.claimInfo.Accident_Type.ToUpper())
                                    {
                                        case "OA":
                                            strBatchString += "*OA";
                                            break;
                                        case "AA":
                                            strBatchString += "*AA";
                                            break;
                                        case "EM":
                                            strBatchString += "*EM";
                                            break;
                                        default:
                                            isErrorInAccident = 1;
                                            break;
                                    }


                                    if (isErrorInAccident == 0)
                                    {
                                        if (!string.IsNullOrEmpty(claim.claimInfo.Accident_State))
                                        {
                                            strBatchString += ":::" + claim.claimInfo.Accident_State + "~";
                                            segmentCount++;
                                        }
                                        else
                                        {
                                            if (claim.claimInfo.Accident_Type.ToUpper().Equals("OA")
                                                || claim.claimInfo.Accident_Type.ToUpper().Equals("EM"))
                                            {
                                                strBatchString += "~";
                                                segmentCount++;
                                            }
                                            else
                                            {
                                                isErrorInAccident = 2;
                                            }
                                        }

                                        if (isErrorInAccident == 0)
                                        {
                                            #region DATE  ACCIDENT (change segment from dtp*439*D8 to dtp*434*RD8)
                                            strBatchString += "DTP*434*RD8*";
                                            if (!string.IsNullOrEmpty(claim.claimInfo.Accident_Date) && !claim.claimInfo.Accident_Date.Equals("1900/01/01"))
                                            {
                                                string[] splitedAccidentDate = claim.claimInfo.Accident_Date.Split('/');
                                                if (splitedAccidentDate.Count() != 3)
                                                {
                                                    isErrorInAccident = 3;
                                                }
                                                strBatchString += splitedAccidentDate[0] + splitedAccidentDate[1] + splitedAccidentDate[2] + "~";
                                                segmentCount++;
                                            }
                                            else
                                            {
                                                isErrorInAccident = 4;
                                            }

                                            #endregion
                                        }
                                    }
                                }
                                //else
                                //{
                                //    strBatchString += "~";
                                //    segmentCount++;
                                //}
                                #endregion

                                #region DATE - ADMISSION (HOSPITALIZATION)


                                if (!string.IsNullOrEmpty(claim.claimInfo.Hospital_From) && !claim.claimInfo.Hospital_From.Equals("1900/01/01"))
                                {
                                    //string originalDateString = claim.claimInfo.Hospital_From; // Replace this with your actual string
                                    //DateTime parsedDateTime;
                                    //string desiredFormat = "yyyyMMddHHmm";
                                    //if (DateTime.TryParse(originalDateString, out parsedDateTime))
                                    //{
                                    //    // Format the DateTime object into the desired format
                                    //    string formattedDateTime = parsedDateTime.ToString(desiredFormat);
                                    //    strBatchString += "DTP*435*D8*" + formattedDateTime + "~";
                                    //}
                                    //string[] spltdHospitalFromDate = claim.claimInfo.Hospital_From.Split('/');
                                    //if (spltdHospitalFromDate.Count() != 3)
                                    //{
                                    //    isErrorInAccident = 3;
                                    //}
                                    //string hospitalFromDate = spltdHospitalFromDate[0] + spltdHospitalFromDate[1] + spltdHospitalFromDate[2];
                                    //strBatchString += "DTP*435*D8*" + formattedDateTime + "~";
                                    //segmentCount++;

                                    string[] spltdHospitalFromDate = claim.claimInfo.Hospital_From.Split('/');
                                    if (spltdHospitalFromDate.Count() != 3)
                                    {
                                        isErrorInAccident = 3;
                                    }
                                    string hospitalFromDate = spltdHospitalFromDate[0] + spltdHospitalFromDate[1] + spltdHospitalFromDate[2];
                                    if (claim.sPDataModel[0].Admhour.Length == 1)
                                    {
                                        claim.sPDataModel[0].Admhour = "0" + claim.sPDataModel[0].Admhour;
                                    }
                                    strBatchString += "DTP*435*DT*" + hospitalFromDate + claim.sPDataModel[0].Admhour + "00" + "~";
                                    segmentCount++;
                                }

                                #endregion

                                #region New:CL1 INSTITUTIONAL CLAIM CODE
                                if (sPClaimData != null && sPClaimData.Count > 0)
                                {
                                    strBatchString += $"CL1*";
                                    strBatchString += sPDataModels[0].Type_Of_Admission_Id;
                                    strBatchString += $"*{sPDataModels[0].AdmSource}*";
                                    if (claim.sPDataModel[0].Discharge_status_Id.Length == 1)
                                    {
                                        strBatchString += $"0{sPDataModels[0].Discharge_status_Id}";
                                    }
                                    else
                                        strBatchString += $"{sPDataModels[0].Discharge_status_Id}";
                                    strBatchString += "~";
                                    segmentCount++;
                                }

                                //if (ci.POS != null)
                                //{
                                //    foreach (var i in claim_Insurances)
                                //    {
                                //        strBatchString += $"CL1*";
                                //        if (ci.POS.Equals("21") || ci.POS.Equals("22") || ci.POS.Equals("23") || ci.POS.Equals("24"))
                                //        {
                                //            ci.Admission_Type_Code = i.Admission_Type_Code;
                                //            strBatchString += $"{ci.Admission_Type_Code}*";
                                //        }
                                //        if (!ci.POS.Equals("21") || !ci.POS.Equals("22") || !ci.POS.Equals("23") || !ci.POS.Equals("24"))
                                //        {
                                //            strBatchString += $"*";
                                //        }
                                //        ci.Admission_Source_Code = i.Admission_Source_Code;
                                //        ci.Patient_Status_Code = i.Patient_Status_Code;
                                //        strBatchString += $"{ci.Admission_Source_Code}*{ci.Patient_Status_Code}~";
                                //        segmentCount++;
                                //        break;
                                //    }
                                //}
                                #endregion End!

                                #region (REF - OTHER PAYER CLAIM CONTROL NUMBER)
                                if (sPClaimData != null && sPClaimData.Count > 0)
                                {
                                    if (!string.IsNullOrEmpty(sPClaimData[0].ICN))
                                    {
                                        strBatchString += "REF*F8*";
                                        strBatchString += sPClaimData[0].PATIENT_ACCOUNT;
                                        strBatchString += "~";
                                        segmentCount++;
                                    }
                                }
                                #endregion End!

                                #region LOOP 2300 (MEDICAL RECORD NUMBER)
                                if (sPClaimData != null && sPClaimData.Count > 0)
                                {

                                    if (!string.IsNullOrEmpty(sPClaimData[0].Pat_Acc.ToString()))
                                    {
                                        strBatchString += "REF*EA*" + sPClaimData[0].clm + "~";
                                        segmentCount++;
                                    }

                                }
                                #endregion

                                #region Removed:DATE - INITIAL TREATMENT
                                //if (!string.IsNullOrEmpty(PatFirstVisitDatesegmentCount))
                                //{
                                //    strBatchString += PatFirstVisitDatesegmentCount;
                                //    segmentCount++;
                                //}

                                #endregion

                                #region Removed:DATE -  Last X-Ray Date

                                //if (!string.IsNullOrEmpty(claim.claimInfo.Last_Xray_Date) && !claim.claimInfo.Last_Xray_Date.Equals("1900/01/01"))
                                //{
                                //    string[] spltdlastXrayDate = claim.claimInfo.Last_Xray_Date.Split('/');
                                //    string LastXrayDate = spltdlastXrayDate[0] + spltdlastXrayDate[1] + spltdlastXrayDate[2];
                                //    strBatchString += "DTP*455*D8*" + LastXrayDate + "~";
                                //    segmentCount++;
                                //}

                                #endregion

                                #region Removed:DATE - ADMISSION (HOSPITALIZATION)


                                //if (!string.IsNullOrEmpty(claim.claimInfo.Hospital_From) && !claim.claimInfo.Hospital_From.Equals("1900/01/01"))
                                //{
                                //    string[] spltdHospitalFromDate = claim.claimInfo.Hospital_From.Split('/');
                                //    if (spltdHospitalFromDate.Count() != 3)
                                //    {
                                //        isErrorInAccident = 3;
                                //    }
                                //    string hospitalFromDate = spltdHospitalFromDate[0] + spltdHospitalFromDate[1] + spltdHospitalFromDate[2];
                                //    strBatchString += "DTP*435*D8*" + hospitalFromDate + "~";
                                //    segmentCount++;
                                //}

                                #endregion

                                #region Removed: Error Checking!
                                //if (isErrorInAccident >= 1)
                                //{
                                //    if (isErrorInAccident == 1)
                                //    {
                                //        errorList.Add("Accident Type is missing.");
                                //    }
                                //    else if (isErrorInAccident == 2)
                                //    {
                                //        errorList.Add("State of accident is necessary.");
                                //    }
                                //    else if (isErrorInAccident == 3)
                                //    {
                                //        errorList.Add("Format of date of accident is not correct.");
                                //    }
                                //    else if (isErrorInAccident == 4)
                                //    {
                                //        errorList.Add("Date of accident is missing.");
                                //    }
                                //}
                                #endregion End!

                                #region Removed:PRIOR AUTHORIZATION
                                //if (!string.IsNullOrEmpty(claim.claimInfo.Prior_Authorization))
                                //{
                                //    strBatchString += "REF*G1*" + claim.claimInfo.Prior_Authorization + "~";
                                //    segmentCount++;
                                //}
                                #endregion

                                #region Removed:PAYER CLAIM CONTROL NUMBER
                                //if (!string.IsNullOrEmpty(claim.claimInfo.Claim_Number))
                                //{
                                //    strBatchString += "REF*F8*" + claim.claimInfo.Claim_Number + "~";
                                //    segmentCount++;
                                //}
                                #endregion

                                #region Removed:CLINICAL LABORATORY IMPROVEMENT AMENDMENT (CLIA) NUMBER
                                //if (!string.IsNullOrEmpty(claim.claimInfo.Clia_Number))
                                //{
                                //    strBatchString += "REF*X4*" + claim.claimInfo.Clia_Number + "~";
                                //    segmentCount++;
                                //}
                                #endregion

                                #region Removed:CLAIM NOTE (LUO)
                                //if (!string.IsNullOrEmpty(claim.claimInfo.Luo))
                                //{
                                //    strBatchString += "NTE*ADD*" + claim.claimInfo.Luo + "~";
                                //    segmentCount++;
                                //}
                                #endregion

                                #region Removed:HEALTH CARE DIAGNOSIS CODE

                                //strBatchString += "HI*";

                                //// ICD-10 Claim
                                //if ((bool)claim.claimInfo.Icd_10_Claim)
                                //{
                                //    strBatchString += "ABK:";  // BK=ICD-9 ABK=ICD-10
                                //}
                                //else // ICD-9 Claim
                                //{
                                //    strBatchString += "BK:";  // BK=ICD-9 ABK=ICD-10 
                                //}

                                ////Adding claim ICDS Diagnosis COdes
                                //int diagCount = 0;
                                //if (claim.claimDiagnosis != null)
                                //{
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code1))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code1);
                                //        diagCount++;
                                //    }

                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code2))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code2);
                                //        diagCount++;
                                //    }

                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code3))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code3);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code4))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code4);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code5))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code5);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code6))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code6);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code7))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code7);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code8))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code8);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code9))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code9);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code10))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code10);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code11))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code11);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code12))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code12);
                                //        diagCount++;
                                //    }
                                //}
                                //if (diagCount == 0)
                                //{
                                //    if ((bool)claim.claimInfo.Icd_10_Claim)
                                //    {
                                //        errorList.Add("HI*ABK:ABF!Claims Diagnosis (ICD-10) are missing.");
                                //    }
                                //    else
                                //    {
                                //        errorList.Add("HI*BK:BF!Claims Diagnosis (ICD-9) are missing.");
                                //    }


                                //}
                                //strBatchString += "~";
                                //segmentCount++;


                                #endregion

                                #region New:HI PRINCIPAL DIAGNOSIS CODES
                                //strBatchString += "HI*BK:";
                                strBatchString += "HI*";

                                // ICD-10 Claim
                                if ((bool)claim.claimInfo.Icd_10_Claim)
                                {
                                    strBatchString += "ABK:";  // BK=ICD-9 ABK=ICD-10
                                }
                                else // ICD-9 Claim
                                {
                                    strBatchString += "BK:";  // BK=ICD-9 ABK=ICD-10 
                                }
                                //Adding claim ICDS Diagnosis COdes
                                int diagCount = 0;
                                if (claim.claimDiagnosis != null)
                                {
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code1))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code1);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code2))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code2);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code3))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code3);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code4))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code4);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code5))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code5);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code6))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code6);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code7))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code7);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code8))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code8);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code9))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code9);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code10))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code10);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code11))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code11);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code12))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code12);
                                        diagCount++;
                                    }
                                }
                                if (diagCount == 0)
                                {
                                    if ((bool)claim.claimInfo.Icd_10_Claim)
                                    {
                                        errorList.Add("HI*ABK:ABF!Claims Diagnosis (ICD-10) are missing.");
                                    }
                                    else
                                    {
                                        errorList.Add("HI*BK:BF!Claims Diagnosis (ICD-9) are missing.");
                                    }


                                }
                                strBatchString += "~";
                                segmentCount++;
                                #endregion End!

                                #region NEW:HI ADMITTING DIAGNOSIS CODE
                                strBatchString += "HI*";
                                // ICD-10 Claim
                                if ((bool)claim.claimInfo.Icd_10_Claim)
                                {
                                    strBatchString += "ABJ:";  // BJ=ICD-9 ABJ=ICD-10
                                }
                                else // ICD-9 Claim
                                {
                                    strBatchString += "BJ:";  // BJ=ICD-9 ABJ=ICD-10 
                                }
                                diagCount = 0;
                                if (claim.claimDiagnosis != null)
                                {
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code1))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code1);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code2))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code2);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code3))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code3);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code4))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code4);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code5))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code5);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code6))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code6);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code7))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code7);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code8))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code8);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code9))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code9);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code10))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code10);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code11))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code11);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code12))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code12);
                                        diagCount++;
                                    }
                                }
                                if (diagCount == 0)
                                {
                                    if ((bool)claim.claimInfo.Icd_10_Claim)
                                    {
                                        errorList.Add("HI*ABK:ABF!Claims Diagnosis (ICD-10) are missing.");
                                    }
                                    else
                                    {
                                        errorList.Add("HI*BK:BF!Claims Diagnosis (ICD-9) are missing.");
                                    }


                                }
                                strBatchString += "~";
                                segmentCount++;
                                #endregion End!

                                #region NEW:HI Reason For Visit
                                //strBatchString += "HI*BK:";
                                strBatchString += "HI*";

                                // ICD-10 Claim
                                if ((bool)claim.claimInfo.Icd_10_Claim)
                                {
                                    strBatchString += "APR:";  // BK=ICD-9 ABK=ICD-10
                                }
                                else // ICD-9 Claim
                                {
                                    strBatchString += "PR:";  // BK=ICD-9 ABK=ICD-10 
                                }
                                //Adding claim ICDS Diagnosis COdes
                                diagCount = 0;
                                if (claim.claimDiagnosis != null)
                                {
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code1))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code1);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code2))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code2);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code3))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code3);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code4))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code4);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code5))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code5);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code6))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code6);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code7))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code7);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code8))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code8);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code9))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code9);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code10))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code10);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code11))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code11);
                                        diagCount++;
                                    }
                                    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code12))
                                    {
                                        strBatchString += appendDxCodesegmentCount837I(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code12);
                                        diagCount++;
                                    }
                                }
                                if (diagCount == 0)
                                {
                                    if ((bool)claim.claimInfo.Icd_10_Claim)
                                    {
                                        errorList.Add("HI*ABK:ABF!Claims Diagnosis (ICD-10) are missing.");
                                    }
                                    else
                                    {
                                        errorList.Add("HI*BK:BF!Claims Diagnosis (ICD-9) are missing.");
                                    }
                                }
                                strBatchString += "~";
                                segmentCount++;
                                #endregion End!

                                #region NEW:External Cause of Injury
                                //strBatchString += "HI*";
                                //// ICD-10 Claim
                                //if ((bool)claim.claimInfo.Icd_10_Claim)
                                //{
                                //    strBatchString += "AB:";  // PR=ICD-9 APR=ICD-10
                                //}
                                //else // ICD-9 Claim
                                //{
                                //    strBatchString += "BN:";  // PR=ICD-9 APR=ICD-10 
                                //}
                                //diagCount = 0;
                                //if (claim.claimDiagnosis != null)
                                //{
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code1))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code1);
                                //        diagCount++;
                                //    }

                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code2))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code2);
                                //        diagCount++;
                                //    }

                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code3))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code3);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code4))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code4);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code5))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code5);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code6))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code6);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code7))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code7);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code8))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code8);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code9))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code9);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code10))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code10);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code11))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code11);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code12))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code12);
                                //        diagCount++;
                                //    }
                                //}
                                //if (diagCount == 0)
                                //{
                                //    if ((bool)claim.claimInfo.Icd_10_Claim)
                                //    {
                                //        errorList.Add("HI*ABK:ABF!Claims Diagnosis (ICD-10) are missing.");
                                //    }
                                //    else
                                //    {
                                //        errorList.Add("HI*BK:BF!Claims Diagnosis (ICD-9) are missing.");
                                //    }


                                //}

                                //strBatchString += "~";
                                //segmentCount++;
                                #endregion End!

                                #region New:HI OTHER DIAGNOSIS CODE INFORMATION
                                ////strBatchString += "HI*";
                                ////strBatchString += "HI*BK:";
                                //strBatchString += "HI*";

                                //// ICD-10 Claim
                                //if ((bool)claim.claimInfo.Icd_10_Claim)
                                //{
                                //    strBatchString += "AB:";  // BF=ICD-9 AB=ICD-10
                                //}
                                //else // ICD-9 Claim
                                //{
                                //    strBatchString += "BF:";  // BF=ICD-9 AB=ICD-10 
                                //}
                                //diagCount = 0;
                                //if (claim.claimDiagnosis != null)
                                //{
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code1))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code1);
                                //        diagCount++;
                                //    }

                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code2))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code2);
                                //        diagCount++;
                                //    }

                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code3))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code3);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code4))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code4);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code5))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code5);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code6))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code6);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code7))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code7);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code8))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code8);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code9))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code9);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code10))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code10);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code11))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code11);
                                //        diagCount++;
                                //    }
                                //    if (!string.IsNullOrEmpty(claim.claimDiagnosis.DX_Code12))
                                //    {
                                //        strBatchString += appendDxCodesegmentCount(diagCount, (bool)claim.claimInfo.Icd_10_Claim, claim.claimDiagnosis.DX_Code12);
                                //        diagCount++;
                                //    }
                                //}
                                //if (diagCount == 0)
                                //{
                                //    if ((bool)claim.claimInfo.Icd_10_Claim)
                                //    {
                                //        errorList.Add("HI*ABK:ABF!Claims Diagnosis (ICD-10) are missing.");
                                //    }
                                //    else
                                //    {
                                //        errorList.Add("HI*BK:BF!Claims Diagnosis (ICD-9) are missing.");
                                //    }


                                //}
                                //strBatchString += "~";
                                //segmentCount++;
                                #endregion End!

                                #region New:HI Principal Procedure Information
                                if (sPDataModels[0].Type_of_Bill == 111)
                                {
                                    DateTime dosFrom;
                                    strBatchString += "HI*";
                                    if ((bool)claim.claimInfo.Icd_10_Claim)
                                    {
                                        strBatchString += "BBR:";
                                    }
                                    else
                                    {
                                        strBatchString += "BR:";
                                    }

                                    if (claim.claimProcedures != null)
                                    {
                                        strBatchString += sPDataModels[0].PROCEDURE_CODE;
                                        strBatchString += ":" + "D8:";
                                        DateTime.TryParse(sPDataModels[0].DOS_FROM.ToString(), out dosFrom);
                                        strBatchString += dosFrom.ToString("yyyyMMdd");
                                    }
                                    strBatchString += "~";
                                    segmentCount++;
                                }
                                #endregion End!

                                #region New:HI OTHER PROCEDURE INFORMATRION
                                //////strBatchString += "HI*";
                                //////strBatchString += "HI*BK:";
                                ////strBatchString += "HI*";
                                ////// ICD-10 Claim
                                ////if ((bool)claim.claimInfo.Icd_10_Claim)
                                ////{
                                ////    strBatchString += "BBQ:";  // BF=ICD-9 AB=ICD-10
                                ////}
                                ////else // ICD-9 Claim
                                ////{
                                ////    strBatchString += "BQ";  // BF=ICD-9 AB=ICD-10 
                                ////}
                                //if (claim.claimProcedures != null && claim.claimProcedures.Count() > 0)
                                //{
                                //    foreach (var proc in claim.claimProcedures)
                                //    {
                                //        //strBatchString += "HI*";
                                //        //strBatchString += "HI*BK:";
                                //        strBatchString += "HI*";
                                //        // ICD-10 Claim
                                //        if ((bool)claim.claimInfo.Icd_10_Claim)
                                //        {
                                //            strBatchString += "BBQ:";  // BF=ICD-9 AB=ICD-10
                                //        }
                                //        else // ICD-9 Claim
                                //        {
                                //            strBatchString += "BQ";  // BF=ICD-9 AB=ICD-10 
                                //        }
                                //        string[] splittedFROMDOS = proc.DosFrom.Split('/');
                                //        string[] splittedTODOS = proc.Dos_To.Split('/');
                                //        string Date_Of_Service_FROM = splittedFROMDOS[0] + splittedFROMDOS[1] + splittedFROMDOS[2];
                                //        string Date_Of_Service_TO = splittedTODOS[0] + splittedTODOS[1] + splittedTODOS[2];
                                //        strBatchString += Date_Of_Service_TO + "~";
                                //        break;
                                //    }
                                //    segmentCount++;
                                //}

                                #endregion End!

                                #region New:HI - OCCURRENCE SPAN INFORMATION
                                if (claim_Result.OccurrenceSpanCodes.Count > 0)
                                {
                                    string dtFrom = "";
                                    string dtTo = "";
                                    string dateFrom = "";
                                    strBatchString += "HI*";
                                    int spanCodeCount = claim_Result.OccurrenceSpanCodes.Count;
                                    foreach (var i in claim_Result.OccurrenceSpanCodes)
                                    {
                                        dateFrom = i.DateFrom;
                                        string dateTo = i.DateThrough;

                                        if (DateTime.TryParse(dateFrom, out DateTime parsedFromDate))
                                        {
                                            dtFrom = parsedFromDate.ToString("yyyyMMdd");
                                        }
                                        if (DateTime.TryParse(dateTo, out DateTime parsedToDate))
                                        {
                                            dtTo = parsedToDate.ToString("yyyyMMdd");
                                        }
                                        strBatchString += $"BI:{i.OccSpanCode}:RD8:{dtFrom}-{dtTo}";
                                        if (spanCodeCount > 1 && i != claim_Result.OccurrenceSpanCodes.Last())
                                        {
                                            strBatchString += "*";
                                        }
                                    }
                                    strBatchString += "~";
                                    segmentCount++;
                                }
                                #endregion End!

                                #region New:HI - OCCURRENCE INFORMATION
                                if (claim_Result.OccurrenceCodes.Count > 0)
                                {
                                    string date = "";
                                    string dates = "";
                                    strBatchString += "HI*";
                                    int CodeCount = claim_Result.OccurrenceSpanCodes.Count;
                                    foreach (var i in claim_Result.OccurrenceCodes)
                                    {
                                        dates = i.Date2;
                                        if (DateTime.TryParse(dates, out DateTime parsedDate))
                                        {
                                            date = parsedDate.ToString("yyyyMMdd");
                                        }
                                        strBatchString += $"BH:{i.OccCode}:D8:{date}";
                                        if (CodeCount > 1 && i != claim_Result.OccurrenceCodes.Last())
                                        {
                                            strBatchString += "*";
                                        }
                                    }
                                    //strBatchString += $"{AddDate}*BH:Industry Code:D8:{AddDate}";
                                    strBatchString += "~";
                                    segmentCount++;
                                }
                                #endregion End!

                                #region New:HI - VALUE INFORMATION
                                if (claim_Result.ValueCodes.Count > 0)
                                {
                                    strBatchString += "HI*";
                                    int ValueCount = claim_Result.ValueCodes.Count;
                                    foreach (var i in claim_Result.ValueCodes)
                                    {
                                        strBatchString += $"BE:{i.Value_Codes_Id}";
                                        strBatchString += ":::";
                                        strBatchString += $"{i.Amount}";
                                        if (ValueCount > 1 && i != claim_Result.ValueCodes.Last())
                                        {
                                            strBatchString += "*";
                                        }
                                    }
                                    strBatchString += "~";
                                    segmentCount++;
                                }
                                #endregion End!

                                #region New:HI - CONDITION INFORMATION
                                if (claim_Result.ConditionCodes.Count > 0)
                                {
                                    strBatchString += "HI*";
                                    int ConditionCount = claim_Result.ConditionCodes.Count;
                                    foreach (var i in claim_Result.ConditionCodes)
                                    {
                                        strBatchString += $"BG:{i}";
                                        if (ConditionCount > 1 && i != claim_Result.ConditionCodes.Last())
                                        {
                                            strBatchString += "*";
                                        }
                                    }
                                    strBatchString += "~";
                                    segmentCount++;
                                }
                                #endregion End!

                                //#region New:HI - TREATMENT CODE INFORMATION
                                //if (sPClaimData != null && sPClaimData.Count > 0)
                                //{
                                //    if (!string.IsNullOrEmpty(sPClaimData[0].REFERRAL_NUMBER))
                                //    {
                                //        strBatchString += "HI*TC:";
                                //        string ref_Num = sPClaimData[0].REFERRAL_NUMBER;
                                //        strBatchString += $"{ref_Num}~";
                                //        segmentCount++;
                                //    }
                                //}
                                //#endregion End!

                                #endregion

                                #region New:Loop 2310A - NM1 - Attending Provider Name

                                if (!string.IsNullOrEmpty(claim.claimInfo.Att_Lname))
                                {
                                    strBatchString += $"NM1*71*1*{claim.claimInfo.Att_Lname}*{claim.claimInfo.Att_Fname}****XX*{claim.claimInfo.Att_Npi}~";
                                    segmentCount++;
                                }

                                if (!string.IsNullOrEmpty(claim.claimInfo.Att_Taxonomy_Code))
                                {
                                    strBatchString += $"PRV*AT*PXC*{claim.claimInfo.Att_Taxonomy_Code}~";
                                    segmentCount++;
                                }

                                //#region New:Loop 2310A  REF ATTENDING PROVIDER SECONDARY IDENTIFICATION
                                ////this is new qualifier

                                //strBatchString += "REF*1G*add DATA here~";
                                //segmentCount++;

                                //#endregion End!

                                #endregion End!

                                //#region New: Loop 2310B - NM1 - Operating Physician Name

                                //foreach (var c in OT)
                                //{
                                //    operatingPhysicianTesting.id = c.id;
                                //    operatingPhysicianTesting.FName = c.FName;
                                //    operatingPhysicianTesting.LName = c.LName;
                                //    operatingPhysicianTesting.practice_code = c.practice_code;
                                //    operatingPhysicianTesting.NPI = c.NPI;
                                //}

                                //if (!string.IsNullOrEmpty(operatingPhysicianTesting.LName))
                                //{
                                //    strBatchString += $"NM1*72*1*{operatingPhysicianTesting.LName}******{operatingPhysicianTesting.NPI}~";
                                //    segmentCount++;
                                //}

                                //#endregion End!


                                #region LOOP 2310B (OPERATING PHYSICIAN SECONDARY IDENTIFICATION)
                                //if (!string.IsNullOrEmpty(claim.sPDataModel[0].PROVID_UPIN))
                                //{
                                //    strBatchString += "REF*1G*:";
                                //    string PROVID_UPIN = claim.sPDataModel[0].PROVID_UPIN;
                                //    strBatchString += $"{PROVID_UPIN}~";
                                //    segmentCount++;
                                //}
                                #endregion

                                #region LOOP 2310C (OTHER OPERATING PHYSICIAN NAME)
                                //if (claim.sPDataModel != null && claim.sPDataModel.Count>0)
                                //{
                                //    if (!string.IsNullOrEmpty(claim.sPDataModel[0].supervising_physician_lname) &&
                                //        !string.IsNullOrEmpty(claim.sPDataModel[0].supervising_physician_fname) &&
                                //        !string.IsNullOrEmpty(claim.sPDataModel[0].supervising_physician_UPIN))
                                //    {
                                //        strBatchString += "NM1*ZZ*1*";
                                //        string supFName = claim.sPDataModel[0].supervising_physician_fname;
                                //        string supLName = claim.sPDataModel[0].supervising_physician_lname;
                                //        string supUPIN = claim.sPDataModel[0].supervising_physician_UPIN;
                                //        string nameFirst = $"{supLName}";
                                //        strBatchString += $"{supFName}*{nameFirst}*A***XX*{supUPIN}";
                                //        strBatchString += $"~";
                                //        segmentCount++;
                                //    }
                                //}
                                #endregion

                                #region LOOP 2310C (OTHER OPERATING PHYSICIAN SECONDARY IDENTIFICATION)
                                //if (claim.sPDataModel != null && claim.sPDataModel.Count > 0)
                                //{
                                //    if(!string.IsNullOrEmpty(claim.sPDataModel[0].supervising_physician_UPIN))
                                //    {

                                //        strBatchString += "REF*1G*:";
                                //        strBatchString += $"{claim.sPDataModel[0].supervising_physician_UPIN}~";
                                //        segmentCount++;
                                //    } 
                                //}
                                #endregion

                                #region Removed:LOOP 2310A (REFERRING PROVIDER) 
                                //if (claim.claimInfo.Referring_Physician != null)
                                //{
                                //    if (!string.IsNullOrEmpty(claim.claimInfo.Ref_Npi))
                                //    {

                                //        if (!isAlphaNumeric(claim.claimInfo.Ref_Lname)
                                //                || !isAlphaNumeric(claim.claimInfo.Ref_Fname))
                                //        {
                                //            errorList.Add("Referring provider’s Name must be Alpha Numeric..");
                                //        }
                                //        else
                                //        {
                                //            strBatchString += "NM1*DN*1*" + claim.claimInfo.Ref_Lname + "*"
                                //                    + claim.claimInfo.Ref_Fname + "****XX*"
                                //                    + claim.claimInfo.Ref_Npi + "~";

                                //            segmentCount++;
                                //        }
                                //    }
                                //    else
                                //    {
                                //        errorList.Add("Referring provider’s NPI is missing.");
                                //    }
                                //}
                                #endregion

                                #region Old :LOOP 2310B (RENDERING PROVIDER)
                                if (claim.claimInfo.Attending_Physician != null)
                                {
                                    #region RENDERING PROVIDER NAME
                                    if (!string.IsNullOrEmpty(claim.claimInfo.Att_Npi))
                                    {

                                        if (!isAlphaNumeric(claim.claimInfo.Att_Lname)
                                                && !isAlphaNumeric(claim.claimInfo.Att_Fname))
                                        {
                                            errorList.Add("Rendering provider’s Name must be Alpha Numeric.");
                                        }
                                        else
                                        {
                                            strBatchString += "NM1*82*1*" + claim.claimInfo.Att_Lname + "*"
                                                    + claim.claimInfo.Att_Fname + "****XX*"
                                                    + claim.claimInfo.Att_Npi + "~";

                                            segmentCount++;
                                        }

                                    }
                                    else
                                    {
                                        errorList.Add("Rendering Provider NPI Missing.");

                                    }
                                    #endregion

                                    #region Removed:RENDERING PROVIDER SPECIALTY INFORMATION

                                    //if (!string.IsNullOrEmpty(claim.claimInfo.Att_Taxonomy_Code))
                                    //{
                                    //    strBatchString += "PRV*PE*PXC*" + claim.claimInfo.Att_Taxonomy_Code + "~"; //5010 CODE CHAGED FROM ZZ TO PXC
                                    //    segmentCount++;
                                    //}
                                    //else
                                    //{
                                    //    errorList.Add("Gateway edi require Rendering Provider Taxonomy Code.");
                                    //}
                                    //#endregion

                                    //#region RENDERING PROVIDER SPECIALTY INFORMATION                                
                                    //if (!string.IsNullOrEmpty(claim.claimInfo.Att_State_License))
                                    //{

                                    //    strBatchString += "REF*0B*" + claim.claimInfo.Att_State_License + "~";
                                    //    segmentCount++;
                                    //}
                                    #endregion

                                }
                                else
                                {
                                    errorList.Add("Rendering Provider Information missing..");
                                }
                                #endregion



                                #region LOOP 2310E (SERVICE FACILITY LOCATION)


                                if (claim.claimInfo.Facility_Code != 0)
                                {

                                    if (!string.IsNullOrEmpty(claim.claimInfo.Facility_Npi))
                                    {
                                        strBatchString += "NM1*77*2*" + claim.claimInfo.Facility_Name + "*****XX*"
                                                + claim.claimInfo.Facility_Npi + "~";
                                    }
                                    else
                                    {
                                        strBatchString += "NM1*77*2*" + claim.claimInfo.Facility_Name + "*****XX*~";
                                    }
                                    segmentCount++;

                                    if (string.IsNullOrEmpty(claim.claimInfo.Facility_Address)
                                            || string.IsNullOrEmpty(claim.claimInfo.Facility_City)
                                            || string.IsNullOrEmpty(claim.claimInfo.Facility_State)
                                            || string.IsNullOrEmpty(claim.claimInfo.Facility_Zip))
                                    {
                                        errorList.Add("Facility's address incomplete.");
                                    }

                                    strBatchString += "N3*" + claim.claimInfo.Facility_Address + "~";
                                    segmentCount++;
                                    strBatchString += "N4*" + claim.claimInfo.Facility_City + "*"
                                            + claim.claimInfo.Facility_State + "*";
                                    if (string.IsNullOrEmpty(claim.claimInfo.Facility_Zip))
                                    {
                                        strBatchString += "     " + "~";
                                    }
                                    else
                                    {
                                        strBatchString += claim.claimInfo.Facility_Zip + "~";
                                    }
                                    segmentCount++;
                                }


                                #endregion

                                //#region New:NM1 ATTENDING PROVIDER (2310A)



                                //#endregion End!

                                //#region New: REF ATTENDING PROVIDER SECONDARY IDENTIFICATION
                                ////this is new qualifier

                                //strBatchString += "REF*1G*add DATA here~";
                                //segmentCount++;

                                //#endregion End!

                                if (SecondaryIns != null)
                                {
                                    #region LOOP 2320

                                    #region OTHER SUBSCRIBER INFORMATION

                                    string SBR02_secondary = "18";

                                    if (!string.IsNullOrEmpty(SecondaryIns.GRelationship))
                                    {
                                        switch (SecondaryIns.GRelationship.ToUpper())
                                        {
                                            case "C":// Child
                                                SBR02_secondary = "19";
                                                break;
                                            case "P"://SPOUSE
                                                SBR02_secondary = "01";
                                                break;
                                            case "S"://Self
                                                SBR02_secondary = "18";
                                                break;
                                            case "O": // Other
                                                SBR02_secondary = "G8";
                                                break;
                                        }
                                    }

                                    strBatchString += "SBR*S*";
                                    string PlanNameSec = "", InsPayerTypeCodeSec = "", payerTypeCodeSec = "";

                                    if (!string.IsNullOrEmpty(SecondaryIns.Insgroup_Name) && SecondaryIns.Insgroup_Name.Contains("MEDICARE"))
                                    {
                                        if (!string.IsNullOrEmpty(SecondaryIns.plan_name) && SecondaryIns.plan_name.ToUpper().Contains("MEDICARE"))
                                        {
                                            PlanNameSec = SecondaryIns.plan_name;
                                        }
                                        else
                                        {
                                            PlanNameSec = "MEDICARE";
                                        }

                                        // Changing for SBR09 Error by TriZetto
                                        payerTypeCodeSec = SecondaryIns.insurance_type_code;

                                        // payerTypeCodeSec = "47"; //5010 required in case of medicare is secondary or ter.
                                        /*                        
                                         12	Medicare Secondary Working Aged Beneficiary or Spouse with Employer Group Health Plan
                                         13	Medicare Secondary End Stage Renal Disease
                                         14	Medicare Secondary , No Fault Insurance including Auto is Primary
                                         15	Medicare Secondary Worker’s Compensation
                                         16	Medicare Secondary Public Health Service (PHS) or other Federal Agency
                                         16	Medicare Secondary Public Health Service
                                         41	Medicare Secondary Black Lung
                                         42	Medicare Secondary Veteran’s Administration
                                         43	Medicare Secondary Veteran’s Administration
                                         47	Medicare Secondary, Other Liability Insurance is Primary
                                         */

                                    }
                                    else
                                    {
                                        PlanNameSec = SecondaryIns.plan_name;
                                        payerTypeCodeSec = SecondaryIns.insurance_type_code;
                                    }


                                    strBatchString += SBR02_secondary + "*" + SecondaryIns.Group_Number + "*" + PlanNameSec + "*" + InsPayerTypeCodeSec + "****" + payerTypeCodeSec + "~";
                                    segmentCount++;

                                    #endregion

                                    //#region LOOP 2320 (CLAIM LEVEL ADJUSTMENTS) // Removed for the time being
                                    //strBatchString += "CAS*";
                                    //string claimAdjustmentGroupCode = "PR*";
                                    //string ClaimAdjustmentReasonCode = "1";
                                    //string monetaryAmmount = "7.93";
                                    //strBatchString += $"{claimAdjustmentGroupCode}*{ClaimAdjustmentReasonCode}*{monetaryAmmount}~";
                                    //segmentCount++;
                                    //strBatchString += "CAS*";
                                    //string claimAdjustmentGroupCode2 = "OA";
                                    //string ClaimAdjustmentReasonCode2 = "93";
                                    //string monetaryAmmount2 = "15.06";
                                    //strBatchString += $"{claimAdjustmentGroupCode2}*{ClaimAdjustmentReasonCode2}*{monetaryAmmount2}~";
                                    //segmentCount++;
                                    //#endregion

                                    //#region LOOP 2320 (COORDINATION OF BENEFITS (COB) PAYER PAID AMOUNT) // Removed for the time being
                                    //strBatchString += "AMT*D";
                                    //string Payerpaidammount = "411";
                                    //strBatchString += $"*{Payerpaidammount}~";
                                    //segmentCount++;
                                    //#endregion

                                    #region OTHER INSURANCE COVERAGE INFORMATION

                                    if (!string.IsNullOrEmpty(SecondaryIns.GRelationship)
                               && SecondaryIns.GRelationship.ToUpper().Equals("S"))
                                    {
                                        //strBatchString += "OI***Y*P**Y~"; //- Changed C to P as per 5010  ==>837 file
                                        strBatchString += "OI***Y***Y~"; //-  ==>for 837i file
                                        segmentCount++;

                                    }
                                    else
                                    {
                                        strBatchString += "OI***Y***Y~"; //- Changed C to P as per 5010
                                        segmentCount++;
                                    }


                                    #endregion

                                    //     #region OTHER INSURANCE COVERAGE INFORMATION

                                    //     if (!string.IsNullOrEmpty(SecondaryIns.GRelationship)
                                    //&& SecondaryIns.GRelationship.ToUpper().Equals("S"))
                                    //     {
                                    //         //strBatchString += "OI***Y*P**Y~"; //- Changed C to P as per 5010  ==>837 file
                                    //         strBatchString += "OI***Y***Y~"; //-  ==>for 837i file
                                    //         segmentCount++;

                                    //     }
                                    //     else
                                    //     {
                                    //         strBatchString += "OI***Y***Y~"; //- Changed C to P as per 5010
                                    //         segmentCount++;
                                    //     }


                                    //     #endregion


                                    //#region LOOP 2320 (INPATIENT ADJUDICATION INFORMATION)  // Removed for the time being
                                    //strBatchString += "MIA*1***3568.98*MA01***************21***MA25~";
                                    //segmentCount++;
                                    //#endregion

                                    #endregion

                                    #region LOOP 2330A (OTHER SUBSCRIBER NAME and Address)
                                    if (!string.IsNullOrEmpty(SecondaryIns.GRelationship)
                                && SecondaryIns.GRelationship.ToUpper().Trim().Equals("S"))
                                    {

                                        strBatchString += "NM1*IL*1*";

                                        if (string.IsNullOrEmpty(claim.claimInfo.Lname) || string.IsNullOrEmpty(claim.claimInfo.Fname))
                                        {
                                            errorList.Add("Self -- Secondary Insurnace'subscriber Last/First Name missing.");
                                        }
                                        else
                                        {
                                            strBatchString += claim.claimInfo.Lname + "*"
                                                    + claim.claimInfo.Fname + "*"
                                                    + claim.claimInfo.Mname + "***MI*"
                                                    + SecondaryIns.Policy_Number.ToUpper() + "~";
                                            segmentCount++;
                                        }
                                        if (string.IsNullOrEmpty(claim.claimInfo.Address)
                                                || string.IsNullOrEmpty(claim.claimInfo.City)
                                                || string.IsNullOrEmpty(claim.claimInfo.State)
                                                || string.IsNullOrEmpty(claim.claimInfo.Zip))
                                        {
                                            errorList.Add("Self -- Subscriber Address incomplete.");
                                        }
                                        else
                                        {
                                            strBatchString += "N3*" + claim.claimInfo.Address + "~";
                                            segmentCount++;

                                            strBatchString += "N4*" + claim.claimInfo.City + "*"
                                                    + claim.claimInfo.State + "*";
                                            if (string.IsNullOrEmpty(claim.claimInfo.Zip))
                                            {
                                                strBatchString += "     " + "~";
                                            }
                                            else
                                            {
                                                strBatchString += claim.claimInfo.Zip + "~";
                                            }
                                            segmentCount++;
                                        }
                                    }
                                    else
                                    {
                                        strBatchString += "NM1*IL*1*";

                                        if (string.IsNullOrEmpty(SecondaryIns.Glname) || string.IsNullOrEmpty(SecondaryIns.Gfname))
                                        {
                                            errorList.Add("Secondary Insurnace'subscriber Last/First Name missing.");

                                        }
                                        else
                                        {
                                            strBatchString += SecondaryIns.Glname + "*"
                                                    + SecondaryIns.Gfname + "*"
                                                    + SecondaryIns.Gmi + "***MI*"
                                                    + SecondaryIns.Policy_Number.ToUpper() + "~";
                                            segmentCount++;
                                        }
                                        if (string.IsNullOrEmpty(SecondaryIns.Gaddress)
                                                || string.IsNullOrEmpty(SecondaryIns.Gcity)
                                                || string.IsNullOrEmpty(SecondaryIns.Gstate)
                                                || string.IsNullOrEmpty(SecondaryIns.Gzip))
                                        {
                                            errorList.Add("Secondary Subscriber Address incomplete.");
                                        }
                                        else
                                        {
                                            strBatchString += "N3*" + SecondaryIns.Gaddress + "~";
                                            segmentCount++;

                                            strBatchString += "N4*" + SecondaryIns.Gcity + "*"
                                                    + SecondaryIns.Gstate + "*";
                                            if (string.IsNullOrEmpty(SecondaryIns.Gzip))
                                            {
                                                strBatchString += "     " + "~";
                                            }
                                            else
                                            {
                                                strBatchString += SecondaryIns.Gzip + "~";
                                            }
                                            segmentCount++;
                                        }
                                    }
                                    #endregion

                                    #region LOOP 2330B (OTHER PAYER AND ADDRESS)
                                    string SecInsPayerName = "";
                                    if (string.IsNullOrEmpty(SecondaryIns.plan_name))
                                    {
                                        errorList.Add("Secondary's payer name missing.");
                                    }
                                    else
                                    {
                                        if (SecondaryIns.Insgroup_Name.Trim().Contains("MEDICARE"))
                                        {
                                            SecInsPayerName = "MEDICARE";
                                        }
                                        else
                                        {
                                            SecInsPayerName = SecondaryIns.plan_name;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(SecondaryIns.Payer_Number))
                                    {
                                        string secPayerNumber = primaryIns.Payer_Number.Equals(SecondaryIns.Payer_Number) ? SecondaryIns.Payer_Number + "A" : SecondaryIns.Payer_Number;
                                        strBatchString += "NM1*PR*2*" + SecInsPayerName + "*****PI*" + secPayerNumber + "~";
                                        segmentCount++;
                                    }
                                    else
                                    {
                                        errorList.Add("Secondary's insurance payer id is compulsory in case of Gateway EDI Clearing house.");
                                    }

                                    //Obsolete
                                    //strBatchString += "N3*" + SecondaryIns.Gaddress + "~";
                                    //segmentCount++;
                                    //strBatchString += "N4*" + SecondaryIns.Gcity + "*" + SecondaryIns.Gstate + "*" + SecondaryIns.Gzip.Trim() + "~";
                                    //segmentCount++;

                                    strBatchString += "N3*" + SecondaryIns.Ins_Address + "~";
                                    segmentCount++;
                                    strBatchString += "N4*" + SecondaryIns.Ins_City + "*" + SecondaryIns.Ins_State + "*" + SecondaryIns.Ins_Zip.Trim() + "~";
                                    segmentCount++;

                                    #endregion
                                }


                                //#region LOOP 2330B (REF - OTHER PAYER CLAIM CONTROL NUMBER)
                                //if (sPClaimData != null && sPClaimData.Count > 0)
                                //{
                                //    if (!string.IsNullOrEmpty(sPClaimData[0].ICN))
                                //    { 
                                //        strBatchString += "REF*F8*";
                                //        strBatchString += sPClaimData[0].PATIENT_ACCOUNT;
                                //        strBatchString += "~";
                                //        segmentCount++;
                                //    }
                                //}
                                //#endregion

                                //---Adding Submit/RESUBMIT CLAIM CPTS-----------
                                int line_no = 0;
                                if (claim.claimProcedures != null && claim.claimProcedures.Count() > 0)
                                {
                                    foreach (var proc in claim.claimProcedures)
                                    {

                                        if (claim.claimInfo.Is_Resubmitted && !proc.Is_Resubmitted)
                                        {
                                            continue;
                                        }

                                        line_no = line_no + 1;

                                        #region LOOP 2400

                                        #region SERVICE LINE                
                                        strBatchString += "LX*" + line_no + "~";
                                        segmentCount++;
                                        //line_no = line_no + 1;
                                        #endregion


                                        #region Removed:PROFESSIONAL SERVICE
                                        //if (!string.IsNullOrEmpty(claim.claimInfo.Claim_Pos))
                                        //{

                                        //    if (proc.Total_Charges > 0)
                                        //    {
                                        //        string modifiers = "";
                                        //        if (!string.IsNullOrEmpty(proc.Mod1.Trim()))
                                        //        {
                                        //            modifiers += ":" + proc.Mod1.Trim();
                                        //        }
                                        //        if (!string.IsNullOrEmpty(proc.Mod2.Trim()))
                                        //        {
                                        //            modifiers += ":" + proc.Mod2.Trim();
                                        //        }
                                        //        if (!string.IsNullOrEmpty(proc.Mod3.Trim()))
                                        //        {
                                        //            modifiers += ":" + proc.Mod3.Trim();
                                        //        }
                                        //        if (!string.IsNullOrEmpty(proc.Mod4.Trim()))
                                        //        {
                                        //            modifiers += ":" + proc.Mod4.Trim();
                                        //        }

                                        //        strBatchString += "SV1*HC:" + proc.Proc_Code.Trim() + modifiers + "*"
                                        //                + string.Format("{0:0.00}", proc.Total_Charges) + "*UN*"
                                        //                + proc.Units + "*"
                                        //                + claim.claimInfo.Claim_Pos + "*"
                                        //                + "*";
                                        //    }
                                        //    else
                                        //    {
                                        //        errorList.Add("Procedure Code:  " + proc.Proc_Code.Trim() + " has ZERO charges");
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    errorList.Add("Claim's pos code missing");
                                        //}

                                        //string pointers = "";
                                        //if (proc.Dx_Pointer1 > 0)
                                        //{
                                        //    pointers = proc.Dx_Pointer1.ToString();
                                        //}
                                        //if (proc.Dx_Pointer2 > 0)
                                        //{
                                        //    pointers += ":" + proc.Dx_Pointer1.ToString();
                                        //}
                                        //if (proc.Dx_Pointer3 > 0)
                                        //{
                                        //    pointers += ":" + proc.Dx_Pointer3.ToString();
                                        //}
                                        //if (proc.Dx_Pointer4 > 0)
                                        //{
                                        //    pointers += ":" + proc.Dx_Pointer4.ToString();
                                        //}

                                        //strBatchString += pointers + "~";
                                        //segmentCount++;

                                        #endregion

                                        #region New: SV2 INSTITUTIONAL SERVICE

                                        if (!string.IsNullOrEmpty(claim.claimInfo.Claim_Pos))
                                        {

                                            if (proc.Total_Charges > 0)
                                            {
                                                string modifiers = "";
                                                if (!string.IsNullOrEmpty(proc.Mod1.Trim()))
                                                {
                                                    modifiers += ":" + proc.Mod1.Trim();
                                                }
                                                if (!string.IsNullOrEmpty(proc.Mod2.Trim()))
                                                {
                                                    modifiers += ":" + proc.Mod2.Trim();
                                                }
                                                if (!string.IsNullOrEmpty(proc.Mod3.Trim()))
                                                {
                                                    modifiers += ":" + proc.Mod3.Trim();
                                                }
                                                if (!string.IsNullOrEmpty(proc.Mod4.Trim()))
                                                {
                                                    modifiers += ":" + proc.Mod4.Trim();
                                                }
                                                if (sPClaimData != null && sPClaimData.Count > 0)
                                                {
                                                    strBatchString += $"SV2*{sPClaimData[0].Revenue_Code.Trim()}*HC:{proc.Proc_Code.Trim()}{modifiers}*{string.Format("{0:0.00}", proc.Total_Charges)}*UN*" + proc.Units + "~";
                                                    segmentCount++;
                                                }
                                            }
                                            else
                                            {
                                                errorList.Add("Procedure Code:  " + proc.Proc_Code.Trim() + " has ZERO charges");
                                            }
                                        }
                                        else
                                        {
                                            errorList.Add("Claim's pos code missing");
                                        }
                                        //string pointers = "";
                                        //if (proc.Dx_Pointer1 > 0)
                                        //{
                                        //    pointers = proc.Dx_Pointer1.ToString();
                                        //}
                                        //if (proc.Dx_Pointer2 > 0)
                                        //{
                                        //    pointers += ":" + proc.Dx_Pointer1.ToString();
                                        //}
                                        //if (proc.Dx_Pointer3 > 0)
                                        //{
                                        //    pointers += ":" + proc.Dx_Pointer3.ToString();
                                        //}
                                        //if (proc.Dx_Pointer4 > 0)
                                        //{
                                        //    pointers += ":" + proc.Dx_Pointer4.ToString();
                                        //}

                                        //strBatchString += pointers + "~";
                                        //segmentCount++;

                                        #endregion End!

                                        #region SERVICE Date

                                        strBatchString += "DTP*472*D8*";

                                        string[] splittedFROMDOS = proc.DosFrom.Split('/');
                                        string[] splittedTODOS = proc.Dos_To.Split('/');
                                        string Date_Of_Service_FROM = splittedFROMDOS[0] + splittedFROMDOS[1] + splittedFROMDOS[2];
                                        string Date_Of_Service_TO = splittedTODOS[0] + splittedTODOS[1] + splittedTODOS[2];
                                        strBatchString += Date_Of_Service_TO + "~";
                                        segmentCount++;
                                        #endregion

                                        #region LINE ITEM CONTROL NUMBER (CLAIM PROCEDURES ID)
                                        strBatchString += "REF*6R*" + proc.Claim_Procedures_Id.ToString() + "~";
                                        segmentCount++;
                                        #endregion

                                        #region LINE Note
                                        if (!string.IsNullOrEmpty(proc.Notes.Trim()))
                                        {
                                            strBatchString += "NTE*ADD*" + proc.Notes.Trim() + "~";
                                            segmentCount++;
                                        }

                                        #endregion

                                        #endregion


                                        #region LOOP 2410 (LIN - DRUG IDENTIFICATION)


                                        if (!string.IsNullOrEmpty(proc.Ndc_Code))
                                        {
                                            strBatchString += "LIN**N4*" + proc.Ndc_Code.Trim() + "~";
                                            segmentCount++;
                                            if (proc.Ndc_Qty > 0)
                                            {
                                                if (!string.IsNullOrEmpty(proc.Ndc_Measure))
                                                {
                                                    strBatchString += "CTP****" + proc.Ndc_Qty.ToString() + "*" + proc.Ndc_Measure + "*~";
                                                    segmentCount++;
                                                }
                                                else
                                                {
                                                    errorList.Add("Procedure NDC Quantity/Qual or Unit Price is missing.");
                                                }
                                            }

                                        }

                                        #endregion

                                        #region LOOP 2430 (LINE ADJUDICATION INFORMATION)

                                        //#region LOOP (SVD - LINE ADJUDICATION INFORMATION) // Removed for the time being
                                        //strBatchString += "SVD*";
                                        //int Identification_Code = 43;
                                        //strBatchString += $"{Identification_Code}*55*HC:84550**3~";
                                        //segmentCount++;
                                        //#endregion

                                        //#region LOOP (CAS - LINE ADJUSTMENT)  // Removed for the time being
                                        //strBatchString += "CAS*PR*";
                                        //string claimAdjustmentReasonCode = "1";
                                        //string monetaryAmmount = "7.93";
                                        //strBatchString += $"{claimAdjustmentReasonCode}*{monetaryAmmount}~";
                                        //segmentCount++;
                                        //#endregion

                                        //#region LOOP (DTP - LINE CHECK OR REMITTANCE DATE)  // Removed for the time being
                                        //strBatchString += "DTP*573*D8*20040203~";
                                        //segmentCount++;
                                        //#endregion

                                        #region LOOP (AMT - REMAINING PATIENT LIABILITY)
                                        //if (sPClaimData != null && sPClaimData.Count > 0)
                                        //{
                                        //    if (!string.IsNullOrEmpty(sPClaimData[0].AMT_DUE.ToString()) && sPClaimData[0].AMT_DUE > 0)
                                        //    {
                                        //        strBatchString += "AMT*EAF*";
                                        //        strBatchString += $"{sPClaimData[0].AMT_DUE}~";
                                        //        segmentCount++;
                                        //    }
                                        //}

                                        #endregion

                                        #endregion
                                    }
                                }
                                if (line_no == 0)
                                {
                                    errorList.Add("Claim Procedures missing.");
                                }
                            }
                        }

                    }
                }


                if (errorList.Count == 0)
                {
                    segmentCount++;
                    strBatchString += "SE*" + segmentCount + "*0001~GE*1*" + batchId + $"~IEA*1*000000001~";

                    objResponse.Status = "Success";
                    objResponse.Response = strBatchString;

                    //using (var w = new StreamWriter(HttpContext.Current.Server.MapPath("/SubmissionFile/" + claim_id + ".txt"), false))
                    //{
                    //    w.WriteLine(strBatchString);
                    //}

                }
                else
                {
                    objResponse.Status = "Error";
                    objResponse.Response = errorList;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objResponse;

        }

        #endregion  End_Generation!

        public List<SPDataModel> getSpResult(string claim_id, string insType)
        {
            ResponseModel objResponse = new ResponseModel();
            List<SPDataModel> resultList = new List<SPDataModel>();
            string storedProcedureName = "WEBEHR_BILLINGPRO_GET_HCFA_PRINT_DETAIL_ICD10_tets";
            using (var ctx = new NPMDBEntities())
            {
                var adoConnection = ctx.Database.Connection as SqlConnection;
                if (adoConnection != null)
                {
                    using (IDbConnection connection = adoConnection)
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("claims", claim_id);
                        parameters.Add("@insType", insType);
                        resultList = connection.Query<SPDataModel>(storedProcedureName, parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    }

                }
            }
            if (resultList != null)
            {
                objResponse.Status = "Success";
                objResponse.Response = resultList;
            }
            else
            {
                objResponse.Status = "No Data Found";
            }
            return resultList;
        }
        public ClaimsDataModel GetClaimsData(string id)
        {
            ClaimsDataModel result = new ClaimsDataModel();
            ClaimsViewModel claimsViewModel = new ClaimsViewModel();
            //string connectionString = ConfigurationManager.ConnectionStrings["NPMDBEntities"].ConnectionString;
            string connectionString = ConfigurationManager.ConnectionStrings["NPMUB04"].ConnectionString;
            //using (SqlConnection con = new SqlConnection("Server=******;Database=*****;User Id=*****;Password=*****;"))
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query1 = "SELECT ConditionCode FROM Claims_Condition_Code WHERE claim_no = @Id";
                string query2 = "SELECT OccCode, Date FROM Claims_Occurrence_Code WHERE claim_no = @Id";
                string query3 = "SELECT OccSpanCode, DateFrom, DateThrough FROM Claims_Occurence_Span_Code WHERE claimno = @Id";
                string query4 = "SELECT Value_Codes_Id, Amount FROM Claims_Value_Code WHERE claim_no = @Id";

                using (SqlCommand cmd1 = new SqlCommand(query1, con))
                {
                    cmd1.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader1 = cmd1.ExecuteReader())
                    {
                        while (reader1.Read())
                        {
                            if (reader1.HasRows)
                            {
                                result.ConditionCodes.Add(reader1["ConditionCode"].ToString());
                            }
                        }
                    }
                }

                using (SqlCommand cmd2 = new SqlCommand(query2, con))
                {
                    cmd2.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader2 = cmd2.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            OccurrenceCodeModel occurrenceCode = new OccurrenceCodeModel
                            {
                                OccCode = reader2["OccCode"].ToString(),
                                Date2 = Convert.ToDateTime(reader2["Date"]).ToString()
                            };
                            result.OccurrenceCodes.Add(occurrenceCode);
                        }
                    }
                }

                using (SqlCommand cmd3 = new SqlCommand(query3, con))
                {
                    cmd3.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader3 = cmd3.ExecuteReader())
                    {
                        while (reader3.Read())
                        {
                            OccurenceSpanModel occurrenceSpanCode = new OccurenceSpanModel
                            {
                                OccSpanCode = reader3["OccSpanCode"].ToString(),
                                DateFrom = Convert.ToDateTime(reader3["DateFrom"]).ToString(),
                                DateThrough = Convert.ToDateTime(reader3["DateThrough"]).ToString()
                            };
                            result.OccurrenceSpanCodes.Add(occurrenceSpanCode);
                        }
                    }
                }

                using (SqlCommand cmd4 = new SqlCommand(query4, con))
                {
                    cmd4.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader4 = cmd4.ExecuteReader())
                    {
                        while (reader4.Read())
                        {
                            ValueeCode valueCode = new ValueeCode
                            {
                                Value_Codes_Id = reader4["Value_Codes_Id"].ToString(),
                                Amount = Decimal.Round(Convert.ToDecimal(reader4["Amount"]), 0)
                            };
                            result.ValueCodes.Add(valueCode);
                        }
                    }
                }
            }

            return result;
        }
        public ResponseModel View837(long batchId, long practice_id)
        {
            List<string> errorList = new List<string>();
            List<ResponseModel> response = new List<ResponseModel>();
            List<ViewBatchRequest> claimDetails;

            using (var ctx = new NPMDBEntities())
            {
                claimDetails = (from detail in ctx.claim_batch_detail
                                join cbd in ctx.claim_batch on detail.batch_id equals cbd.batch_id 
                                join c in ctx.Claims on detail.claim_id equals c.Claim_No
                                where detail.batch_id == batchId
                                select new ViewBatchRequest
                                {
                                    claim_no = detail.claim_id ?? 0,
                                    claim_type = c.Claim_Type,
                                    batch_claim_type =cbd.batch_claim_type,
                                    Pri_Status = c.Pri_Status,
                                    Sec_Status = c.Sec_Status
                                }).ToList();
            }
            foreach (var c in claimDetails)
            {
                ResponseModel objres = null;

                if ((c.claim_type == "P" || c.claim_type == null) && (c.batch_claim_type == "P" || (c.Pri_Status == "N" || c.Pri_Status == "R" || c.Pri_Status == "B")))
                {
                    objres = GenerateBatch_5010_P_P(practice_id, c.claim_no);
                }
                else if ((c.claim_type == "P" || c.claim_type == null) && (c.batch_claim_type == "S" || (c.Sec_Status == "N" || c.Sec_Status == "R" || c.Sec_Status == "B")))
                {
                    objres = GenerateBatch_5010_P_S(practice_id, c.claim_no);
                }
                else
                {
                    objres = GenerateBatch_For_Packet_837i_5010_I(practice_id, c.claim_no);
                }
                try
                {
                    if (objres != null)
                    {
                        if (objres.Status == "Error")
                        {
                            errorList.Add($"Claim No: {c.claim_no}");
                            if (objres.Response != null)
                            {
                                if (objres.Response is IEnumerable<string> errorMessages)
                                {
                                    foreach (var error in errorMessages)
                                    {
                                        errorList.Add(error);
                                    }
                                }
                                else
                                {
                                    errorList.Add("Response contains unexpected data type for errors.");
                                }
                            }
                            else
                            {
                                errorList.Add("Response is null.");
                            }
                        }
                        else
                        {
                            response.Add(objres);
                        }
                    }
                    else
                    {
                        errorList.Add("Response is null.");
                    }
                }
                catch (Exception ex)
                {
                    errorList.Add($"An error occurred: {ex.Message}");
                }
            }


            ResponseModel finalResponse = new ResponseModel();

            if (errorList.Count > 0)
            {
                finalResponse.Status = "Error";
                finalResponse.Response = errorList;
            }
            else
            {
                finalResponse.Status = "Success";
                finalResponse.Response = response;
            }

            return finalResponse;
        }
        public ResponseModel HoldBatch(HoldBatchRequestViewModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    claim_batch batch = ctx.claim_batch.FirstOrDefault(c => c.batch_id == model.BatchId);
                    if (batch != null)
                    {
                        batch.On_Hold = model.holdStatus;
                        batch.modified_user = model.UserId;
                        batch.date_modified = DateTime.Now;
                        if (ctx.SaveChanges() > 0)
                        {
                            batch = ctx.claim_batch.FirstOrDefault(c => c.batch_id == model.BatchId);
                            responseModel.Status = "Success";
                            responseModel.Response = batch;
                        }
                        else
                        {
                            responseModel.Status = "Error";
                        }
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        #region internal_status
        // Example method to extract claim_number from EDI string

        //string responseString277 = bcd?.Response_String_277;
        public static string ExtractClaimNumber(string responseString277)
        {
            string claimNumber = null;

            // Regex to match REF*EJ*<claim_number> format
            //var match = Regex.Match(responseString277, @"REF\*EJ\*(\d+)");
            var match = Regex.Match(responseString277, @"TRN\*2\*(\d+)");

            if (match.Success)
            {
                claimNumber = match.Groups[1].Value; // Extract the claim number
            }

            return claimNumber;
        }

        // Example method to update the csi_batch table with the stcStatus
        public static void UpdateCsiBatchStatus(string claimNumber, string stcStatus)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE csi_batch SET Internal_status = @stcStatus WHERE Trn = @claimNumber";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@stcStatus", stcStatus);
                    command.Parameters.AddWithValue("@claimNumber", claimNumber);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Internal_status updated successfully.");
                    }
                    else
                    {
                        Console.WriteLine("No matching claim_number found.");
                    }
                }
            }
        }

        // Method to process EDI and update database
        public static void ProcessEdiFileAndUpdateStatus(string responseString277, string stcStatus)
        {
            // Extract claim number from EDI file
            string claimNumber = ExtractClaimNumber(responseString277);

            if (!string.IsNullOrEmpty(claimNumber))
            {
                // Update the Internal_status for matching claim_number in the csi_batch table
                UpdateCsiBatchStatus(claimNumber, stcStatus);
            }
            else
            {
                Console.WriteLine("Claim number not found in the EDI file.");
            }
        }
        #endregion
    }


    public class ClaimsDataModel
    {
        public List<string> ConditionCodes { get; set; } = new List<string>();
        public List<OccurrenceCodeModel> OccurrenceCodes { get; set; } = new List<OccurrenceCodeModel>();
        public List<OccurenceSpanModel> OccurrenceSpanCodes { get; set; } = new List<OccurenceSpanModel>();
        public List<ValueeCode> ValueCodes { get; set; } = new List<ValueeCode>();
    }
}