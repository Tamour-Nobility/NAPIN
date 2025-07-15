using EdiFabric.Core.Model.Edi;
using EdiFabric.Framework.Readers;
using EdiFabric.Templates.Hipaa5010;
using Newtonsoft.Json;
using NPMAPI.Enums;
using NPMAPI.Models.ViewModels;
using NPMAPI.Models;
using NPMAPI.Repositories;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using static NPMAPI.Controllers.SubmissionController;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Configuration;
using Microsoft.Ajax.Utilities;
using Dapper;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace NPMAPI.Services
{

    public class ClaimStatusService : IClaimStatusService
    {
        private readonly IPracticeRepository _practiceService;
        public ClaimStatusService(IPracticeRepository practiceService)
        {
            _practiceService = practiceService;
        }
        #region   Generate TRN Serial Number
        public ResponseModelForSerialnumber GenerateSerialNumber(int length = 8)
        {
            if (length < 8 || length > 10)
            {
                return new ResponseModelForSerialnumber
                {
                    Success = false,
                    Message = "Length must be between 8 and 10 digits"
                };
            }

            string serialNumber = GenerateNextSerialNumber(length);

            if (serialNumber == null)
            {
                return new ResponseModelForSerialnumber
                {
                    Success = false,
                    Message = "Unable to generate a unique serial number"
                };
            }

            InsertProduct(serialNumber, "TRN");

            return new ResponseModelForSerialnumber
            {
                Success = true,
                Message = "Serial number generated successfully",
                SerialNumber = serialNumber
            };
        }
        private string GenerateNextSerialNumber(int length)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 SerialNumber FROM ClaimStatusTrn ORDER BY SerialNumber DESC", conn))
                {
                    var latestSerialNumber = cmd.ExecuteScalar() as string;

                    if (latestSerialNumber != null && latestSerialNumber.Length == length && long.TryParse(latestSerialNumber, out long number))
                    {
                        number++;
                        string nextSerialNumber = number.ToString().PadLeft(length, '0');

                        if (nextSerialNumber.Length > length)
                        {
                            return null;
                        }

                        return nextSerialNumber;
                    }
                    return "1".PadLeft(length, '0');
                }
            }
        }
        private void InsertProduct(string serialNumber, string name)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO ClaimStatusTrn (SerialNumber, Name) VALUES (@SerialNumber, @Name)", conn))
                {
                    cmd.Parameters.AddWithValue("@SerialNumber", serialNumber);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion
        #region  Generate ISA AND IEA Sequancer Number
        public ResponseModelForSerialnumber GenerateSerialNumberISA(int length = 9)
        {
            if (length < 8 || length > 10)
            {
                return new ResponseModelForSerialnumber
                {
                    Success = false,
                    Message = "Length must be between 8 and 10 digits"
                };
            }

            string serialNumber = GenerateNextSerialNumberISA(length);

            if (serialNumber == null)
            {
                return new ResponseModelForSerialnumber
                {
                    Success = false,
                    Message = "Unable to generate a unique serial number"
                };
            }

            InsertProductISA(serialNumber);

            return new ResponseModelForSerialnumber
            {
                Success = true,
                Message = "Serial number generated successfully",
                SerialNumber = serialNumber
            };
        }
        private string GenerateNextSerialNumberISA(int length)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 SequancerNumber FROM ClaimStatusISAIEA ORDER BY SequancerNumber DESC", conn))
                {
                    var latestSerialNumber = cmd.ExecuteScalar() as string;

                    if (latestSerialNumber != null && latestSerialNumber.Length == length && long.TryParse(latestSerialNumber, out long number))
                    {
                        number++;
                        string nextSerialNumber = number.ToString().PadLeft(length, '0');

                        if (nextSerialNumber.Length > length)
                        {
                            return null;
                        }

                        return nextSerialNumber;
                    }
                    return "1".PadLeft(length, '0');
                }
            }
        }
        private void InsertProductISA(string sequancernumber)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO ClaimStatusISAIEA (SequancerNumber) VALUES (@SequancerNumber)", conn))
                {
                    cmd.Parameters.AddWithValue("@SequancerNumber", sequancernumber);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion
        #region  Generate BHT AND GS Sequancer Number
        public ResponseModelForSerialnumber GenerateSerialNumberBHTGS(int length = 9)
        {
            if (length < 8 || length > 10)
            {
                return new ResponseModelForSerialnumber
                {
                    Success = false,
                    Message = "Length must be between 8 and 10 digits"
                };
            }

            string serialNumber = GenerateNextSerialNumberBHTGS();

            if (serialNumber == null)
            {
                return new ResponseModelForSerialnumber
                {
                    Success = false,
                    Message = "Unable to generate a unique serial number"
                };
            }

            InsertProductBHTGS(serialNumber);

            return new ResponseModelForSerialnumber
            {
                Success = true,
                Message = "Serial number generated successfully",
                SerialNumber = serialNumber
            };
        }

        private string GenerateNextSerialNumberBHTGS()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 SequancerNumber FROM ClaimStatusISAIEA ORDER BY SequancerNumber DESC", conn))
                {
                    var latestSerialNumber = cmd.ExecuteScalar() as string;

                    if (latestSerialNumber != null && latestSerialNumber.Length == 9 && long.TryParse(latestSerialNumber, out long number))
                    {
                        // Increment the second digit from the left by 1
                        long secondDigit = (number / 100000000) % 10;
                        secondDigit++;

                        // Ensure the second digit stays within the limit
                        secondDigit %= 10;

                        // Reconstruct the next serial number
                        string nextSerialNumber = $"2{secondDigit:D1}{number % 100000000:D7}";

                        return nextSerialNumber;
                    }
                    return "222222220"; // Default starting value
                }
            }
        }
        private void InsertProductBHTGS(string sequancernumber)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO ClaimStatusBHTGSGE (SequancerNumber) VALUES (@SequancerNumber)", conn))
                {
                    cmd.Parameters.AddWithValue("@SequancerNumber", sequancernumber);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion
        #region  Get Data From Stored Procedure OF TRN SeraialNumber
        public ResponseModelSerialNumber SequanceNumber()
        {
            ResponseModelSerialNumber objResponse = new ResponseModelSerialNumber();
            string storedProcedureName = "Sp_ClaimStatusTRNNumber";

            using (var ctx = new NPMDBEntities())
            {
                List<ClaimStatusTRNNumberModel> claimStatusTRNNumbers = new List<ClaimStatusTRNNumberModel>();
                var adoConnection = ctx.Database.Connection as SqlConnection;

                if (adoConnection != null)
                {
                    using (IDbConnection connection = adoConnection)
                    {
                        connection.Open();
                        claimStatusTRNNumbers = connection.Query<ClaimStatusTRNNumberModel>(storedProcedureName, commandType: CommandType.StoredProcedure).ToList();
                        objResponse.obj = claimStatusTRNNumbers;
                        objResponse.Status = "Success";
                    }
                }

                return objResponse;
            }
        }
        #endregion
        #region  Get Data From Stored Procedure OF IEAISASequancerNumber
        public ResponseModelSequancerNumber SequanceNumberISA()
        {
            ResponseModelSequancerNumber objResponse = new ResponseModelSequancerNumber();
            string storedProcedureName = "Sp_ClaimStatusISAIEANumber";

            using (var ctx = new NPMDBEntities())
            {
                List<ClaimStatusISAIEANumberModel> claimStatusTRNNumbers = new List<ClaimStatusISAIEANumberModel>();
                var adoConnection = ctx.Database.Connection as SqlConnection;

                if (adoConnection != null)
                {
                    using (IDbConnection connection = adoConnection)
                    {
                        connection.Open();
                        claimStatusTRNNumbers = connection.Query<ClaimStatusISAIEANumberModel>(storedProcedureName, commandType: CommandType.StoredProcedure).ToList();
                        objResponse.obj = claimStatusTRNNumbers;
                        objResponse.Status = "Success";
                    }
                }

                return objResponse;
            }
        }
        #endregion
        #region  Get Data From Stored Procedure OF BHTGEGSSequancerNumber
        public ResponseModelSequancerNumber SequanceNumberBHT()
        {
            ResponseModelSequancerNumber objResponse = new ResponseModelSequancerNumber();
            string storedProcedureName = "Sp_ClaimStatusBHTGEGSNumber";

            using (var ctx = new NPMDBEntities())
            {
                List<ClaimStatusISAIEANumberModel> claimStatusTRNNumbers = new List<ClaimStatusISAIEANumberModel>();
                var adoConnection = ctx.Database.Connection as SqlConnection;

                if (adoConnection != null)
                {
                    using (IDbConnection connection = adoConnection)
                    {
                        connection.Open();
                        claimStatusTRNNumbers = connection.Query<ClaimStatusISAIEANumberModel>(storedProcedureName, commandType: CommandType.StoredProcedure).ToList();
                        objResponse.obj = claimStatusTRNNumbers;
                        objResponse.Status = "Success";
                    }
                }

                return objResponse;
            }
        }
        #endregion

        #region Generate_Packet_276_File!
        public ResponseModel GenerateBatch_276(long practice_id, long claim_id, long Insurance_Id, string unique_name)
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
                List<Claim_Insurance> claim_Insurances = null;
                List<SPDataModel> sPDataModels = null;
                ClaimsDataModel claim_Result = null;

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


                ResponseModelSerialNumber res = new ResponseModelSerialNumber();
                ResponseModelSequancerNumber res1 = new ResponseModelSequancerNumber();
                ResponseModelSequancerNumber res2 = new ResponseModelSequancerNumber();
                if (errorList.Count == 0)
                {
                    using (var ctx = new NPMDBEntities())
                    {
                        batchClaimInfo = ctx.spGetBatchClaimsInfo(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                        batchClaimDiagnosis = ctx.spGetBatchClaimsDiagnosis(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                        batchClaimProcedures = ctx.spGetBatchClaimsProcedurestest(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                        insuraceInfo = ctx.spGetBatchClaimsInsurancesInfo(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
                        claim_Result = GetClaimsData(claim_id.ToString());
                        claim_Insurances = ctx.Claim_Insurance.Where(c => c.Claim_No == claim_id).ToList();
                        var filtered_Data = insuraceInfo.Where(i => i.Insurance_Id == Insurance_Id).ToList();
                        insuraceInfo = filtered_Data;
                        var originalclaim = ctx.Claims.Where(x => x.Claim_No == claim_id).FirstOrDefault();
                        sPDataModels = getSpResult(claim_id.ToString(), "P").ToList();
                        res = SequanceNumber();
                        res1 = SequanceNumberISA();
                        res2 = SequanceNumberBHT();
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

                        claimSubmissionInfo.Add(claimSubmissionModel);

                    }

                    if (claimSubmissionInfo.Count > 0)
                    {
                        batchId = claimSubmissionInfo[0].claim_No.ToString();
                        string dateTime_yyMMdd = DateTime.Now.ToString("yyMMdd");
                        string dateTime_yyyyMMdd = DateTime.Now.ToString("yyyyMMdd");
                        string dateTime_HHmm = DateTime.Now.ToString("HHmm");
                        segmentCount = 0;

                        #region ISA Header
                        strBatchString = "ISA*";
                        strBatchString += "00*          " + "*00*" + "          *" + "ZZ" + "*V313           ".PadRight(15) + "*ZZ*263923727      *";
                        strBatchString += dateTime_yyMMdd + "*";
                        strBatchString += dateTime_HHmm + "*";
                        strBatchString += $"^*00501*";
                        strBatchString += res1.obj[0].SequancerNumber + "*";
                        strBatchString += "0*P*:~";
                        #endregion  ISA Header
                        #region GS
                        strBatchString += "GS*HR*V313*";
                        strBatchString += "263923727" + "*";
                        strBatchString += dateTime_yyyyMMdd + "*";
                        strBatchString += dateTime_HHmm + "*";
                        strBatchString += res2.obj[0].SequancerNumber + "*";
                        strBatchString += "X*005010X212~";
                        #endregion
                        #region  ST
                        strBatchString += "ST*276*00001*005010X212~";
                        segmentCount++;
                        #endregion
                        #region BHT
                        strBatchString += $"BHT*0010*13*";
                        strBatchString += res2.obj[0].SequancerNumber + "*";
                        strBatchString += dateTime_yyyyMMdd + "*";
                        strBatchString += dateTime_HHmm + "~";
                        segmentCount++;
                        #endregion
                        #region HL1 BILLING PROVIDER HIERARCHICAL LEVEL
                        int HL = 1;
                        strBatchString += "HL*" + HL + "*";
                        strBatchString += "*20*1~";
                        segmentCount++;
                        #endregion


                        spGetBatchClaimsInsurancesInfo_Result primaryIns = null;
                        spGetBatchClaimsInsurancesInfo_Result SecondaryIns = null;
                        spGetBatchClaimsInsurancesInfo_Result otherIns = null;


                        foreach (var claim in claimSubmissionInfo)
                        {
                            var claimid = claim.claim_No;
                            List<SPDataModel> sPClaimData = new List<SPDataModel>();
                            sPClaimData = claim.sPDataModel.Where(p => p.clm == claimid.ToString()).ToList();
                            long patientId = (long)claim.claimInfo.Patient_Id;
                            long claimId = claim.claimInfo.Claim_No;
                            string DOS = claim.claimInfo.Dos;
                            string patientName = claim.claimInfo.Lname + ", " + claim.claimInfo.Fname;
                            string Billing_Provider_NPI = "";
                            string TaxonomyCode = "";
                            string FederalTaxID = "";
                            string FederalTaxIDType = "";

                            string box_33_type = "";

                            #region Check If Payer Validation Expires
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

                            if (claim.claimInsurance == null || claim.claimInsurance.Count == 0)
                            {
                                errorList.Add("Patient Insurance Information is missing.");
                            }
                            else if (primaryIns == null)
                            {
                                if (SecondaryIns == null)
                                {
                                    if (otherIns == null)
                                    {
                                        errorList.Add("Patient Primary Insurance Information is missing.");
                                    }

                                }
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
                                //if (!primaryIns.GRelationship.Trim().ToUpper().Equals("S") && primaryIns.Guarantor_Id == null)
                                //{
                                //    errorList.Add("Subscriber information is missing.");

                                //}
                                if (primaryIns.Inspayer_Id == null)
                                {
                                    errorList.Add("Payer's information is missing.");
                                }


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
                                    }
                                }
                                if (line_no == 0)
                                {
                                    errorList.Add("Claim Procedures missing.");
                                }
                                #endregion

                                #region NMI PR LOOP 2010BB (PAYER INFORMATION)
                                if (string.IsNullOrEmpty(primaryIns.plan_name))
                                {
                                    errorList.Add("Payer name missing.");
                                }
                                string paperPayerName = "";
                                string paperPayerID = "";
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
                                if (!string.IsNullOrEmpty(primaryIns.Insgroup_Name) && primaryIns.plan_name.Trim().ToUpper().Equals("WORK COMP"))
                                {
                                    if (string.IsNullOrEmpty(primaryIns.Sub_Empaddress)
                                            || string.IsNullOrEmpty(primaryIns.Sub_Emp_City)
                                            || string.IsNullOrEmpty(primaryIns.Sub_Emp_State)
                                            || string.IsNullOrEmpty(primaryIns.Sub_Emp_Zip))
                                    {
                                        errorList.Add("Payer is Worker Company, so its subscriber employer’s address is necessary.");

                                    }
                                    strBatchString += "~";
                                    //segmentCount++;
                                }
                            }
                            if (SecondaryIns != null)
                            {
                                if (string.IsNullOrEmpty(SecondaryIns.plan_name))
                                {
                                    errorList.Add("Payer name missing.");
                                }
                                string paperPayerName = "";
                                string paperPayerID = "";
                                if (!string.IsNullOrEmpty(SecondaryIns.plan_name) && SecondaryIns.plan_name.Trim().ToUpper().Equals("MEDICARE"))
                                {
                                    paperPayerName = "MEDICARE";
                                }
                                else
                                {
                                    paperPayerName = SecondaryIns.plan_name;
                                }

                                paperPayerID = SecondaryIns.Payer_Number;
                                if (!string.IsNullOrEmpty(paperPayerID))
                                {
                                    strBatchString += "NM1*PR*2*" + paperPayerName + "*****PI*" + paperPayerID + "~";
                                    segmentCount++;
                                }
                                else
                                {
                                    errorList.Add("Payer id is compulsory in case of Gateway EDI Clearing house.");
                                }
                                if (!string.IsNullOrEmpty(SecondaryIns.Insgroup_Name) && SecondaryIns.plan_name.Trim().ToUpper().Equals("WORK COMP"))
                                {
                                    if (string.IsNullOrEmpty(SecondaryIns.Sub_Empaddress)
                                            || string.IsNullOrEmpty(SecondaryIns.Sub_Emp_City)
                                            || string.IsNullOrEmpty(SecondaryIns.Sub_Emp_State)
                                            || string.IsNullOrEmpty(SecondaryIns.Sub_Emp_Zip))
                                    {
                                        errorList.Add("Payer is Worker Company, so its subscriber employer’s address is necessary.");

                                    }
                                    strBatchString += "~";
                                    //segmentCount++;
                                }
                            }

                            if (otherIns != null)
                            {
                                if (string.IsNullOrEmpty(otherIns.plan_name))
                                {
                                    errorList.Add("Payer name missing.");
                                }
                                string paperPayerName = "";
                                string paperPayerID = "";
                                if (!string.IsNullOrEmpty(otherIns.plan_name) && otherIns.plan_name.Trim().ToUpper().Equals("MEDICARE"))
                                {
                                    paperPayerName = "MEDICARE";
                                }
                                else
                                {
                                    paperPayerName = otherIns.plan_name;
                                }

                                paperPayerID = otherIns.Payer_Number;
                                if (!string.IsNullOrEmpty(paperPayerID))
                                {
                                    strBatchString += "NM1*PR*2*" + paperPayerName + "*****PI*" + paperPayerID + "~";
                                    segmentCount++;
                                }
                                else
                                {
                                    errorList.Add("Payer id is compulsory in case of Gateway EDI Clearing house.");
                                }
                                if (!string.IsNullOrEmpty(otherIns.Insgroup_Name) && otherIns.plan_name.Trim().ToUpper().Equals("WORK COMP"))
                                {
                                    if (string.IsNullOrEmpty(otherIns.Sub_Empaddress)
                                            || string.IsNullOrEmpty(otherIns.Sub_Emp_City)
                                            || string.IsNullOrEmpty(otherIns.Sub_Emp_State)
                                            || string.IsNullOrEmpty(otherIns.Sub_Emp_Zip))
                                    {
                                        errorList.Add("Payer is Worker Company, so its subscriber employer’s address is necessary.");

                                    }
                                    strBatchString += "~";
                                    //segmentCount++;
                                }
                            }

                            int P = HL;
                            HL = HL + 1;
                            int CHILD = 0;
                            #region HL: SUBSCRIBER HIERARCHICAL LEVEL
                            strBatchString += "HL*";
                            strBatchString += HL + "*" + P + "*";
                            strBatchString += "21*" + "1" + "~";
                            segmentCount++;
                            #endregion
                            #region LOOP 1000B (RECEIVER NAME)
                            strBatchString += "NM1*41*2*TRIZETTO*****46" + "*263923727" + "~";
                            segmentCount++;
                            #endregion
                            HL = HL + 1;
                            P = P + 1;
                            #region HL: SUBSCRIBER HIERARCHICAL LEVEL
                            strBatchString += "HL*";
                            strBatchString += HL + "*" + P + "*";
                            strBatchString += "19*" + "1" + "~";
                            segmentCount++;
                            #endregion
                            #endregion

                            #region NM1 IP Billing Provider Name
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


                            #region NM1 1P LOOP 2310B (RENDERING PROVIDER)
                            if (claim.claimInfo.Attending_Physician != null)
                            {
                                if (!string.IsNullOrEmpty(claim.claimInfo.Att_Npi))
                                {

                                    if (!isAlphaNumeric(claim.claimInfo.Att_Lname)
                                            && !isAlphaNumeric(claim.claimInfo.Att_Fname))
                                    {
                                        errorList.Add("Rendering provider’s Name must be Alpha Numeric.");
                                    }
                                    else
                                    {
                                        strBatchString += "NM1*1P*1*" + claim.claimInfo.Att_Lname + "*"
                                                + claim.claimInfo.Att_Fname + "****XX*"
                                                + claim.claimInfo.Att_Npi + "~";

                                        segmentCount++;
                                    }

                                }
                                else
                                {
                                    errorList.Add("Rendering Provider NPI Missing.");

                                }
                            }
                            else
                            {
                                errorList.Add("Rendering Provider Information missing..");
                            }
                            #endregion
                            #endregion
                            if (primaryIns != null)
                            {
                                #region For HL 4
                                if
                                (
                                 !primaryIns.GRelationship.Trim().ToUpper().Equals("S")
                                 && !string.IsNullOrEmpty(primaryIns.GRelationship))
                                {
                                    #region HL : (PATIENT HIERARCHICAL LEVEL)
                                    int PHL = HL;
                                    HL++;
                                    strBatchString += "HL*" + HL + "*" + PHL + "*22*1~";
                                    segmentCount++;
                                    #endregion
                                }
                                else
                                {
                                    #region HL : (PATIENT HIERARCHICAL LEVEL)
                                    int PHL = HL;
                                    HL++;
                                    strBatchString += "HL*" + HL + "*" + PHL + "*22*0~";
                                    segmentCount++;
                                    #endregion
                                }

                                #endregion
                            }
                            else if (SecondaryIns != null)
                            {
                                {
                                    #region For HL 4

                                    if
                                    (
                                     !SecondaryIns.GRelationship.Trim().ToUpper().Equals("S")
                                     && !string.IsNullOrEmpty(SecondaryIns.GRelationship))
                                    {
                                        #region HL : (PATIENT HIERARCHICAL LEVEL)
                                        int PHL = HL;
                                        HL++;
                                        strBatchString += "HL*" + HL + "*" + PHL + "*22*1~";
                                        segmentCount++;
                                        #endregion
                                    }
                                    else
                                    {
                                        #region HL : (PATIENT HIERARCHICAL LEVEL)
                                        int PHL = HL;
                                        HL++;
                                        strBatchString += "HL*" + HL + "*" + PHL + "*22*0~";
                                        segmentCount++;
                                        #endregion
                                    }
                                }
                                if (otherIns != null)
                                {
                                    if
                                    (
                                     !otherIns.GRelationship.Trim().ToUpper().Equals("S")
                                     && !string.IsNullOrEmpty(otherIns.GRelationship))
                                    {
                                        #region HL : (PATIENT HIERARCHICAL LEVEL)
                                        int PHL = HL;
                                        HL++;
                                        strBatchString += "HL*" + HL + "*" + PHL + "*22*1~";
                                        segmentCount++;
                                        #endregion
                                    }
                                    else
                                    {
                                        #region HL : (PATIENT HIERARCHICAL LEVEL)
                                        int PHL = HL;
                                        HL++;
                                        strBatchString += "HL*" + HL + "*" + PHL + "*22*0~";
                                        segmentCount++;
                                        #endregion
                                    }
                                }
                                #endregion

                            }
                            else if (otherIns != null)
                            {
                                {
                                    #region For HL 4

                                    if
                                    (
                                     !otherIns.GRelationship.Trim().ToUpper().Equals("S")
                                     && !string.IsNullOrEmpty(otherIns.GRelationship))
                                    {
                                        #region HL : (PATIENT HIERARCHICAL LEVEL)
                                        int PHL = HL;
                                        HL++;
                                        strBatchString += "HL*" + HL + "*" + PHL + "*22*1~";
                                        segmentCount++;
                                        #endregion
                                    }
                                    else
                                    {
                                        #region HL : (PATIENT HIERARCHICAL LEVEL)
                                        int PHL = HL;
                                        HL++;
                                        strBatchString += "HL*" + HL + "*" + PHL + "*22*0~";
                                        segmentCount++;
                                        #endregion
                                    }
                                }
                                #endregion

                            }
                            string SBR02 = "18";
                            if (primaryIns != null)
                            {
                                #region DMG Date
                                //string SBR02 = "18";
                                if (
                                    !primaryIns.GRelationship.Trim().ToUpper().Equals("S")
                                    && !string.IsNullOrEmpty(primaryIns.GRelationship))
                                {
                                    SBR02 = "";
                                    CHILD = 1;
                                }
                                if (primaryIns.GRelationship.Trim().ToUpper().Equals("S")
                                    && !string.IsNullOrEmpty(primaryIns.GRelationship))
                                {
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
                                }
                                #endregion
                            }
                            else if (SecondaryIns != null)
                            {
                                #region DMG Date
                                //string SBR02 = "18";
                                if (
                                    !SecondaryIns.GRelationship.Trim().ToUpper().Equals("S")
                                    && !string.IsNullOrEmpty(SecondaryIns.GRelationship))
                                {
                                    SBR02 = "";
                                    CHILD = 1;
                                }
                                if (SecondaryIns.GRelationship.Trim().ToUpper().Equals("S")
                                    && !string.IsNullOrEmpty(SecondaryIns.GRelationship))
                                {
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
                                }
                                #endregion

                            }
                            else if (otherIns != null)
                            {
                                #region DMG Date
                                //string SBR02 = "18";
                                if (
                                    !otherIns.GRelationship.Trim().ToUpper().Equals("S")
                                    && !string.IsNullOrEmpty(otherIns.GRelationship))
                                {
                                    SBR02 = "";
                                    CHILD = 1;
                                }
                                if (otherIns.GRelationship.Trim().ToUpper().Equals("S")
                                    && !string.IsNullOrEmpty(otherIns.GRelationship))
                                {
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
                                }
                                #endregion

                            }


                            #region LOOP 2000BA (SUBSCRIBER Information)
                            #region  NM1*IL
                            if (primaryIns != null)
                            {
                                strBatchString += "NM1*IL*1*";
                                if ((string.IsNullOrEmpty(primaryIns.Glname)
                                || string.IsNullOrEmpty(primaryIns.Gfname))
                                && string.IsNullOrEmpty(primaryIns.GRelationship)
                                && !primaryIns.GRelationship.Trim().ToUpper().Equals("S"))
                                {
                                    errorList.Add("Subscriber Last/First Name missing.");
                                }
                                //Entering Subscriber Information if Relationship is SELF-----
                                //string SBR02 = "18";
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
                                        if (claim.claimInfo.Mname == " ")
                                        {
                                            strBatchString += claim.claimInfo.Lname + "*"
                                                    + claim.claimInfo.Fname + "*" + "***MI*"
                                                    //+ claim.claimInfo.Mname + "***MI*"
                                                    + primaryIns.Policy_Number.ToUpper() + "~";
                                            segmentCount++;
                                        }
                                        else
                                        {
                                            strBatchString += claim.claimInfo.Lname + "*"
                                                     + claim.claimInfo.Fname + "*"
                                                     + claim.claimInfo.Mname + "***MI*"
                                                     + primaryIns.Policy_Number.ToUpper() + "~";
                                            segmentCount++;
                                        }
                                    }

                                }
                                else //---Entering Subscriber Information In case of other than SELF---------
                                {
                                    strBatchString += primaryIns.Glname + "*"
                                            + primaryIns.Gfname + "*"
                                            + primaryIns.Gmi + "***MI*"
                                            + primaryIns.Policy_Number.ToUpper() + "~";
                                    segmentCount++;
                                    #endregion

                                    #region  HL 
                                    int PHL = HL;
                                    HL++;
                                    strBatchString += "HL*" + HL + "*" + PHL + "*23~";
                                    segmentCount++;
                                    #endregion

                                    #region  DMG
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
                                    #endregion

                                    #region PATIENT NAME INFORMATION
                                    strBatchString += "NM1*QC*1*";
                                    //----ENTERING PATIENT INFORMATION NOW------------
                                    strBatchString += claim.claimInfo.Lname + "*";
                                    strBatchString += claim.claimInfo.Fname + "~";
                                    //strBatchString += claim.claimInfo.Mname + "~";
                                    segmentCount++;

                                    if (string.IsNullOrEmpty(claim.claimInfo.Gender.ToString()))
                                    {
                                        errorList.Add("Patient gender missing.");
                                    }
                                    #endregion
                                }
                                #endregion

                            }
                            else if (SecondaryIns != null)
                            {
                                strBatchString += "NM1*IL*1*";
                                if ((string.IsNullOrEmpty(SecondaryIns.Glname)
                                || string.IsNullOrEmpty(SecondaryIns.Gfname))
                                && string.IsNullOrEmpty(SecondaryIns.GRelationship)
                                && !SecondaryIns.GRelationship.Trim().ToUpper().Equals("S"))
                                {
                                    errorList.Add("Subscriber Last/First Name missing.");
                                }
                                //Entering Subscriber Information if Relationship is SELF-----
                                //string SBR02 = "18";
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
                                        if (claim.claimInfo.Mname == " ")
                                        {
                                            strBatchString += claim.claimInfo.Lname + "*"
                                                    + claim.claimInfo.Fname + "*" + "***MI*"
                                                    //+ claim.claimInfo.Mname + "***MI*"
                                                    + SecondaryIns.Policy_Number.ToUpper() + "~";
                                            segmentCount++;
                                        }
                                        else
                                        {
                                            strBatchString += claim.claimInfo.Lname + "*"
                                                     + claim.claimInfo.Fname + "*"
                                                     + claim.claimInfo.Mname + "***MI*"
                                                     + SecondaryIns.Policy_Number.ToUpper() + "~";
                                            segmentCount++;
                                        }
                                    }

                                }
                                else //---Entering Subscriber Information In case of other than SELF---------
                                {
                                    strBatchString += SecondaryIns.Glname + "*"
                                            + SecondaryIns.Gfname + "*"
                                            + SecondaryIns.Gmi + "***MI*"
                                            + SecondaryIns.Policy_Number.ToUpper() + "~";
                                    segmentCount++;

                                    #region  HL 
                                    int PHL = HL;
                                    HL++;
                                    strBatchString += "HL*" + HL + "*" + PHL + "*23~";
                                    segmentCount++;
                                    #endregion
                                    #region  DMG
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
                                    #endregion
                                    #region PATIENT NAME INFORMATION
                                    strBatchString += "NM1*QC*1*";
                                    //----ENTERING PATIENT INFORMATION NOW------------
                                    strBatchString += claim.claimInfo.Lname + "*";
                                    strBatchString += claim.claimInfo.Fname + "~";
                                    //strBatchString += claim.claimInfo.Mname + "~";
                                    segmentCount++;

                                    if (string.IsNullOrEmpty(claim.claimInfo.Gender.ToString()))
                                    {
                                        errorList.Add("Patient gender missing.");
                                    }
                                    #endregion
                                }

                            }
                            else if (otherIns != null)
                            {
                                strBatchString += "NM1*IL*1*";
                                if ((string.IsNullOrEmpty(otherIns.Glname)
                                || string.IsNullOrEmpty(otherIns.Gfname))
                                && string.IsNullOrEmpty(otherIns.GRelationship)
                                && !otherIns.GRelationship.Trim().ToUpper().Equals("S"))
                                {
                                    errorList.Add("Subscriber Last/First Name missing.");
                                }
                                //Entering Subscriber Information if Relationship is SELF-----
                                //string SBR02 = "18";
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
                                        if (claim.claimInfo.Mname == " ")
                                        {
                                            strBatchString += claim.claimInfo.Lname + "*"
                                                    + claim.claimInfo.Fname + "*" + "***MI*"
                                                    //+ claim.claimInfo.Mname + "***MI*"
                                                    + otherIns.Policy_Number.ToUpper() + "~";
                                            segmentCount++;
                                        }
                                        else
                                        {
                                            strBatchString += claim.claimInfo.Lname + "*"
                                                     + claim.claimInfo.Fname + "*"
                                                     + claim.claimInfo.Mname + "***MI*"
                                                     + otherIns.Policy_Number.ToUpper() + "~";
                                            segmentCount++;
                                        }
                                    }

                                }
                                else //---Entering Subscriber Information In case of other than SELF---------
                                {
                                    strBatchString += otherIns.Glname + "*"
                                            + otherIns.Gfname + "*"
                                            + otherIns.Gmi + "***MI*"
                                            + otherIns.Policy_Number.ToUpper() + "~";
                                    segmentCount++;

                                    #region  HL 
                                    int PHL = HL;
                                    HL++;
                                    strBatchString += "HL*" + HL + "*" + PHL + "*23~";
                                    segmentCount++;
                                    #endregion
                                    #region  DMG
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
                                    #endregion
                                    #region PATIENT NAME INFORMATION
                                    strBatchString += "NM1*QC*1*";
                                    //----ENTERING PATIENT INFORMATION NOW------------
                                    strBatchString += claim.claimInfo.Lname + "*";
                                    strBatchString += claim.claimInfo.Fname + "~";
                                    //strBatchString += claim.claimInfo.Mname + "~";
                                    segmentCount++;

                                    if (string.IsNullOrEmpty(claim.claimInfo.Gender.ToString()))
                                    {
                                        errorList.Add("Patient gender missing.");
                                    }
                                    #endregion
                                }

                            }
                            #endregion  NM1*IL

                            #region TRN Sgment
                            strBatchString += "TRN*1*";
                            strBatchString += res.obj[0].SerialNumber + "~";
                            segmentCount++;
                            #endregion

                            #region REF
                            if (sPClaimData != null && sPClaimData.Count > 0)
                            {
                                if (!string.IsNullOrEmpty(sPClaimData[0].Pat_Acc.ToString()))
                                {
                                    strBatchString += "REF*EJ*" + sPClaimData[0].clm + "~";
                                    segmentCount++;
                                }
                            }
                            #endregion

                            #region AMT
                            strBatchString += "AMT*T3*";

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
                            strBatchString += string.Format("{0:0.00}", total_amount); // 5010
                            strBatchString += "~";
                            segmentCount++;
                            //#region LOOP 2300 (DATE - DISCHARGE)
                            int isErrorInAccident = 0;
                            #endregion

                            #region SERVICE Date DTP
                            if (claim.claimProcedures.Count > 0)
                            {

                                strBatchString += "DTP*472*RD8*";
                                string[] splittedFROMDOS = claim.claimProcedures[0].DOSCfrom.Split('/');
                                string[] splittedTODOS = claim.claimProcedures[0].DOSCto.Split('/');
                                string Date_Of_Service_FROM = splittedFROMDOS[0] + splittedFROMDOS[1] + splittedFROMDOS[2];
                                string Date_Of_Service_TO = splittedTODOS[0] + splittedTODOS[1] + splittedTODOS[2];
                                strBatchString += Date_Of_Service_FROM + "-" + Date_Of_Service_TO + "~";
                                segmentCount++;
                            }
                            else
                            {
                                errorList.Add("Service Date is missing.");
                            }
                            #endregion

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

                            #region Footer
                            if (errorList.Count == 0)
                            {
                                segmentCount++;
                                strBatchString += "SE*" + segmentCount + "*00001~GE*1*" + $"{res2.obj[0].SequancerNumber}~" +
                                    $"IEA*1*{res1.obj[0].SequancerNumber}~";

                                objResponse.Status = "Success";
                                objResponse.Response = strBatchString;

                            }
                            else
                            {
                                objResponse.Status = "Error";
                                //objResponse.Response = null;
                                objResponse.Response = errorList;
                            }
                            #endregion
                            InsertIntoCSIRequestsData(strBatchString, practice_id, unique_name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objResponse;

        }



        private bool isAlphaNumeric(string value)
        {
            Regex regxAlphaNum = new Regex("^[a-zA-Z0-9 ]*$");

            return regxAlphaNum.IsMatch(value);
        }
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
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
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
                            result.ConditionCodes.Add(reader1["ConditionCode"].ToString());
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

                                Amount = Decimal.Round(Convert.ToDecimal(reader4["Amount"].ToString()), 0)

                            };
                            result.ValueCodes.Add(valueCode);
                        }
                    }
                }
            }

            return result;
        }
        public void InsertIntoCSIRequestsData(string strBatchString, long practice_id, string unique_name)
        {
            try
            {
                string isaControlNum = ExtractValueFromSegment(strBatchString, "ISA", 13);
                string trn = ExtractValueFromSegment(strBatchString, "TRN", 2);
                string CliamNo = ExtractValueFromSegment(strBatchString, "REF", 2);
                string Payer_Name = ExtractValueFromSegment(strBatchString, "NM1", 3);
                string Payer_Id = ExtractValueFromSegment(strBatchString, "NM1", 9);
                string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                //using (SqlConnection connection = new SqlConnection("Server=172.30.128.142,1984;Database=NPMQA;User Id=QAWeb;Password=*****;"))
                {
                    connection.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandType = CommandType.Text;

                        cmd.CommandText = @"INSERT INTO CSI_REQUESTS_DATA (
                            STRING_276,
                            ENTRY_DATE,
                            PRACTICE_ID,
                            USER_NAME,
                            CLAIM_NUMBER,
                            PAYER_NAME,
                            PAYER_ID,
                            ISA_CONTROL_NUM,
                            TRN,
                            GENERATOR_STATUS,
                            RESPONSE_STRING_277,
                            PARSING_STATUS,
                            STATUS_277,
                            STRING_276_999,
                            PROCESSING_DATE,
                            LINKING_STATUS,
                            ENTERED_BY,
                            EXCEPTION)
                            VALUES (
                            @STRING_276,
                            @ENTRY_DATE,
                            @PRACTICE_ID,
                            @USER_NAME,
                            @CLAIM_NUMBER,
                            @PAYER_NAME,
                            @PAYER_ID,
                            @ISA_CONTROL_NUM,
                            @TRN,
                            @GENERATOR_STATUS,
                            @RESPONSE_STRING_277,
                            @PARSING_STATUS,
                            @STATUS_277,
                            @STRING_276_999,
                            @PROCESSING_DATE,
                            @LINKING_STATUS,
                            @ENTERED_BY,
                            @EXCEPTION)";


                        cmd.Parameters.AddWithValue("@STRING_276", Truncate(strBatchString, 550));
                        cmd.Parameters.AddWithValue("@ENTRY_DATE", DateTime.Now);
                        cmd.Parameters.AddWithValue("@PRACTICE_ID", Truncate(practice_id.ToString(), 100));
                        cmd.Parameters.AddWithValue("@USER_NAME", Truncate("V313", 200));
                        cmd.Parameters.AddWithValue("@CLAIM_NUMBER", Truncate(CliamNo, 100));
                        cmd.Parameters.AddWithValue("@PAYER_NAME", Truncate(Payer_Name, 200));
                        cmd.Parameters.AddWithValue("@PAYER_ID", Truncate(Payer_Id, 100));
                        cmd.Parameters.AddWithValue("@ISA_CONTROL_NUM", Truncate(isaControlNum, 100));
                        cmd.Parameters.AddWithValue("@TRN", Truncate(trn, 100));
                        cmd.Parameters.AddWithValue("@GENERATOR_STATUS", Truncate("Success", 100));
                        cmd.Parameters.AddWithValue("@RESPONSE_STRING_277", Truncate("", 5500));
                        cmd.Parameters.AddWithValue("@PARSING_STATUS", Truncate("", 200));
                        cmd.Parameters.AddWithValue("@STATUS_277", "");
                        cmd.Parameters.AddWithValue("@STRING_276_999", Truncate("", 8));
                        cmd.Parameters.AddWithValue("@PROCESSING_DATE", DateTime.Now);
                        cmd.Parameters.AddWithValue("@LINKING_STATUS", Truncate("", 1));
                        cmd.Parameters.AddWithValue("@ENTERED_BY", unique_name);
                        cmd.Parameters.AddWithValue("@EXCEPTION", Truncate("", 200));

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting data into CSI_REQUESTS_DATA: " + ex.Message);
            }
        }
        private string Truncate(string value, int maxLength)
        {
            return value.Length > maxLength ? value.Substring(0, maxLength) : value;
        }
        private string ExtractValueFromSegment(string ediString, string segmentId, int position)
        {
            string[] segments = ediString.Split('~');
            foreach (var segment in segments)
            {
                if (segment.StartsWith(segmentId))
                {
                    string[] elements = segment.Split('*');
                    if (elements.Length > position)
                    {
                        return elements[position];
                    }
                }
            }
            return string.Empty;
        }
        private int GenerateID()
        {
            return new Random().Next(1, 100000); // Example placeholder logic
        }
    }

}
























































