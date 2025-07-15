using Dapper;
using EdiFabric.Core.Model.Edi.X12;
using EdiFabric.Templates.Hipaa5010;
using Microsoft.Ajax.Utilities;
using NPMAPI.Enums;
using NPMAPI.Models;
using NPMAPI.Repositories;
using NPOI.OpenXmlFormats.Spreadsheet;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace NPMAPI.Services
{
    public class GenerateBatch276Service : IGenerateBatch276Service
    {
        public string unique_name;
        public int Insurance_Id;
        #region Generate_Packet_276_File!
        private readonly IPracticeRepository _practiceService;
        public GenerateBatch276Service(IPracticeRepository practiceService)
        {
            _practiceService = practiceService;
        }
        #region Generate TRN Serial Number
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

            string serialNumber = GenerateNextSerialNumberTRN(length);

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
        private string GenerateNextSerialNumberTRN(int length)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 SerialNumber FROM BatchClaimStatusTrn ORDER BY SerialNumber DESC", conn))
                {
                    var latestSerialNumber = cmd.ExecuteScalar() as string;

                    if (string.IsNullOrEmpty(latestSerialNumber) || !long.TryParse(latestSerialNumber, out long lastSerialNum))
                    {
                        return "1".PadLeft(length, '0'); // No serial number found, start from "00000001"
                    }

                    // Increment the serial number
                    lastSerialNum++;
                    return lastSerialNum.ToString().PadLeft(length, '0');
                }
            }
        }
        private void InsertProduct(string serialNumber, string name)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO BatchClaimStatusTrn (SerialNumber, Name) VALUES (@SerialNumber, @Name)", conn))
                {
                    cmd.Parameters.AddWithValue("@SerialNumber", serialNumber);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion
        #region Generate ISA and IEA Sequence Number
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
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 SequancerNumber FROM BatchClaimStatusISAIEA ORDER BY SequancerNumber DESC", conn))
                {
                    var latestSerialNumber = cmd.ExecuteScalar() as string;

                    if (string.IsNullOrEmpty(latestSerialNumber) || !long.TryParse(latestSerialNumber, out long lastSerialNum))
                    {
                        return "1".PadRight(length, '0'); // Start from "000000001"
                    }

                    // Increment the serial number
                    lastSerialNum++;
                    return lastSerialNum.ToString().PadRight(length, '0');
                }
            }
        }

        private void InsertProductISA(string sequancernumber)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO BatchClaimStatusISAIEA (SequancerNumber) VALUES (@SequancerNumber)", conn))
                {
                    cmd.Parameters.AddWithValue("@SequancerNumber", sequancernumber);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion
        #region Generate BHT and GS Sequence Number
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

            string serialNumber = GenerateNextSerialNumberBHTGS(length);

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

        private string GenerateNextSerialNumberBHTGS(int length)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 SequancerNumber FROM BatchClaimStatusBHTGSGE ORDER BY SequancerNumber DESC", conn))
                {
                    var latestSerialNumber = cmd.ExecuteScalar() as string;

                    if (string.IsNullOrEmpty(latestSerialNumber) || !long.TryParse(latestSerialNumber, out long lastSerialNum))
                    {
                        return "222222220"; // Default start value for BHTGS
                    }

                    // Increment the serial number
                    lastSerialNum++;
                    return lastSerialNum.ToString().PadLeft(length, '0');
                }
            }
        }

        private void InsertProductBHTGS(string sequancernumber)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO BatchClaimStatusBHTGSGE (SequancerNumber) VALUES (@SequancerNumber)", conn))
                {
                    cmd.Parameters.AddWithValue("@SequancerNumber", sequancernumber);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion
        #region Get Data From Stored Procedure of TRN Serial Number
        public ResponseModelSerialNumber SequanceNumber()
        {
            ResponseModelSerialNumber objResponse = new ResponseModelSerialNumber();
            string storedProcedureName = "Sp_BatchClaimStatusTRNNumber";

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
            }

            return objResponse;
        }
        #endregion
        #region Get Data From Stored Procedure of IEA/ISA Sequence Number
        public ResponseModelSequancerNumber SequanceNumberISA()
        {
            ResponseModelSequancerNumber objResponse = new ResponseModelSequancerNumber();
            string storedProcedureName = "Sp_BatchClaimStatusISAIEANumber";

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
            }

            return objResponse;
        }
        #endregion
        #region Get Data From Stored Procedure of BHT/GS Sequence Number
        public ResponseModelSequancerNumber SequanceNumberBHT()
        {
            ResponseModelSequancerNumber objResponse = new ResponseModelSequancerNumber();
            string storedProcedureName = "Sp_BatchClaimStatusBHTGEGSNumber";

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
            }

            return objResponse;
        }
        #endregion
        public ResponseModel GenerateBatch_276277(long practice_id, long claim_id)
        {

            var response1 = GenerateSerialNumber(10);
            var response2 = GenerateSerialNumberISA(9);
            var response3 = GenerateSerialNumberBHTGS(9);


            ResponseModel objResponse = new ResponseModel();
            try
            {
                string strBatchString = "";
                int segmentCount = 0;
                List<string> errorList;

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
                //List<Sp_ClaimStatusTRNNumber> TrnNumber = null;
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
                        //var filtered_Data = insuraceInfo.Where(i => i.Insurance_Id == Insurance_Id).ToList();
                        //insuraceInfo = filtered_Data;
                        insuraceInfo = ctx.spGetBatchClaimsInsurancesInfo(practice_id.ToString(), claim_id.ToString(), "claim_id").ToList();
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
                        strBatchString += "0*T*:~";
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
                            #endregion

                            #region
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
                                            //strBatchString += "NM1*1P*1*" + claim.claimInfo.Att_Lname + "*"
                                            //        + claim.claimInfo.Att_Fname + "****XX*"
                                            //        + claim.claimInfo.Att_Npi + "~";
                                            //strBatchString += "NM1*1P*2*"
                                            //       strBatchString += "NM1*1P*1*" + claim.claimInfo.Att_Lname + "*"
                                            //        + claim.claimInfo.Att_Fname + "****XX*"
                                            //        + claim.claimInfo.Att_Npi + "~";
                                            switch (box_33_type)
                                            {
                                                case "GROUP": // Group                                                        
                                                    if (!string.IsNullOrEmpty(submitterCompanyName))
                                                    {
                                                        strBatchString += "NM1*1P*2*";
                                                        strBatchString += submitterCompanyName + "*****XX*";

                                                    }
                                                    else
                                                    {
                                                        errorList.Add("2010AA - Billing Provider Organization Name Missing.");
                                                    }

                                                    if (!string.IsNullOrEmpty(Billing_Provider_NPI))
                                                    {
                                                        strBatchString += Billing_Provider_NPI + "~";
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
                                                        strBatchString += "NM1*1P*1*";
                                                        strBatchString += claim.claimInfo.Bl_Lname + "*" + claim.claimInfo.Bl_Fname + "*" + claim.claimInfo.Bl_Mi + "***XX*";

                                                    }
                                                    else
                                                    {
                                                        errorList.Add("2010AA - Billing Provider Name Missing.");
                                                    }

                                                    if (!string.IsNullOrEmpty(Billing_Provider_NPI))
                                                    {
                                                        strBatchString += Billing_Provider_NPI + "~";
                                                    }
                                                    else
                                                    {
                                                        errorList.Add("2010AA - Billing Provider Individual NPI Missing.");
                                                    }

                                                    break;
                                            }

                                            //if (!string.IsNullOrEmpty(submitterCompanyName))
                                            //{
                                            //    strBatchString += "NM1*1P*2*";
                                            //    strBatchString += submitterCompanyName + "*****XX*";

                                            //}
                                            //else
                                            //{
                                            //    errorList.Add("2010AA - Billing Provider Organization Name Missing.");
                                            //}

                                            //if (!string.IsNullOrEmpty(Billing_Provider_NPI))
                                            //{

                                            //    strBatchString += Billing_Provider_NPI;
                                            //}
                                            //else
                                            //{
                                            //    errorList.Add("2010AA - Billing Provider Group NPI Missing.");
                                            //}


                                            //+ claim.claimInfo.Att_Lname + "*"
                                            // + claim.claimInfo.Att_Fname + "****XX*"
                                            // + claim.claimInfo.Att_Npi + "~";
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

                                #region DMG Date
                                string SBR02 = "18";
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

                                #region LOOP 2000BA (SUBSCRIBER Information)
                                #region  NM1*IL
                                strBatchString += "NM1*IL*1*";
                                if ((string.IsNullOrEmpty(primaryIns.Glname)
                                || string.IsNullOrEmpty(primaryIns.Gfname))
                                && string.IsNullOrEmpty(primaryIns.GRelationship)
                                && !primaryIns.GRelationship.Trim().ToUpper().Equals("S"))
                                {
                                    errorList.Add("Subscriber Last/First Name missing.");
                                }
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
                                    strBatchString += claim.claimInfo.Lname + "*";
                                    strBatchString += claim.claimInfo.Fname + "~";
                                    segmentCount++;

                                    if (string.IsNullOrEmpty(claim.claimInfo.Gender.ToString()))
                                    {
                                        errorList.Add("Patient gender missing.");
                                    }
                                    #endregion
                                }
                                #endregion


                            }


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
                            strBatchString += string.Format("{0:0.00}", total_amount);
                            strBatchString += "~";
                            segmentCount++;
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
                                objResponse.Response = errorList;
                            }
                            #endregion

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
        #endregion  End_Generation!
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
            //using (SqlConnection con = new SqlConnection("Server=13.64.106.147,1984;Database=******;User Id=*****;Password=******;"))
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
        private string Truncate(string value, int maxLength)
        {
            if (value == null) return "";  // Return an empty string if the input value is null
            return value.Length > maxLength ? value.Substring(0, maxLength) : value;
        }
        private string ExtractValueFromSegment(string ediString, string segmentId, int position)
        {
            if (string.IsNullOrEmpty(ediString))  // Check if ediString is null or empty
            {
                return string.Empty;  // Return an empty string if ediString is null or empty
            }

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
            return string.Empty;  // Return an empty string if no value is found
        }
        private int GenerateID()
        {
            return new Random().Next(1, 100000); // Example placeholder logic
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
                                batchName = provider.Provid_LName.Substring(0, 1).ToUpper() + provider.Provid_FName.Substring(0, 1).ToUpper() + "_" + "P" + "_" + model.Date.ToString("MMddyyyy") + "_";
                            }
                            else
                                batchName = provider.Provid_LName.Substring(0, 1).ToUpper() + provider.Provid_FName.Substring(0, 1).ToUpper() + "_" + "I" + "_" + model.Date.ToString("MMddyyyy") + "_";
                        }
                        batch = ctx.claim_batch.Where(b => b.provider_id == model.ProviderCode && b.practice_id == model.PracticeCode && b.date == model.Date && b.batch_type == model.BatchType).OrderByDescending(d => d.date_created).FirstOrDefault();
                    }
                    else
                    {
                        if (model.BatchType == "P")
                        {
                            batchName = "X12 AL" + "_" + "P" + "_" + model.Date.ToString("MMddyyyy") + "_";
                        }
                        else
                            //batchName = "AL" + "_" + "I" + "_" + model.Date.ToString("MMddyyyy") + "_";
                            batch = ctx.claim_batch.Where(b => b.provider_id == null && b.practice_id == model.PracticeCode && b.date == model.Date && b.batch_type == model.BatchType).OrderByDescending(d => d.date_created).FirstOrDefault();
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
        public async Task<ResponseModel> UploadBatches(BatchUploadRequest model, long v)
        {
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
                                   where model.BatcheIds.Contains(cb.batch_id) && cb.batch_status.ToLower() == "Sent"
                                   select new BatchUploadViewModelBatchCSI()
                                   {
                                       BatchId = cb.batch_id,
                                       ClaimId = c.Claim_No,
                                       PatientAccount = p.Patient_Account,
                                       PracticeCode = c.practice_code,
                                       DOS = c.DOS,
                                       PatientName = p.Last_Name + ", " + p.First_Name,
                                       Claim_type = c.Claim_Type,
                                       Submission_Type = cb.Submission_Type,
                                       batch_status = cb.batch_status,
                                       batch_name = cb.batch_name,
                                       File_Path = cb.file_path,
                                       batch_type = cb.batch_type,
                                   }).GroupBy(b => b.BatchId).ToList();


                    if (batches != null && batches.Count > 0)
                    {
                        foreach (var batch in batches)
                        {
                            bool batchHasError = false;
                            List<BatchClaimSubmissionResponse> responsedPerBatch = new List<BatchClaimSubmissionResponse>();
                            foreach (var claim in batch)
                            {
                                if (!batchHasError)
                                {
                                    long insId = 0;
                                    ResponseModel res = new ResponseModel();
                                    if (claim.Claim_type == null || claim.Claim_type.ToUpper() == "P")
                                    {
                                        var result = ctx.Sp_GetCSIBatchStatusMedicare(claim.BatchId, claim.ClaimId).FirstOrDefault();
                                        insId = ctx.Claim_Insurance.Where(i => i.Claim_No == claim.ClaimId).Select(i => i.Insurance_Id).FirstOrDefault();
                                        if (result.HasValue && result.Value == true)
                                        {
                                            var existingRecord = ctx.CSI_Batch.FirstOrDefault(b => b.Batch_Id == claim.BatchId);

                                            if (existingRecord == null)
                                            {
                                                CSI_Batch record = new CSI_Batch()
                                                {
                                                    Batch_Id = claim.BatchId,
                                                    Batch_Name = claim.batch_name,
                                                    File_Path = claim.File_Path,
                                                    Claim_Number = claim.ClaimId,
                                                    Practice_Id = claim.PracticeCode,
                                                    Submission_Type = claim.Submission_Type,
                                                    Batch_Status837 = claim.batch_status,
                                                    Batch_Status999 = "Pending",
                                                    Uploaded_date837 = DateTime.Now,
                                                    Uploaded_date_CSIBatch = DateTime.Now,
                                                    CSI_Batch_Status = "Pending",
                                                    Entry_Date = DateTime.Now,
                                                    date_processed_277 = DateTime.Now,
                                                    batch_type = claim.batch_type,
                                                    Status_277 = "Pending",
                                                    insurance_id = insId

                                                };

                                                ctx.CSI_Batch.Add(record);
                                                ctx.SaveChanges();
                                            }
                                            else
                                            {
                                                Console.WriteLine("A record with Batch_Id {0} already exists. No new record inserted.", claim.BatchId);
                                            }

                                            if (claim.Claim_type == null || claim.Claim_type.ToUpper() == "P")
                                            {
                                                res = GenerateBatch_276277(Convert.ToInt64(claim.PracticeCode), claim.ClaimId);
                                            }
                                            else
                                            {
                                                res.Status = "institutional Batch File Does not Generate";
                                            }

                                        }
                                        else
                                        {
                                            res.Status = "Claim_No Does not find";
                                            insId = 0;
                                        }
                                    }
                                    else
                                    {
                                        res.Status = "institutional Batch File Does not Generate";
                                    }

                                    if (res.Status == "Error")
                                    {
                                        isAllClaimsSuccess = false;
                                        batchHasError = true;
                                        AddUpdateClaimBatchError(claim.BatchId, claim.ClaimId, v, string.Join(";", res.Response), claim.PatientName, claim.PatientAccount, claim.DOS);
                                    }
                                    responsedPerBatch.Add(new BatchClaimSubmissionResponse() { ClaimId = claim.ClaimId, PracticeCode = claim.PracticeCode, response = res.Response, BatchId = claim.BatchId,insurance_id = insId });
                                }
                            }
                            if (!batchHasError)
                            {

                                var batchToUpdate = ctx.CSI_Batch.Where(b => b.Batch_Id == batch.Key).FirstOrDefault();
                                foreach (var claim in batch)
                                    try
                                    {
                                        if (claim.Claim_type == null || claim.Claim_type.ToUpper() == "P")
                                        {
                                            var responses = responsedPerBatch.Select(r => r.response).ToList();

                                            if (!batchToUpdate.Batch_Name.StartsWith("X12"))
                                            {
                                                batchToUpdate.Batch_Name = "X12" + batchToUpdate.Batch_Name;
                                            }

                                            if (!batchToUpdate.Batch_Name.StartsWith("X12Batch"))
                                            {
                                                batchToUpdate.Batch_Name = "X12Batch" + batchToUpdate.Batch_Name;
                                            }

                                            if (batchToUpdate.Batch_Name.StartsWith("X12Batch"))
                                            {
                                                string currentDateTime = DateTime.Now.ToString("MMddyyyy_HHmmss");
                                                batchToUpdate.Batch_Name = "X12Batch276_" + currentDateTime;
                                            }


                                            string stringToWrite = string.Join("\n", responses.Select(r => r));
                                            if (!Directory.Exists(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["CSIClaimBatchPath"])))
                                                Directory.CreateDirectory(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["CSIClaimBatchPath"]));
                                            File.WriteAllText(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["CSIClaimBatchPath"] + "/" + batchToUpdate.Batch_Name + ".txt"),
                                           stringToWrite);

                                            batchToUpdate.File_Path = batchToUpdate.Batch_Name + ".txt.bci";
                                            var batch_name = batchToUpdate.Batch_Name;
                                            var filepath = batchToUpdate.File_Path;
                                            var Username = v.ToString() ?? string.Empty;

                                            InsertMultipleClaimsIntoCSIRequestsData(responsedPerBatch, batch_name, filepath, Username /*, BatchStatus*/);

                                            string fileUploadStatus = "success";
                                            fileUploadStatus = UploadFileToFTP(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["CSIClaimBatchPath"] + "/" + batchToUpdate.Batch_Name + ".txt"), (long)batchToUpdate.Practice_Id, v, "Upload Batch");
                                            ctx.SaveChanges();
                                            if (fileUploadStatus == "error")
                                            {
                                                responseModel.Status = "error";
                                                responseModel.Response = "File generation success, but file uploaded has been failed.";
                                                return responseModel;
                                            }
                                        }


                                        else
                                        {
                                            responseModel.Status = "institutional Batch File Does not Generate";
                                            isAllClaimsSuccess = false;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        throw;
                                    }
                            }
                            //if (!batchHasError)
                            //{

                            //    var batchToUpdate = ctx.CSI_Batch.Where(b => b.Batch_Id == batch.Key).FirstOrDefault();

                            //    try
                            //    {
                            //        var responses = responsedPerBatch.Select(r => r.response).ToList();

                            //        if (!batchToUpdate.Batch_Name.StartsWith("X12"))
                            //        {
                            //            batchToUpdate.Batch_Name = "X12" + batchToUpdate.Batch_Name;
                            //        }

                            //        if (!batchToUpdate.Batch_Name.StartsWith("X12Batch"))
                            //        {
                            //            batchToUpdate.Batch_Name = "X12Batch" + batchToUpdate.Batch_Name;
                            //        }

                            //        if (batchToUpdate.Batch_Name.StartsWith("X12Batch"))
                            //        {
                            //            string currentDateTime = DateTime.Now.ToString("MMddyyyy_HHmmss");
                            //            batchToUpdate.Batch_Name = "X12Batch276_" + currentDateTime;
                            //        }
                            //        string stringToWrite = string.Join("\n", responses.Select(r => r));
                            //        if (!Directory.Exists(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["CSIClaimBatchPath"])))
                            //            Directory.CreateDirectory(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["CSIClaimBatchPath"]));
                            //        File.WriteAllText(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["CSIClaimBatchPath"] + "/" + batchToUpdate.Batch_Name + ".txt"),
                            //       stringToWrite);

                            //        batchToUpdate.File_Path = batchToUpdate.Batch_Name + ".txt";
                            //        var batch_name = batchToUpdate.Batch_Name;
                            //        var filepath = batchToUpdate.File_Path;
                            //        var Username = v.ToString() ?? string.Empty;
                            //        InsertMultipleClaimsIntoCSIRequestsData(responsedPerBatch, batch_name, filepath, Username /*, BatchStatus*/);
                            //        string fileUploadStatus = "success";
                            //        fileUploadStatus = UploadFileToFTP(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["CSIClaimBatchPath"] + "/" + batchToUpdate.Batch_Name + ".txt"), (long)batchToUpdate.Practice_Id, v, "Upload Batch");
                            //        ctx.SaveChanges();
                            //        if (fileUploadStatus == "error")
                            //        {
                            //            responseModel.Status = "error";
                            //            responseModel.Response = "File generation success, but file uploaded has been failed.";
                            //            return responseModel;
                            //        }
                            //    }
                            //    catch (Exception)
                            //    {
                            //        throw;
                            //    }
                            //}

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
            }
            return responseModel;
        }
        public async Task<ResponseModel> SingleCSIClaimBatchUpload(CSIClaimBatchUploadRequest model, long v)
        {
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
                                   where model.BatcheIds.Contains(cb.batch_id) && cb.batch_status.ToLower() == "Sent"
                                   select new BatchUploadViewModelBatchCSI()
                                   {
                                       BatchId = cb.batch_id,
                                       ClaimId = c.Claim_No,
                                       PatientAccount = p.Patient_Account,
                                       PracticeCode = c.practice_code,
                                       DOS = c.DOS,
                                       PatientName = p.Last_Name + ", " + p.First_Name,
                                       Claim_type = c.Claim_Type,
                                       Submission_Type = cb.Submission_Type,
                                       batch_status = cb.batch_status,
                                       batch_name = cb.batch_name,
                                       File_Path = cb.file_path,
                                       batch_type = cb.batch_type,
                                   }).GroupBy(b => b.BatchId).ToList();


                    if (batches != null && batches.Count > 0)
                    {
                        foreach (var batch in batches)
                        {
                            bool batchHasError = false;
                            List<BatchClaimSubmissionResponse> responsedPerBatch = new List<BatchClaimSubmissionResponse>();
                            foreach (var claim in batch)
                            {
                                var abc = model.InsuranceId.ToString();
                                var bdc = int.Parse(abc);
                                Insurance_Id = bdc;

                                if (!batchHasError)
                                {
                                    ResponseModel res = new ResponseModel();
                                    if (claim.Claim_type == null || claim.Claim_type.ToUpper() == "P")
                                    {
                                        var result = ctx.Sp_GetCSIBatchStatusMedicare(claim.BatchId, claim.ClaimId).FirstOrDefault();

                                        if (result.HasValue && result.Value == true && claim.ClaimId == model.ClaimNo)
                                        {
                                            var existingRecord = ctx.CSI_Batch.FirstOrDefault(b => b.Batch_Id == claim.BatchId);
                                            if (existingRecord == null)
                                            {
                                                CSI_Batch record = new CSI_Batch()
                                                {
                                                    Batch_Id = claim.BatchId,
                                                    Batch_Name = claim.batch_name,
                                                    File_Path = claim.File_Path,
                                                    Claim_Number = claim.ClaimId,
                                                    Practice_Id = claim.PracticeCode,
                                                    Submission_Type = claim.Submission_Type,
                                                    Batch_Status837 = claim.batch_status,
                                                    Batch_Status999 = "Pending",
                                                    Uploaded_date837 = DateTime.Now,
                                                    Uploaded_date_CSIBatch = DateTime.Now,
                                                    CSI_Batch_Status = "Pending",
                                                    Entry_Date = DateTime.Now,
                                                    date_processed_277 = DateTime.Now,
                                                    batch_type = claim.batch_type,
                                                    Status_277 = "Pending",
                                                    insurance_id = model.InsuranceId,

                                                };

                                                ctx.CSI_Batch.Add(record);
                                                //ctx.SaveChanges();
                                            }
                                            else
                                            {
                                                Console.WriteLine("A record with Batch_Id {0} already exists. No new record inserted.", claim.BatchId);
                                            }


                                            if (claim.Claim_type == null || claim.Claim_type.ToUpper() == "P")
                                            {
                                                res = GenerateBatch_276277(Convert.ToInt64(claim.PracticeCode), claim.ClaimId);
                                            }
                                            else
                                            {
                                                res.Status = "institutional Batch Claim File Does not Generate";
                                            }
                                        }
                                        else
                                        {
                                            res.Status = "Claim_No Does not find";
                                        }
                                        if (res.Status == "Error")
                                        {
                                            isAllClaimsSuccess = false;
                                            batchHasError = true;
                                            AddUpdateClaimBatchError(claim.BatchId, claim.ClaimId, v, string.Join(";", res.Response), claim.PatientName, claim.PatientAccount, claim.DOS);
                                        }
                                        responsedPerBatch.Add(new BatchClaimSubmissionResponse() { ClaimId = claim.ClaimId, PracticeCode = claim.PracticeCode, response = res.Response, BatchId = claim.BatchId , insurance_id = model.InsuranceId});
                                    }
                                    else
                                    {
                                        res.Status = "institutional Batch Claim File Does not Generate";
                                    }
                                }


                            }
                            if (!batchHasError)
                            {

                                var batchToUpdate = ctx.CSI_Batch.Where(b => b.Batch_Id == batch.Key).FirstOrDefault();
                                foreach (var claim in batch)
                                    try
                                    {
                                        if (claim.Claim_type == null || claim.Claim_type.ToUpper() == "P")
                                        {
                                            string fileUploadStatus = "error";
                                            var responses = responsedPerBatch.Select(r => r.response).ToList();
                                            if (batchToUpdate != null)
                                            {
                                                if (!batchToUpdate.Batch_Name.StartsWith("X12"))
                                                {
                                                    batchToUpdate.Batch_Name = "X12" + batchToUpdate.Batch_Name;
                                                }

                                                if (!batchToUpdate.Batch_Name.StartsWith("X12Batch"))
                                                {
                                                    batchToUpdate.Batch_Name = "X12Batch" + batchToUpdate.Batch_Name;
                                                }

                                                if (batchToUpdate.Batch_Name.StartsWith("X12Batch"))
                                                {
                                                    string currentDateTime = DateTime.Now.ToString("MMddyyyy_HHmmss");
                                                    batchToUpdate.Batch_Name = "X12Batch276_" + currentDateTime;
                                                }


                                                string stringToWrite = string.Join("\n", responses.Select(r => r));
                                                if (!Directory.Exists(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["CSIClaimBatchPath"])))
                                                    Directory.CreateDirectory(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["CSIClaimBatchPath"]));
                                                File.WriteAllText(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["CSIClaimBatchPath"] + "/" + batchToUpdate.Batch_Name + ".txt"),
                                               stringToWrite);
                                                batchToUpdate.File_Path = batchToUpdate.Batch_Name + ".txt.bci";
                                                var batch_name = batchToUpdate.Batch_Name;
                                                var filepath = batchToUpdate.File_Path;
                                                var Username = v.ToString() ?? string.Empty;

                                                //InsertMultipleClaimsIntoCSIRequestsData(responsedPerBatch, batch_name, filepath, Username /*, BatchStatus*/);

                                                //string fileUploadStatus = "success";
                                                fileUploadStatus = "success";
                                                fileUploadStatus = UploadFileToFTP(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["CSIClaimBatchPath"] + "/" + batchToUpdate.Batch_Name + ".txt"), (long)batchToUpdate.Practice_Id, v, "Upload Batch");
                                                if (fileUploadStatus == "success")
                                                {
                                                    InsertMultipleClaimsIntoCSIRequestsData(responsedPerBatch, batch_name, filepath, Username /*, BatchStatus*/);
                                                    ctx.SaveChanges();
                                                }
                                            }
                                            //ctx.SaveChanges();
                                            if (fileUploadStatus == "error")
                                            {
                                                responseModel.Status = "error";
                                                responseModel.Response = "File generation success, but file uploaded has been failed.";
                                                return responseModel;
                                            }
                                        }


                                        else
                                        {
                                            responseModel.Status = "institutional Batch File Does not Generate";
                                            isAllClaimsSuccess = false;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        throw;
                                    }
                            }
                            //if (!batchHasError)
                            //{
                            //    var batchToUpdate = ctx.CSI_Batch.Where(b => b.Batch_Id == batch.Key).FirstOrDefault();
                            //    batchToUpdate.Uploaded_date_CSIBatch = DateTime.Now;
                            //    batchToUpdate.CSI_Batch_Status = "Sent";
                            //    var Batch837Upload = batchToUpdate.Uploaded_date_CSIBatch;
                            //    try
                            //    {
                            //        var responses = responsedPerBatch.Select(r => r.response).ToList();

                            //        if (!batchToUpdate.Batch_Name.StartsWith("X12"))
                            //        {
                            //            batchToUpdate.Batch_Name = "X12" + batchToUpdate.Batch_Name;
                            //        }

                            //        if (!batchToUpdate.Batch_Name.StartsWith("X12Batch"))
                            //        {
                            //            batchToUpdate.Batch_Name = "X12Batch" + batchToUpdate.Batch_Name;
                            //        }

                            //        if (batchToUpdate.Batch_Name.StartsWith("X12Batch"))
                            //        {
                            //            string currentDateTime = DateTime.Now.ToString("MMddyyyy_HHmmss");
                            //            batchToUpdate.Batch_Name = "X12Batch276_" + currentDateTime;
                            //        }
                            //        string stringToWrite = string.Join("\n", responses.Select(r => r));
                            //        if (!Directory.Exists(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["CSIClaimBatchPath"])))
                            //            Directory.CreateDirectory(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["CSIClaimBatchPath"]));
                            //        File.WriteAllText(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["CSIClaimBatchPath"] + "/" + batchToUpdate.Batch_Name + ".txt"),
                            //       stringToWrite);
                            //        batchToUpdate.File_Path = batchToUpdate.Batch_Name;

                            //        var batch_name = batchToUpdate.Batch_Name;
                            //        var filepath = batchToUpdate.File_Path;
                            //        var Username = v.ToString() ?? string.Empty;
                            //        InsertMultipleClaimsIntoCSIRequestsData(responsedPerBatch, batch_name, filepath, Username /*, BatchStatus*/);
                            //        string fileUploadStatus = "success";
                            //        fileUploadStatus = UploadFileToFTP(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["CSIClaimBatchPath"] + "/" + batchToUpdate.Batch_Name + ".txt"), (long)batchToUpdate.Practice_Id, v, "Upload Batch");
                            //        ctx.SaveChanges();
                            //        if (fileUploadStatus == "error")
                            //        {
                            //            responseModel.Status = "error";
                            //            responseModel.Response = "File generation success, but file uploaded has been failed.";
                            //            return responseModel;
                            //        }
                            //    }
                            //    catch (Exception)
                            //    {
                            //        throw;
                            //    }
                            //}

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
            }
            return responseModel;
        }

        public string UploadFileToFTP(string source, long practiceCode, long userId, string caller = "")
        {
            try
            {
                NPMLogger.GetInstance().Info($"Upload Batch File to FTP Called by user '{userId}' from {caller}");
                var practInfo = _practiceService.GetBatchCSIPracticeFTPInfo(practiceCode, FTPType.EDI);

                if (practInfo != null)
                {
                    using (SftpClient client = new SftpClient(practInfo.Host, practInfo.Port, practInfo.Username, practInfo.Password))
                    {
                        NPMLogger.GetInstance().Info($"Attempting to connect to FTP server at {practInfo.Host}:{practInfo.Port}");

                        client.Connect();
                        NPMLogger.GetInstance().Info($"Practice '{practiceCode}' connection successful");
                        client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(60);
                        if (client.IsConnected)
                        {
                            client.ChangeDirectory(practInfo.Destination);
                            using (FileStream fs = new FileStream(source, FileMode.Open))
                            {
                                client.BufferSize = 4 * 1024; // 4KB buffer size
                                client.UploadFile(fs, Path.GetFileName(source));
                                NPMLogger.GetInstance().Info($"{source} File uploaded to Practice {practiceCode} FTP by user {userId} from {caller}");
                            }
                        }
                        else
                        {
                            NPMLogger.GetInstance().Error($"Connection failed to FTP of '{practiceCode}'");
                            return "error";
                        }

                        return "success";
                    }
                }
                else
                {
                    NPMLogger.GetInstance().Error($"FTP Information not found in database for practice {practiceCode}");
                    return "error";
                }
            }
            catch (Exception ex)
            {
                NPMLogger.GetInstance().Error($"Error while uploading file to FTP for practice {practiceCode}: {ex.Message}");
                return "error";
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
        public ResponseModel RegenerateBatchCSIFile(RegenerateBatchCSIFileModel model, long userId)
        {

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
                                        .Select(cb => cb.batch_type).FirstOrDefault();
                                    if (b_Type == "P" || b_Type == null)
                                    {
                                        res = GenerateBatch_276277(Convert.ToInt64(model.Practice_Code), claim.Claim_No);
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
                                    File.WriteAllText(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["ClaimBatchSubmissionPath"] + "/" + batchToUpdate.batch_name + ".txt.bci"),
                                stringToWrite);
                                    batchToUpdate.file_path = batchToUpdate.batch_name + ".txt.bci";
                                    batchToUpdate.file_generated = true;
                                    //ctx.SaveChanges();

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
        public void AddUpdateClaimBatchError(long batchId, long claimNo, long userId, string errorResponse, string patientName, long? patientAccount, DateTime? Dos)
        {
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var claimBatchError = ctx.CSI_Batch_Error.FirstOrDefault(cbe => cbe.batch_id == batchId && cbe.claim_id == claimNo);
                    if (claimBatchError != null)
                    {
                        claimBatchError.dos = (DateTime)Dos;
                        claimBatchError.error = errorResponse;
                        ctx.Entry(claimBatchError).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        CSI_Batch_Error error = new CSI_Batch_Error()
                        {
                            id = Convert.ToInt64(ctx.SP_TableIdGenerator("claim_batch_error_id").FirstOrDefault()),
                            batch_id = batchId,
                            claim_id = claimNo,
                            created_user = userId,
                            date_created = DateTime.Now,
                            deleted = false,
                            dos = (DateTime)Dos,
                            error = errorResponse,
                            patient_id = (long)patientAccount,
                            patient_name = patientName
                        };
                        ctx.CSI_Batch_Error.Add(error);
                    }
                    ctx.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void InsertMultipleClaimsIntoCSIRequestsData(List<BatchClaimSubmissionResponse> responsedPerBatch, string batch_name, string filepath, string Username/*, DateTime? Batch837Upload*//*, string BatchStatus*/)
        {
            try
            {
                var batch_status = "Pending";
                // Retrieve the connection string from configuration
                string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandType = CommandType.Text;


                        cmd.CommandText = @"
                INSERT INTO CSI_Batch (
                    Batch_Id,
                    Batch_Name,
                    File_Path,
                    ENTRY_DATE,
                    PRACTICE_ID,
                    UserName,
                    CLAIM_NUMBER,
                    PAYER_NAME,
                    PAYER_ID,
                    ISA_CONTROL_NUM,
                    TRN,
                    STRING_276,
                    GENERATOR_STATUS,
                    RESPONSE_STRING_277,
                    PARSING_STATUS,
                    STATUS_277,
                    STRING_276_999,
                    PROCESSING_DATE,
                    Uploaded_date837,
                    Uploaded_date_CSIBatch,
                    Batch_Status837, 
                    CSI_Batch_Status,
                    Deleted,
                    response_file_path,
                    date_processed,
                    provider_id,
                    Batch_Status999,
                    Submission_Type,
                    modified_user,
                    date_modified,
                    batch_lock,
                    Internal_status,
                    insurance_id
                )
                VALUES (
                    @Batch_Id,
                    @Batch_Name,
                    @File_Path, 
                    @ENTRY_DATE,
                    @PRACTICE_ID,
                    @UserName,
                    @CLAIM_NUMBER,
                    @PAYER_NAME,
                    @PAYER_ID,
                    @ISA_CONTROL_NUM,
                    @TRN,
                    @STRING_276,
                    @GENERATOR_STATUS,
                    @RESPONSE_STRING_277,
                    @PARSING_STATUS,
                    @STATUS_277,
                    @STRING_276_999,
                    @PROCESSING_DATE,
                    @Uploaded_date837,
                    @Uploaded_date_CSIBatch,
                    @Batch_Status837,
                    @CSI_Batch_Status,
                    @Submission_Type,
                    @modified_user,
                    @date_modified,
                    @batch_lock,
                    @Deleted,
                    @response_file_path,
                    @date_processed,
                    @provider_id,
                    @Batch_Status999,
                    @Internal_status,
                    @insurance_id
                )";


                        foreach (var claim in responsedPerBatch)
                        {
                            // Ensure claim.response is not null
                            if (string.IsNullOrEmpty(claim.response))
                            {
                                // Skip if response is null or empty
                                Console.WriteLine("Skipping claim with empty response.");
                                continue; // Skip the rest of the loop for this claim
                            }

                            // Extract the required values from the response
                            string isaControlNum = ExtractValueFromSegment(claim.response, "ISA", 13);
                            string trn = ExtractValueFromSegment(claim.response, "TRN", 2);
                            string claimNo = ExtractValueFromSegment(claim.response, "REF", 2);
                            string payerName = ExtractValueFromSegment(claim.response, "NM1", 3);
                            string payerId = ExtractValueFromSegment(claim.response, "NM1", 9);
                            long claimNumber = 0;
                            if (!string.IsNullOrEmpty(claimNo) && long.TryParse(claimNo, out claimNumber))
                            {
                            }
                            else
                            {
                                claimNumber = 0;
                            }

                            int payerIdValue = 0;
                            if (!string.IsNullOrEmpty(payerId) && int.TryParse(payerId, out payerIdValue))
                            {
                            }
                            else
                            {
                                payerIdValue = 0;
                            }
                            using (var ctx = new NPMDBEntities())
                            {
                                var existingRecord = ctx.CSI_Batch.FirstOrDefault(b => b.Batch_Id == claim.BatchId && b.Claim_Number == claim.ClaimId);
                                if (existingRecord != null)
                                {
                                    existingRecord.Batch_Name = batch_name;
                                    existingRecord.File_Path = filepath;
                                    //existingRecord.File_Path = Truncate("", 50);
                                    existingRecord.Entry_Date = DateTime.Now;
                                    existingRecord.Practice_Id = claim.PracticeCode;
                                    existingRecord.UserName = Username;
                                    existingRecord.Claim_Number = claimNumber;
                                    existingRecord.Payer_Name = Truncate(payerName, 100);
                                    existingRecord.Payer_Id = payerIdValue;
                                    existingRecord.ISA_Control_Num = Truncate(isaControlNum, 50);
                                    existingRecord.Trn = Truncate(trn, 50);
                                    existingRecord.String_276 = Truncate(claim.response, 700);
                                    existingRecord.Generator_Status = Truncate("Success", 50);
                                    existingRecord.Response_String_277 = Truncate("", 5500);
                                    existingRecord.Parsing_Status = Truncate("", 50);
                                    existingRecord.Status_277 = "Pending";
                                    existingRecord.String_276_999 = Truncate("", 8);
                                    existingRecord.Processing_Date = DateTime.Now;
                                    existingRecord.Uploaded_date837 = DateTime.Now;
                                    existingRecord.Uploaded_date_CSIBatch = DateTime.Now;
                                    existingRecord.Batch_Status837 = "Pending";
                                    existingRecord.CSI_Batch_Status = batch_status;
                                    existingRecord.Batch_Status999 = "Pending";
                                    existingRecord.CSI_Batch_Status = batch_status;
                                    existingRecord.Deleted = false;
                                    existingRecord.response_file_path = "Pending";
                                    existingRecord.date_processed = DateTime.Now;
                                    existingRecord.provider_id = 0;
                                    existingRecord.Submission_Type = "P";
                                    existingRecord.modified_user = 0;
                                    existingRecord.date_modified = DateTime.Now;
                                    existingRecord.batch_lock = false;
                                    existingRecord.Internal_status = "";
                                    existingRecord.insurance_id = claim.insurance_id;
                                    ctx.SaveChanges();
                                }
                                else
                                {
                                    CSI_Batch record = new CSI_Batch()
                                    {
                                        Batch_Id = claim.BatchId,
                                        Batch_Name = batch_name,
                                        File_Path = filepath,
                                        Entry_Date = DateTime.Now,
                                        Practice_Id = claim.PracticeCode,
                                        UserName = Username,
                                        Claim_Number = claimNumber,
                                        Payer_Name = Truncate(payerName, 100),
                                        Payer_Id = payerIdValue,
                                        ISA_Control_Num = Truncate(isaControlNum, 50),
                                        Trn = Truncate(trn, 50),
                                        String_276 = Truncate(claim.response, 700),
                                        Generator_Status = Truncate("Success", 50),
                                        Response_String_277 = Truncate("", 5500),
                                        Parsing_Status = Truncate("", 50),
                                        Status_277 = "Pending",
                                        String_276_999 = Truncate("", 8),
                                        Processing_Date = DateTime.Now,
                                        Uploaded_date837 = DateTime.Now,
                                        Uploaded_date_CSIBatch = DateTime.Now,
                                        Batch_Status837 = "pending",
                                        CSI_Batch_Status = batch_status,
                                        Deleted = false,
                                        response_file_path = "Pending",
                                        date_processed = DateTime.Now,
                                        provider_id = 0,
                                        Submission_Type = "P",
                                        modified_user = 0,
                                        date_modified = DateTime.Now,
                                        batch_lock = false,
                                        Batch_Status999 = "Pending",
                                        batch_type = "",
                                        date_processed_277 = DateTime.Now,
                                        Internal_status="",
                                        insurance_id = claim.insurance_id
                                    };

                                    ctx.CSI_Batch.Add(record);
                                    ctx.SaveChanges();
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting data into CSI_Batch: " + ex.Message);
            }
        }

    }
}