//using EdiFabric.Core.Model.Edi;
//using EdiFabric.Framework.Readers;
//using EdiFabric.Templates.Hipaa5010;
//using Newtonsoft.Json;
//using NPMAPI.Enums;
//using NPMAPI.Models.ViewModels;
//using NPMAPI.Models;
//using NPMAPI.Repositories;
//using Renci.SshNet;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Dynamic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using static NPMAPI.Controllers.SubmissionController;
//using System.Text.RegularExpressions;
//using System.Web.Hosting;
//using System.Configuration;
//using Microsoft.Ajax.Utilities;
//using Dapper;
//using System.Data.SqlClient;
//using System.Data;
//using System.Threading.Tasks;

//namespace NPMAPI.Services
//{

//    public class ClaimStatusService : IClaimStatusService
//    {
//        private readonly IPracticeRepository _practiceService;
//        public ClaimStatusService(IPracticeRepository practiceService)
//        {
//            _practiceService = practiceService;
//        }
//        #region   Generate TRN Serial Number
//        public ResponseModelForSerialnumber GenerateSerialNumber(int length = 8)
//        {
//            if (length < 8 || length > 10)
//            {
//                return new ResponseModelForSerialnumber
//                {
//                    Success = false,
//                    Message = "Length must be between 8 and 10 digits"
//                };
//            }

//            string serialNumber = GenerateNextSerialNumber(length);

//            if (serialNumber == null)
//            {
//                return new ResponseModelForSerialnumber
//                {
//                    Success = false,
//                    Message = "Unable to generate a unique serial number"
//                };
//            }

//            InsertProduct(serialNumber, "TRN");

//            return new ResponseModelForSerialnumber
//            {
//                Success = true,
//                Message = "Serial number generated successfully",
//                SerialNumber = serialNumber
//            };
//        }
//        private string GenerateNextSerialNumber(int length)
//        {
//            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
//            using (SqlConnection conn = new SqlConnection(connectionString))
//            {
//                conn.Open();
//                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 SerialNumber FROM ClaimStatusTrn ORDER BY SerialNumber DESC", conn))
//                {
//                    var latestSerialNumber = cmd.ExecuteScalar() as string;

//                    if (latestSerialNumber != null && latestSerialNumber.Length == length && long.TryParse(latestSerialNumber, out long number))
//                    {
//                        number++;
//                        string nextSerialNumber = number.ToString().PadLeft(length, '0');

//                        if (nextSerialNumber.Length > length)
//                        {
//                            return null;
//                        }

//                        return nextSerialNumber;
//                    }
//                    return "1".PadLeft(length, '0');
//                }
//            }
//        }
//        private void InsertProduct(string serialNumber, string name)
//        {
//            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
//            using (SqlConnection conn = new SqlConnection(connectionString))
//            {
//                conn.Open();
//                using (SqlCommand cmd = new SqlCommand("INSERT INTO ClaimStatusTrn (SerialNumber, Name) VALUES (@SerialNumber, @Name)", conn))
//                {
//                    cmd.Parameters.AddWithValue("@SerialNumber", serialNumber);
//                    cmd.Parameters.AddWithValue("@Name", name);
//                    cmd.ExecuteNonQuery();
//                }
//            }
//        }
//        #endregion
//        #region  Generate ISA AND IEA Sequancer Number
//        public ResponseModelForSerialnumber GenerateSerialNumberISA(int length = 9)
//        {
//            if (length < 8 || length > 10)
//            {
//                return new ResponseModelForSerialnumber
//                {
//                    Success = false,
//                    Message = "Length must be between 8 and 10 digits"
//                };
//            }

//            string serialNumber = GenerateNextSerialNumberISA(length);

//            if (serialNumber == null)
//            {
//                return new ResponseModelForSerialnumber
//                {
//                    Success = false,
//                    Message = "Unable to generate a unique serial number"
//                };
//            }

//            InsertProductISA(serialNumber);

//            return new ResponseModelForSerialnumber
//            {
//                Success = true,
//                Message = "Serial number generated successfully",
//                SerialNumber = serialNumber
//            };
//        }
//        private string GenerateNextSerialNumberISA(int length)
//        {
//            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
//            using (SqlConnection conn = new SqlConnection(connectionString))
//            {
//                conn.Open();
//                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 SequancerNumber FROM ClaimStatusISAIEA ORDER BY SequancerNumber DESC", conn))
//                {
//                    var latestSerialNumber = cmd.ExecuteScalar() as string;

//                    if (latestSerialNumber != null && latestSerialNumber.Length == length && long.TryParse(latestSerialNumber, out long number))
//                    {
//                        number++;
//                        string nextSerialNumber = number.ToString().PadLeft(length, '0');

//                        if (nextSerialNumber.Length > length)
//                        {
//                            return null;
//                        }

//                        return nextSerialNumber;
//                    }
//                    return "1".PadLeft(length, '0');
//                }
//            }
//        }
//        private void InsertProductISA(string sequancernumber)
//        {
//            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
//            using (SqlConnection conn = new SqlConnection(connectionString))
//            {
//                conn.Open();
//                using (SqlCommand cmd = new SqlCommand("INSERT INTO ClaimStatusISAIEA (SequancerNumber) VALUES (@SequancerNumber)", conn))
//                {
//                    cmd.Parameters.AddWithValue("@SequancerNumber", sequancernumber);
//                    cmd.ExecuteNonQuery();
//                }
//            }
//        }
//        #endregion
//        #region  Generate BHT AND GS Sequancer Number
//        public ResponseModelForSerialnumber GenerateSerialNumberBHTGS(int length = 9)
//        {
//            if (length < 8 || length > 10)
//            {
//                return new ResponseModelForSerialnumber
//                {
//                    Success = false,
//                    Message = "Length must be between 8 and 10 digits"
//                };
//            }

//            string serialNumber = GenerateNextSerialNumberBHTGS();

//            if (serialNumber == null)
//            {
//                return new ResponseModelForSerialnumber
//                {
//                    Success = false,
//                    Message = "Unable to generate a unique serial number"
//                };
//            }

//            InsertProductBHTGS(serialNumber);

//            return new ResponseModelForSerialnumber
//            {
//                Success = true,
//                Message = "Serial number generated successfully",
//                SerialNumber = serialNumber
//            };
//        }

//        private string GenerateNextSerialNumberBHTGS()
//        {
//            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
//            using (SqlConnection conn = new SqlConnection(connectionString))
//            {
//                conn.Open();
//                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 SequancerNumber FROM ClaimStatusISAIEA ORDER BY SequancerNumber DESC", conn))
//                {
//                    var latestSerialNumber = cmd.ExecuteScalar() as string;

//                    if (latestSerialNumber != null && latestSerialNumber.Length == 9 && long.TryParse(latestSerialNumber, out long number))
//                    {
//                        // Increment the second digit from the left by 1
//                        long secondDigit = (number / 100000000) % 10;
//                        secondDigit++;

//                        // Ensure the second digit stays within the limit
//                        secondDigit %= 10;

//                        // Reconstruct the next serial number
//                        string nextSerialNumber = $"2{secondDigit:D1}{number % 100000000:D7}";

//                        return nextSerialNumber;
//                    }
//                    return "222222220"; // Default starting value
//                }
//            }
//        }
//        private void InsertProductBHTGS(string sequancernumber)
//        {
//            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
//            using (SqlConnection conn = new SqlConnection(connectionString))
//            {
//                conn.Open();
//                using (SqlCommand cmd = new SqlCommand("INSERT INTO ClaimStatusBHTGSGE (SequancerNumber) VALUES (@SequancerNumber)", conn))
//                {
//                    cmd.Parameters.AddWithValue("@SequancerNumber", sequancernumber);
//                    cmd.ExecuteNonQuery();
//                }
//            }
//        }
//        #endregion
//        #region  Get Data From Stored Procedure OF TRN SeraialNumber
//        public ResponseModelSerialNumber SequanceNumber()
//        {
//            ResponseModelSerialNumber objResponse = new ResponseModelSerialNumber();
//            string storedProcedureName = "Sp_ClaimStatusTRNNumber";

//            using (var ctx = new NPMDBEntities())
//            {
//                List<ClaimStatusTRNNumberModel> claimStatusTRNNumbers = new List<ClaimStatusTRNNumberModel>();
//                var adoConnection = ctx.Database.Connection as SqlConnection;

//                if (adoConnection != null)
//                {
//                    using (IDbConnection connection = adoConnection)
//                    {
//                        connection.Open();
//                        claimStatusTRNNumbers = connection.Query<ClaimStatusTRNNumberModel>(storedProcedureName, commandType: CommandType.StoredProcedure).ToList();
//                        objResponse.obj = claimStatusTRNNumbers;
//                        objResponse.Status = "Success";
//                    }
//                }

//                return objResponse;
//            }
//        }
//        #endregion
//        #region  Get Data From Stored Procedure OF IEAISASequancerNumber
//        public ResponseModelSequancerNumber SequanceNumberISA()
//        {
//            ResponseModelSequancerNumber objResponse = new ResponseModelSequancerNumber();
//            string storedProcedureName = "Sp_ClaimStatusISAIEANumber";

//            using (var ctx = new NPMDBEntities())
//            {
//                List<ClaimStatusISAIEANumberModel> claimStatusTRNNumbers = new List<ClaimStatusISAIEANumberModel>();
//                var adoConnection = ctx.Database.Connection as SqlConnection;

//                if (adoConnection != null)
//                {
//                    using (IDbConnection connection = adoConnection)
//                    {
//                        connection.Open();
//                        claimStatusTRNNumbers = connection.Query<ClaimStatusISAIEANumberModel>(storedProcedureName, commandType: CommandType.StoredProcedure).ToList();
//                        objResponse.obj = claimStatusTRNNumbers;
//                        objResponse.Status = "Success";
//                    }
//                }

//                return objResponse;
//            }
//        }
//        #endregion
//        #region  Get Data From Stored Procedure OF BHTGEGSSequancerNumber
//        public ResponseModelSequancerNumber SequanceNumberBHT()
//        {
//            ResponseModelSequancerNumber objResponse = new ResponseModelSequancerNumber();
//            string storedProcedureName = "Sp_ClaimStatusBHTGEGSNumber";

//            using (var ctx = new NPMDBEntities())
//            {
//                List<ClaimStatusISAIEANumberModel> claimStatusTRNNumbers = new List<ClaimStatusISAIEANumberModel>();
//                var adoConnection = ctx.Database.Connection as SqlConnection;

//                if (adoConnection != null)
//                {
//                    using (IDbConnection connection = adoConnection)
//                    {
//                        connection.Open();
//                        claimStatusTRNNumbers = connection.Query<ClaimStatusISAIEANumberModel>(storedProcedureName, commandType: CommandType.StoredProcedure).ToList();
//                        objResponse.obj = claimStatusTRNNumbers;
//                        objResponse.Status = "Success";
//                    }
//                }

//                return objResponse;
//            }
//        }
//        #endregion

//        #region Generate_Packet_276_File!
//        public ResponseModel GenerateBatch_276(long practice_id, long claim_id, long Insurance_Id, string unique_name)
//        {
//            //var res = read_Tasks_And_Insert("Opera scheduled Autoupdate 1675961446");
//            ResponseModel objResponse = new ResponseModel();
//            try
//            {
//                string strBatchString = "";
//                int segmentCount = 0;
//                List<string> errorList;

//                //string billingOrganizationName = "practiceName";//practiceName
//                string sumbitterId = "";
//                string submitterCompanyName = "";
//                string submitterContactPerson = "";
//                string submitterCompanyEmail = "";
//                string submitterCompanyPhone = "";
//                string batchId = "";
//                long subId = 0;

//                errorList = new List<string>();

//                List<spGetBatchCompanyDetails_Result> batchCompanyInfo = null;
//                List<spGetBatchClaimsInfo_Result> batchClaimInfo = null;
//                List<spGetBatchClaimsDiagnosis_Result> batchClaimDiagnosis = null;
//                List<spGetBatchClaimsProcedurestest_Result> batchClaimProcedures = null;
//                List<spGetBatchClaimsInsurancesInfo_Result> insuraceInfo = null;
//                //List<Sp_ClaimStatusTRNNumber> TrnNumber = null;
//                List<Claim_Insurance> claim_Insurances = null;
//                List<SPDataModel> sPDataModels = null;
//                //ClaimsDataModel claim_Result = null;

//                List<ClaimSubmissionModel> claimSubmissionInfo = new List<ClaimSubmissionModel>();

//                using (var ctx = new NPMDBEntities())
//                {
//                    batchCompanyInfo = ctx.spGetBatchCompanyDetails(practice_id.ToString()).ToList();
//                }

//                if (batchCompanyInfo != null && batchCompanyInfo.Count > 0)
//                {
//                    sumbitterId = batchCompanyInfo[0].Submitter_Id;
//                    submitterCompanyName = batchCompanyInfo[0].Company_Name;
//                    submitterContactPerson = batchCompanyInfo[0].Contact_Person;
//                    submitterCompanyEmail = batchCompanyInfo[0].Company_Email;
//                    submitterCompanyPhone = batchCompanyInfo[0].Company_Phone;
//                }

//                if (string.IsNullOrEmpty(sumbitterId))
//                {
//                    errorList.Add("Patient Submitter ID is missing.");
//                }
//                if (string.IsNullOrEmpty(submitterCompanyName))
//                {
//                    errorList.Add("Company ClearingHouse information is missing.");
//                }
//                if (string.IsNullOrEmpty(submitterCompanyEmail) && string.IsNullOrEmpty(submitterCompanyPhone))
//                {
//                    errorList.Add("Submitter Contact Information is Missing.");
//                }


//                ResponseModelSerialNumber res = new ResponseModelSerialNumber();
//                ResponseModelSequancerNumber res1 = new ResponseModelSequancerNumber();
//                ResponseModelSequancerNumber res2 = new ResponseModelSequancerNumber();
//                if (errorList.Count == 0)
//                {
//                    using (var ctx = new NPMDBEntities())
//                    {
//                        batchClaimInfo = ctx.spGetBatchClaimsInfo(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
//                        batchClaimDiagnosis = ctx.spGetBatchClaimsDiagnosis(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
//                        batchClaimProcedures = ctx.spGetBatchClaimsProcedurestest(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
//                        insuraceInfo = ctx.spGetBatchClaimsInsurancesInfo(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
//                        //claim_Result = GetClaimsData(claim_id.ToString());
//                        claim_Insurances = ctx.Claim_Insurance.Where(c => c.Claim_No == claim_id).ToList();
//                        var filtered_Data = insuraceInfo.Where(i => i.Insurance_Id == Insurance_Id).ToList();
//                        insuraceInfo = filtered_Data;
//                        var originalclaim = ctx.Claims.Where(x => x.Claim_No == claim_id).FirstOrDefault();
//                        sPDataModels = getSpResult(claim_id.ToString(), "P").ToList();
//                        res = SequanceNumber();
//                        res1 = SequanceNumberISA();
//                        res2 = SequanceNumberBHT();
//                    }


//                    foreach (var claim in batchClaimInfo)
//                    {

//                        if (claim.Patient_Id == null)
//                        {
//                            errorList.Add("Patient identifier is missing. DOS:" + claim.Dos);
//                        }
//                        else if (claim.Billing_Physician == null)
//                        {
//                            errorList.Add("Billing Physician identifier is missing. DOS:" + claim.Dos);
//                        }

//                        IEnumerable<spGetBatchClaimsInsurancesInfo_Result> claimInsurances = (from ins in insuraceInfo
//                                                                                              where ins.Claim_No == claim.Claim_No
//                                                                                              select ins).ToList();

//                        spGetBatchClaimsDiagnosis_Result claimDiagnosis = (from spGetBatchClaimsDiagnosis_Result diag in batchClaimDiagnosis
//                                                                           where diag.Claim_No == claim.Claim_No
//                                                                           select diag).FirstOrDefault();

//                        IEnumerable<SPDataModel> data = (from SPDataModel resId in sPDataModels
//                                                         where resId.clm == claim.Claim_No.ToString()
//                                                         select resId).ToList();

//                        IEnumerable<spGetBatchClaimsProcedurestest_Result> claimProcedures = (from spGetBatchClaimsProcedurestest_Result proc in batchClaimProcedures
//                                                                                              where proc.Claim_No == claim.Claim_No
//                                                                                              select proc).ToList();

//                        ClaimSubmissionModel claimSubmissionModel = new ClaimSubmissionModel();
//                        claimSubmissionModel.claim_No = claim.Claim_No;
//                        claimSubmissionModel.claimInfo = claim;
//                        claimSubmissionModel.claimInsurance = claimInsurances as List<spGetBatchClaimsInsurancesInfo_Result>;
//                        claimSubmissionModel.claimDiagnosis = claimDiagnosis as spGetBatchClaimsDiagnosis_Result;
//                        //claimSubmissionModel.getReasonId = reason as USP_GetReasonId_Result;
//                        claimSubmissionModel.sPDataModel = data as List<SPDataModel>;
//                        claimSubmissionModel.claimProcedures = claimProcedures as List<spGetBatchClaimsProcedurestest_Result>;
//                        List<uspGetBatchClaimsProviderPayersDataFromUSP_Result> claimBillingProviderPayerInfo;
//                        foreach (var ins in claimInsurances)
//                        {
//                            if (ins.Insurace_Type.Trim().ToUpper().Equals("P") && ins.Inspayer_Id != null)//primary
//                            {

//                                using (var ctx = new NPMDBEntities())
//                                {
//                                    claimBillingProviderPayerInfo = ctx.uspGetBatchClaimsProviderPayersDataFromUSP(ins.Inspayer_Id.ToString(), claim.Claim_No.ToString(), "CLAIM_ID").ToList();

//                                    if (claimBillingProviderPayerInfo != null && claimBillingProviderPayerInfo.Count > 0)
//                                    {
//                                        claimSubmissionModel.claimBillingProviderPayer = claimBillingProviderPayerInfo[0];
//                                    }
//                                }
//                                break;
//                            }
//                        }

//                        claimSubmissionInfo.Add(claimSubmissionModel);

//                    }

//                    if (claimSubmissionInfo.Count > 0)
//                    {
//                        batchId = claimSubmissionInfo[0].claim_No.ToString();
//                        string dateTime_yyMMdd = DateTime.Now.ToString("yyMMdd");
//                        string dateTime_yyyyMMdd = DateTime.Now.ToString("yyyyMMdd");
//                        string dateTime_HHmm = DateTime.Now.ToString("HHmm");
//                        segmentCount = 0;

//                        #region ISA Header
//                        strBatchString = "ISA*";
//                        strBatchString += "00*          " + "*00*" + "          *" + "ZZ" + "*V313           ".PadRight(15) + "*ZZ*263923727      *";
//                        strBatchString += dateTime_yyMMdd + "*";
//                        strBatchString += dateTime_HHmm + "*";
//                        strBatchString += $"^*00501*";
//                        strBatchString += res1.obj[0].SequancerNumber + "*";
//                        strBatchString += "0*P*:~";
//                        #endregion  ISA Header
//                        #region GS
//                        strBatchString += "GS*HR*V313*";
//                        strBatchString += "263923727" + "*";
//                        strBatchString += dateTime_yyyyMMdd + "*";
//                        strBatchString += dateTime_HHmm + "*";
//                        strBatchString += res2.obj[0].SequancerNumber + "*";
//                        strBatchString += "X*005010X212~";
//                        #endregion
//                        #region  ST
//                        strBatchString += "ST*276*00001*005010X212~";
//                        segmentCount++;
//                        #endregion
//                        #region BHT
//                        strBatchString += $"BHT*0010*13*";
//                        strBatchString += res2.obj[0].SequancerNumber + "*";
//                        strBatchString += dateTime_yyyyMMdd + "*";
//                        strBatchString += dateTime_HHmm + "~";
//                        segmentCount++;
//                        #endregion
//                        #region HL1 BILLING PROVIDER HIERARCHICAL LEVEL
//                        int HL = 1;
//                        strBatchString += "HL*" + HL + "*";
//                        strBatchString += "*20*1~";
//                        segmentCount++;
//                        #endregion


//                        spGetBatchClaimsInsurancesInfo_Result primaryIns = null;
//                        spGetBatchClaimsInsurancesInfo_Result SecondaryIns = null;
//                        spGetBatchClaimsInsurancesInfo_Result otherIns = null;


//                        foreach (var claim in claimSubmissionInfo)
//                        {
//                            var claimid = claim.claim_No;
//                            List<SPDataModel> sPClaimData = new List<SPDataModel>();
//                            sPClaimData = claim.sPDataModel.Where(p => p.clm == claimid.ToString()).ToList();
//                            long patientId = (long)claim.claimInfo.Patient_Id;
//                            long claimId = claim.claimInfo.Claim_No;
//                            string DOS = claim.claimInfo.Dos;
//                            string patientName = claim.claimInfo.Lname + ", " + claim.claimInfo.Fname;
//                            string Billing_Provider_NPI = "";
//                            string TaxonomyCode = "";
//                            string FederalTaxID = "";
//                            string FederalTaxIDType = "";

//                            string box_33_type = "";

//                            #region Check If Payer Validation Expires
//                            if (claim.claimBillingProviderPayer != null)
//                            {
//                                if (string.IsNullOrEmpty(claim.claimBillingProviderPayer.Validation_Expiry_Date.ToString()) && claim.claimBillingProviderPayer.Validation_Expiry_Date.ToString() != "01/01/1900")
//                                {

//                                    string validationExpriyDate = claim.claimBillingProviderPayer.Validation_Expiry_Date.ToString();
//                                    DateTime dtExpiry = DateTime.Parse(validationExpriyDate);
//                                    DateTime dtToday = new DateTime();

//                                    if (DateTime.Compare(dtExpiry, dtToday) >= 0) // expires
//                                    {
//                                        errorList.Add("VALIDATION EXPIRED : Provider validation with the Payer has been expired.");
//                                    }

//                                }
//                            }

//                            if (claim.claimInsurance != null && claim.claimInsurance.Count > 0)
//                            {
//                                foreach (var ins in claim.claimInsurance)
//                                {
//                                    switch (ins.Insurace_Type.ToUpper().Trim())
//                                    {
//                                        case "P":
//                                            primaryIns = ins;
//                                            break;
//                                        case "S":
//                                            SecondaryIns = ins;
//                                            break;
//                                        case "O":
//                                            otherIns = ins;
//                                            break;
//                                    }
//                                }
//                            }

//                            //--End

//                            if (claim.claimInsurance == null || claim.claimInsurance.Count == 0)
//                            {
//                                errorList.Add("Patient Insurance Information is missing.");
//                            }
//                            else if (primaryIns == null)
//                            {
//                                if (SecondaryIns == null)
//                                {
//                                    if (otherIns == null)
//                                    {
//                                        errorList.Add("Patient Primary Insurance Information is missing.");
//                                    }

//                                }
//                            }
//                            else
//                            {
//                                if (!string.IsNullOrEmpty(primaryIns.GRelationship)
//                               && primaryIns.GRelationship.Trim().ToUpper().Equals("S"))
//                                {
//                                    primaryIns.Glname = claim.claimInfo.Lname;
//                                    primaryIns.Gfname = claim.claimInfo.Fname;
//                                    primaryIns.Gmi = claim.claimInfo.Mname;
//                                    primaryIns.Gaddress = claim.claimInfo.Address;
//                                    primaryIns.Gcity = claim.claimInfo.City;
//                                    primaryIns.Gdob = claim.claimInfo.Dob;
//                                    primaryIns.Ggender = claim.claimInfo.Gender.ToString();
//                                    primaryIns.Gstate = claim.claimInfo.State;
//                                    primaryIns.Gzip = claim.claimInfo.Zip;

//                                }
//                                //if (!primaryIns.GRelationship.Trim().ToUpper().Equals("S") && primaryIns.Guarantor_Id == null)
//                                //{
//                                //    errorList.Add("Subscriber information is missing.");

//                                //}
//                                if (primaryIns.Inspayer_Id == null)
//                                {
//                                    errorList.Add("Payer's information is missing.");
//                                }


//                                //---Adding Submit/RESUBMIT CLAIM CPTS-----------
//                                int line_no = 0;
//                                if (claim.claimProcedures != null && claim.claimProcedures.Count() > 0)
//                                {
//                                    foreach (var proc in claim.claimProcedures)
//                                    {

//                                        if (claim.claimInfo.Is_Resubmitted && !proc.Is_Resubmitted)
//                                        {
//                                            continue;
//                                        }

//                                        line_no = line_no + 1;
//                                    }
//                                }
//                                if (line_no == 0)
//                                {
//                                    errorList.Add("Claim Procedures missing.");
//                                }
//                                #endregion

//                                #region NMI PR LOOP 2010BB (PAYER INFORMATION)
//                                if (string.IsNullOrEmpty(primaryIns.plan_name))
//                                {
//                                    errorList.Add("Payer name missing.");
//                                }
//                                string paperPayerName = "";
//                                string paperPayerID = "";
//                                if (!string.IsNullOrEmpty(primaryIns.plan_name) && primaryIns.plan_name.Trim().ToUpper().Equals("MEDICARE"))
//                                {
//                                    paperPayerName = "MEDICARE";
//                                }
//                                else
//                                {
//                                    paperPayerName = primaryIns.plan_name;
//                                }

//                                paperPayerID = primaryIns.Payer_Number;
//                                if (!string.IsNullOrEmpty(paperPayerID))
//                                {
//                                    strBatchString += "NM1*PR*2*" + paperPayerName + "*****PI*" + paperPayerID + "~";
//                                    segmentCount++;
//                                }
//                                else
//                                {
//                                    errorList.Add("Payer id is compulsory in case of Gateway EDI Clearing house.");
//                                }
//                                if (!string.IsNullOrEmpty(primaryIns.Insgroup_Name) && primaryIns.plan_name.Trim().ToUpper().Equals("WORK COMP"))
//                                {
//                                    if (string.IsNullOrEmpty(primaryIns.Sub_Empaddress)
//                                            || string.IsNullOrEmpty(primaryIns.Sub_Emp_City)
//                                            || string.IsNullOrEmpty(primaryIns.Sub_Emp_State)
//                                            || string.IsNullOrEmpty(primaryIns.Sub_Emp_Zip))
//                                    {
//                                        errorList.Add("Payer is Worker Company, so its subscriber employer’s address is necessary.");

//                                    }
//                                    strBatchString += "~";
//                                    //segmentCount++;
//                                }
//                                int P = HL;
//                                HL = HL + 1;
//                                int CHILD = 0;
//                                #region HL: SUBSCRIBER HIERARCHICAL LEVEL
//                                strBatchString += "HL*";
//                                strBatchString += HL + "*" + P + "*";
//                                strBatchString += "21*" + "1" + "~";
//                                segmentCount++;
//                                #endregion
//                                #region LOOP 1000B (RECEIVER NAME)
//                                strBatchString += "NM1*41*2*TRIZETTO*****46" + "*263923727" + "~";
//                                segmentCount++;
//                                #endregion
//                                HL = HL + 1;
//                                P = P + 1;
//                                #region HL: SUBSCRIBER HIERARCHICAL LEVEL
//                                strBatchString += "HL*";
//                                strBatchString += HL + "*" + P + "*";
//                                strBatchString += "19*" + "1" + "~";
//                                segmentCount++;
//                                #endregion
//                                #endregion

//                                #region NM1 IP Billing Provider Name
//                                #region Provider NPI/Group NPI on the basis of Box 33 Type . Group or Individual | Federal Tax ID | Box33                         
//                                if (claim.claimBillingProviderPayer != null)
//                                {
//                                    if (!string.IsNullOrEmpty(claim.claimBillingProviderPayer.Provider_Identification_Number_Type)
//                                        && !string.IsNullOrEmpty(claim.claimBillingProviderPayer.Provider_Identification_Number))
//                                    {

//                                        FederalTaxIDType = claim.claimBillingProviderPayer.Provider_Identification_Number_Type;
//                                        FederalTaxID = claim.claimBillingProviderPayer.Provider_Identification_Number;
//                                    }

//                                    if (!string.IsNullOrEmpty(claim.claimBillingProviderPayer.Box_33_Type))
//                                    {
//                                        box_33_type = claim.claimBillingProviderPayer.Box_33_Type;
//                                    }
//                                }
//                                if (string.IsNullOrEmpty(FederalTaxIDType) || string.IsNullOrEmpty(FederalTaxID))
//                                {
//                                    FederalTaxIDType = claim.claimInfo.Federal_Taxidnumbertype;
//                                    FederalTaxID = claim.claimInfo.Federal_Taxid;
//                                }



//                                if (string.IsNullOrEmpty(box_33_type))
//                                {
//                                    switch (FederalTaxIDType)
//                                    {
//                                        case "EIN": // Group
//                                            box_33_type = "GROUP";
//                                            break;
//                                        case "SSN": // Individual
//                                            box_33_type = "INDIVIDUAL";
//                                            break;
//                                    }
//                                }
//                                switch (box_33_type)
//                                {
//                                    case "GROUP": // Group  
//                                        if (!string.IsNullOrEmpty(claim.claimInfo.Bl_Group_Npi))
//                                        {
//                                            Billing_Provider_NPI = claim.claimInfo.Bl_Group_Npi;
//                                        }
//                                        if (!string.IsNullOrEmpty(claim.claimInfo.Grp_Taxonomy_Id))
//                                        {
//                                            TaxonomyCode = claim.claimInfo.Grp_Taxonomy_Id;
//                                        }
//                                        break;
//                                    case "INDIVIDUAL": // Individual
//                                        if (!string.IsNullOrEmpty(claim.claimInfo.Bl_Npi))
//                                        {
//                                            Billing_Provider_NPI = claim.claimInfo.Bl_Npi;
//                                        }

//                                        if (!string.IsNullOrEmpty(claim.claimInfo.Taxonomy_Code))
//                                        {
//                                            TaxonomyCode = claim.claimInfo.Taxonomy_Code;
//                                        }
//                                        break;
//                                }
//                                #endregion


//                                #region NM1 1P LOOP 2310B (RENDERING PROVIDER)
//                                if (claim.claimInfo.Attending_Physician != null)
//                                {
//                                    if (!string.IsNullOrEmpty(claim.claimInfo.Att_Npi))
//                                    {

//                                        if (!isAlphaNumeric(claim.claimInfo.Att_Lname)
//                                                && !isAlphaNumeric(claim.claimInfo.Att_Fname))
//                                        {
//                                            errorList.Add("Rendering provider’s Name must be Alpha Numeric.");
//                                        }
//                                        else
//                                        {
//                                            strBatchString += "NM1*1P*1*" + claim.claimInfo.Att_Lname + "*"
//                                                    + claim.claimInfo.Att_Fname + "****XX*"
//                                                    + claim.claimInfo.Att_Npi + "~";

//                                            segmentCount++;
//                                        }

//                                    }
//                                    else
//                                    {
//                                        errorList.Add("Rendering Provider NPI Missing.");

//                                    }
//                                }
//                                else
//                                {
//                                    errorList.Add("Rendering Provider Information missing..");
//                                }
//                                #endregion
//                                #endregion

//                                #region For HL 4
//                                if
//                                (
//                                 !primaryIns.GRelationship.Trim().ToUpper().Equals("S")
//                                 && !string.IsNullOrEmpty(primaryIns.GRelationship))
//                                {
//                                    #region HL : (PATIENT HIERARCHICAL LEVEL)
//                                    int PHL = HL;
//                                    HL++;
//                                    strBatchString += "HL*" + HL + "*" + PHL + "*22*1~";
//                                    segmentCount++;
//                                    #endregion
//                                }
//                                else
//                                {
//                                    #region HL : (PATIENT HIERARCHICAL LEVEL)
//                                    int PHL = HL;
//                                    HL++;
//                                    strBatchString += "HL*" + HL + "*" + PHL + "*22*0~";
//                                    segmentCount++;
//                                    #endregion
//                                }

//                                #endregion

//                                #region DMG Date
//                                string SBR02 = "18";
//                                if (
//                                    !primaryIns.GRelationship.Trim().ToUpper().Equals("S")
//                                    && !string.IsNullOrEmpty(primaryIns.GRelationship))
//                                {
//                                    SBR02 = "";
//                                    CHILD = 1;
//                                }
//                                if (primaryIns.GRelationship.Trim().ToUpper().Equals("S")
//                                    && !string.IsNullOrEmpty(primaryIns.GRelationship))
//                                {
//                                    strBatchString += "DMG*D8*";
//                                    if (string.IsNullOrEmpty(claim.claimInfo.Dob))
//                                    {
//                                        errorList.Add("Patient DOB is missing.");
//                                    }
//                                    else
//                                    {
//                                        strBatchString += !string.IsNullOrEmpty(claim.claimInfo.Dob) ? claim.claimInfo.Dob.Split('/')[0] + claim.claimInfo.Dob.Split('/')[1] + claim.claimInfo.Dob.Split('/')[2] : "";
//                                        strBatchString += "*";
//                                    }
//                                    if (string.IsNullOrEmpty(claim.claimInfo.Gender.ToString()))
//                                    {
//                                        errorList.Add("Patient Gender is missing.");
//                                    }
//                                    else
//                                    {
//                                        strBatchString += claim.claimInfo.Gender.ToString();

//                                    }
//                                    strBatchString += "~";
//                                    segmentCount++;
//                                }
//                                #endregion

//                                #region LOOP 2000BA (SUBSCRIBER Information)
//                                #region  NM1*IL
//                                strBatchString += "NM1*IL*1*";
//                                if ((string.IsNullOrEmpty(primaryIns.Glname)
//                                || string.IsNullOrEmpty(primaryIns.Gfname))
//                                && string.IsNullOrEmpty(primaryIns.GRelationship)
//                                && !primaryIns.GRelationship.Trim().ToUpper().Equals("S"))
//                                {
//                                    errorList.Add("Subscriber Last/First Name missing.");
//                                }
//                                //Entering Subscriber Information if Relationship is SELF-----
//                                if (SBR02.Equals("18"))
//                                {
//                                    if (!isAlphaNumeric(claim.claimInfo.Lname)
//                                        || !isAlphaNumeric(claim.claimInfo.Fname)
//                                        )
//                                    {
//                                        errorList.Add("Subscriber Name must be Alpha Numeric.");
//                                    }
//                                    else
//                                    {
//                                        if (claim.claimInfo.Mname == " ")
//                                        {
//                                            strBatchString += claim.claimInfo.Lname + "*"
//                                                    + claim.claimInfo.Fname + "*" + "***MI*"
//                                                    //+ claim.claimInfo.Mname + "***MI*"
//                                                    + primaryIns.Policy_Number.ToUpper() + "~";
//                                            segmentCount++;
//                                        }
//                                        else
//                                        {
//                                            strBatchString += claim.claimInfo.Lname + "*"
//                                                     + claim.claimInfo.Fname + "*"
//                                                     + claim.claimInfo.Mname + "***MI*"
//                                                     + primaryIns.Policy_Number.ToUpper() + "~";
//                                            segmentCount++;
//                                        }
//                                    }

//                                }
//                                else //---Entering Subscriber Information In case of other than SELF---------
//                                {
//                                    strBatchString += primaryIns.Glname + "*"
//                                            + primaryIns.Gfname + "*"
//                                            + primaryIns.Gmi + "***MI*"
//                                            + primaryIns.Policy_Number.ToUpper() + "~";
//                                    segmentCount++;
//                                    #endregion
//                                    #region  HL 
//                                    int PHL = HL;
//                                    HL++;
//                                    strBatchString += "HL*" + HL + "*" + PHL + "*23~";
//                                    segmentCount++;
//                                    #endregion
//                                    #region  DMG
//                                    strBatchString += "DMG*D8*";
//                                    if (string.IsNullOrEmpty(claim.claimInfo.Dob))
//                                    {
//                                        errorList.Add("Patient DOB is missing.");
//                                    }
//                                    else
//                                    {
//                                        strBatchString += !string.IsNullOrEmpty(claim.claimInfo.Dob) ? claim.claimInfo.Dob.Split('/')[0] + claim.claimInfo.Dob.Split('/')[1] + claim.claimInfo.Dob.Split('/')[2] : "";
//                                        strBatchString += "*";
//                                    }
//                                    if (string.IsNullOrEmpty(claim.claimInfo.Gender.ToString()))
//                                    {
//                                        errorList.Add("Patient Gender is missing.");
//                                    }
//                                    else
//                                    {
//                                        strBatchString += claim.claimInfo.Gender.ToString();

//                                    }
//                                    strBatchString += "~";
//                                    segmentCount++;
//                                    #endregion
//                                    #region PATIENT NAME INFORMATION
//                                    strBatchString += "NM1*QC*1*";
//                                    //----ENTERING PATIENT INFORMATION NOW------------
//                                    strBatchString += claim.claimInfo.Lname + "*";
//                                    strBatchString += claim.claimInfo.Fname + "~";
//                                    //strBatchString += claim.claimInfo.Mname + "~";
//                                    segmentCount++;

//                                    if (string.IsNullOrEmpty(claim.claimInfo.Gender.ToString()))
//                                    {
//                                        errorList.Add("Patient gender missing.");
//                                    }
//                                    #endregion
//                                }
//                                #endregion


//                            }


//                            #region TRN Sgment
//                            strBatchString += "TRN*1*";
//                            strBatchString += res.obj[0].SerialNumber + "~";
//                            segmentCount++;
//                            #endregion

//                            #region REF
//                            if (sPClaimData != null && sPClaimData.Count > 0)
//                            {
//                                if (!string.IsNullOrEmpty(sPClaimData[0].Pat_Acc.ToString()))
//                                {
//                                    strBatchString += "REF*EJ*" + sPClaimData[0].clm + "~";
//                                    segmentCount++;
//                                }
//                            }
//                            #endregion

//                            #region AMT
//                            strBatchString += "AMT*T3*";

//                            decimal total_amount = 0;

//                            if (claim.claimInfo.Is_Resubmitted)
//                            {
//                                foreach (var proc in claim.claimProcedures)
//                                {
//                                    if (proc.Is_Resubmitted)
//                                    {
//                                        total_amount = total_amount + (decimal)proc.Total_Charges;
//                                    }
//                                }

//                            }
//                            else
//                            {
//                                total_amount = claim.claimInfo.Claim_Total;
//                            }
//                            strBatchString += string.Format("{0:0.00}", total_amount); // 5010
//                            strBatchString += "~";
//                            segmentCount++;
//                            //#region LOOP 2300 (DATE - DISCHARGE)
//                            int isErrorInAccident = 0;
//                            #endregion

//                            #region SERVICE Date DTP
//                            if (claim.claimProcedures.Count > 0)
//                            {

//                                strBatchString += "DTP*472*RD8*";
//                                string[] splittedFROMDOS = claim.claimProcedures[0].DOSCfrom.Split('/');
//                                string[] splittedTODOS = claim.claimProcedures[0].DOSCto.Split('/');
//                                string Date_Of_Service_FROM = splittedFROMDOS[0] + splittedFROMDOS[1] + splittedFROMDOS[2];
//                                string Date_Of_Service_TO = splittedTODOS[0] + splittedTODOS[1] + splittedTODOS[2];
//                                strBatchString += Date_Of_Service_FROM + "-" + Date_Of_Service_TO + "~";
//                                segmentCount++;
//                            }
//                            else
//                            {

//                            }
//                            #endregion

//                            #region (REF - OTHER PAYER CLAIM CONTROL NUMBER)
//                            if (sPClaimData != null && sPClaimData.Count > 0)
//                            {
//                                if (!string.IsNullOrEmpty(sPClaimData[0].ICN))
//                                {
//                                    strBatchString += "REF*F8*";
//                                    strBatchString += sPClaimData[0].PATIENT_ACCOUNT;
//                                    strBatchString += "~";
//                                    segmentCount++;
//                                }
//                            }
//                            #endregion End!

//                            #region Footer
//                            if (errorList.Count == 0)
//                            {
//                                segmentCount++;
//                                strBatchString += "SE*" + segmentCount + "*00001~GE*1*" + $"{res2.obj[0].SequancerNumber}~" +
//                                    $"IEA*1*{res1.obj[0].SequancerNumber}~";

//                                objResponse.Status = "Success";
//                                objResponse.Response = strBatchString;

//                            }
//                            else
//                            {
//                                objResponse.Status = "Error";
//                                //objResponse.Response = null;
//                                objResponse.Response = errorList;
//                            }
//                            #endregion
//                            InsertIntoCSIRequestsData(strBatchString, practice_id, unique_name);
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//            return objResponse;

//        }



//        #endregion  End_Generation!

//        private bool isAlphaNumeric(string value)
//        {
//            Regex regxAlphaNum = new Regex("^[a-zA-Z0-9 ]*$");

//            return regxAlphaNum.IsMatch(value);
//        }
//        public List<SPDataModel> getSpResult(string claim_id, string insType)
//        {
//            ResponseModel objResponse = new ResponseModel();
//            List<SPDataModel> resultList = new List<SPDataModel>();
//            string storedProcedureName = "WEBEHR_BILLINGPRO_GET_HCFA_PRINT_DETAIL_ICD10_tets";
//            using (var ctx = new NPMDBEntities())
//            {
//                var adoConnection = ctx.Database.Connection as SqlConnection;
//                if (adoConnection != null)
//                {
//                    using (IDbConnection connection = adoConnection)
//                    {
//                        var parameters = new DynamicParameters();
//                        parameters.Add("claims", claim_id);
//                        parameters.Add("@insType", insType);
//                        resultList = connection.Query<SPDataModel>(storedProcedureName, parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
//                    }

//                }
//            }
//            if (resultList != null)
//            {
//                objResponse.Status = "Success";
//                objResponse.Response = resultList;
//            }
//            else
//            {
//                objResponse.Status = "No Data Found";
//            }
//            return resultList;
//        }
//        //public ClaimsDataModel GetClaimsData(string id)
//        //{
//        //    ClaimsDataModel result = new ClaimsDataModel();
//        //    //string connectionString = ConfigurationManager.ConnectionStrings["NPMDBEntities"].ConnectionString;
//        //    string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
//        //    //using (SqlConnection con = new SqlConnection("Server=13.64.106.147,1984;Database=NPMDevDB;User Id=EDITeam;Password=****;"))
//        //    using (SqlConnection con = new SqlConnection(connectionString))
//        //    {
//        //        con.Open();

//        //        string query1 = "SELECT ConditionCode FROM Claims_Condition_Code WHERE claim_no = @Id";
//        //        string query2 = "SELECT OccCode, Date FROM Claims_Occurrence_Code WHERE claim_no = @Id";
//        //        string query3 = "SELECT OccSpanCode, DateFrom, DateThrough FROM Claims_Occurence_Span_Code WHERE claimno = @Id";
//        //        string query4 = "SELECT Value_Codes_Id, Amount FROM Claims_Value_Code WHERE claim_no = @Id";

//        //        using (SqlCommand cmd1 = new SqlCommand(query1, con))
//        //        {
//        //            cmd1.Parameters.AddWithValue("@Id", id);
//        //            using (SqlDataReader reader1 = cmd1.ExecuteReader())
//        //            {
//        //                while (reader1.Read())
//        //                {
//        //                    result.ConditionCodes.Add(reader1["ConditionCode"].ToString());
//        //                }
//        //            }
//        //        }

//        //        using (SqlCommand cmd2 = new SqlCommand(query2, con))
//        //        {
//        //            cmd2.Parameters.AddWithValue("@Id", id);
//        //            using (SqlDataReader reader2 = cmd2.ExecuteReader())
//        //            {
//        //                while (reader2.Read())
//        //                {
//        //                    OccurrenceCodeModel occurrenceCode = new OccurrenceCodeModel
//        //                    {
//        //                        OccCode = reader2["OccCode"].ToString(),
//        //                        Date2 = Convert.ToDateTime(reader2["Date"]).ToString()
//        //                    };
//        //                    result.OccurrenceCodes.Add(occurrenceCode);
//        //                }
//        //            }
//        //        }

//        //        using (SqlCommand cmd3 = new SqlCommand(query3, con))
//        //        {
//        //            cmd3.Parameters.AddWithValue("@Id", id);
//        //            using (SqlDataReader reader3 = cmd3.ExecuteReader())
//        //            {
//        //                while (reader3.Read())
//        //                {
//        //                    OccurenceSpanModel occurrenceSpanCode = new OccurenceSpanModel
//        //                    {
//        //                        OccSpanCode = reader3["OccSpanCode"].ToString(),
//        //                        DateFrom = Convert.ToDateTime(reader3["DateFrom"]).ToString(),
//        //                        DateThrough = Convert.ToDateTime(reader3["DateThrough"]).ToString()
//        //                    };
//        //                    result.OccurrenceSpanCodes.Add(occurrenceSpanCode);
//        //                }
//        //            }
//        //        }

//        //        using (SqlCommand cmd4 = new SqlCommand(query4, con))
//        //        {
//        //            cmd4.Parameters.AddWithValue("@Id", id);
//        //            using (SqlDataReader reader4 = cmd4.ExecuteReader())
//        //            {
//        //                while (reader4.Read())
//        //                {
//        //                    ValueeCode valueCode = new ValueeCode
//        //                    {
//        //                        Value_Codes_Id = reader4["Value_Codes_Id"].ToString(),

//        //                        Amount = Decimal.Round(Convert.ToDecimal(reader4["Amount"].ToString()), 0)

//        //                    };
//        //                    result.ValueCodes.Add(valueCode);
//        //                }
//        //            }
//        //        }
//        //    }

//        //    return result;
//        //}
//        public void InsertIntoCSIRequestsData(string strBatchString, long practice_id, string unique_name)
//        {
//            try
//            {
//                string isaControlNum = ExtractValueFromSegment(strBatchString, "ISA", 13);
//                string trn = ExtractValueFromSegment(strBatchString, "TRN", 2);
//                string CliamNo = ExtractValueFromSegment(strBatchString, "REF", 2);
//                string Payer_Name = ExtractValueFromSegment(strBatchString, "NM1", 3);
//                string Payer_Id = ExtractValueFromSegment(strBatchString, "NM1", 9);
//                string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;

//                using (SqlConnection connection = new SqlConnection(connectionString))
//                //using (SqlConnection connection = new SqlConnection("Server=172.30.128.142,1984;Database=NPMQA;User Id=QAWeb;Password=*****;"))
//                {
//                    connection.Open();

//                    using (SqlCommand cmd = new SqlCommand())
//                    {
//                        cmd.Connection = connection;
//                        cmd.CommandType = CommandType.Text;

//                        cmd.CommandText = @"INSERT INTO CSI_REQUESTS_DATA (
//                            STRING_276,
//                            ENTRY_DATE,
//                            PRACTICE_ID,
//                            USER_NAME,
//                            CLAIM_NUMBER,
//                            PAYER_NAME,
//                            PAYER_ID,
//                            ISA_CONTROL_NUM,
//                            TRN,
//                            GENERATOR_STATUS,
//                            RESPONSE_STRING_277,
//                            PARSING_STATUS,
//                            STATUS_277,
//                            STRING_276_999,
//                            PROCESSING_DATE,
//                            LINKING_STATUS,
//                            ENTERED_BY,
//                            EXCEPTION)
//                            VALUES (
//                            @STRING_276,
//                            @ENTRY_DATE,
//                            @PRACTICE_ID,
//                            @USER_NAME,
//                            @CLAIM_NUMBER,
//                            @PAYER_NAME,
//                            @PAYER_ID,
//                            @ISA_CONTROL_NUM,
//                            @TRN,
//                            @GENERATOR_STATUS,
//                            @RESPONSE_STRING_277,
//                            @PARSING_STATUS,
//                            @STATUS_277,
//                            @STRING_276_999,
//                            @PROCESSING_DATE,
//                            @LINKING_STATUS,
//                            @ENTERED_BY,
//                            @EXCEPTION)";


//                        cmd.Parameters.AddWithValue("@STRING_276", Truncate(strBatchString, 550));
//                        cmd.Parameters.AddWithValue("@ENTRY_DATE", DateTime.Now);
//                        cmd.Parameters.AddWithValue("@PRACTICE_ID", Truncate(practice_id.ToString(), 100));
//                        cmd.Parameters.AddWithValue("@USER_NAME", Truncate("V313", 200));
//                        cmd.Parameters.AddWithValue("@CLAIM_NUMBER", Truncate(CliamNo, 100));
//                        cmd.Parameters.AddWithValue("@PAYER_NAME", Truncate(Payer_Name, 200));
//                        cmd.Parameters.AddWithValue("@PAYER_ID", Truncate(Payer_Id, 100));
//                        cmd.Parameters.AddWithValue("@ISA_CONTROL_NUM", Truncate(isaControlNum, 100));
//                        cmd.Parameters.AddWithValue("@TRN", Truncate(trn, 100));
//                        cmd.Parameters.AddWithValue("@GENERATOR_STATUS", Truncate("Success", 100));
//                        cmd.Parameters.AddWithValue("@RESPONSE_STRING_277", Truncate("", 5500));
//                        cmd.Parameters.AddWithValue("@PARSING_STATUS", Truncate("", 200));
//                        cmd.Parameters.AddWithValue("@STATUS_277", "");
//                        cmd.Parameters.AddWithValue("@STRING_276_999", Truncate("", 8));
//                        cmd.Parameters.AddWithValue("@PROCESSING_DATE", DateTime.Now);
//                        cmd.Parameters.AddWithValue("@LINKING_STATUS", Truncate("", 1));
//                        cmd.Parameters.AddWithValue("@ENTERED_BY", unique_name);
//                        cmd.Parameters.AddWithValue("@EXCEPTION", Truncate("", 200));

//                        cmd.ExecuteNonQuery();
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Error inserting data into CSI_REQUESTS_DATA: " + ex.Message);
//            }
//        }
//        private string Truncate(string value, int maxLength)
//        {
//            return value.Length > maxLength ? value.Substring(0, maxLength) : value;
//        }
//        private string ExtractValueFromSegment(string ediString, string segmentId, int position)
//        {
//            string[] segments = ediString.Split('~');
//            foreach (var segment in segments)
//            {
//                if (segment.StartsWith(segmentId))
//                {
//                    string[] elements = segment.Split('*');
//                    if (elements.Length > position)
//                    {
//                        return elements[position];
//                    }
//                }
//            }
//            return string.Empty;
//        }
//        private int GenerateID()
//        {
//            return new Random().Next(1, 100000); // Example placeholder logic
//        }
//    }

//}