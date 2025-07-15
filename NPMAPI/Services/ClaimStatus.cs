using NPMAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Web;
using NPMAPI.Repositories;
using System.Configuration;
using PracticeSaopApi.ServiceReferenceCore;
using System.Globalization;
using System.Threading.Tasks;

namespace NPMAPI.Services
{
    public class ClaimStatus : IClaimService
    {
        private readonly IClaimStatusService _claimStatusService;
        private readonly IPracticeRepository _practiceService;
        public string _username = "V313";
        public string _password = "Sn^TPknajEU76Pt";
        private readonly Vendor _vendor;
        private const string _payLoadType = "X12_276_Request_005010X212";

        public ClaimStatus(string username, string password, Vendor vendor)
        {
            _username = username;
            _password = password;
            _vendor = vendor;
        }

        public enum Vendor
        {
            Trizetto
        }

        public ClaimStatus(IClaimStatusService claimStatusService, IPracticeRepository practiceService)
        {
            _claimStatusService = claimStatusService ?? throw new ArgumentNullException(nameof(claimStatusService));
            _practiceService = practiceService ?? throw new ArgumentNullException(nameof(practiceService));
        }

        public async Task<Output277> SendRequest(long practice_id, long claim_id, long Insurance_Id, string unique_name)
        {
            var output = new Output277();

            try
            {
                var response1 = _claimStatusService.GenerateSerialNumber(10);
                var response2 = _claimStatusService.GenerateSerialNumberISA(9);
                var response3 = _claimStatusService.GenerateSerialNumberBHTGS(9);
                var ResponseModelSerialNumber = _claimStatusService.SequanceNumber();

                var request276 = _claimStatusService.GenerateBatch_276(practice_id, claim_id, Insurance_Id, unique_name);
                if (request276 == null)
                {
                    throw new Exception("GenerateBatch_276 returned null.");
                }

                if (request276.Response == null)
                {
                    throw new Exception("Failed to generate request 276.");
                }

                if (request276.Status == "Error")
                {
                    output.ErrorMessage = "Error in GenerateBatch_276: " +
                        (request276.Response != null && request276.Response.Count > 0
                        ? string.Join(", ", request276.Response)
                        : "No additional error information.");
                    return output;
                }

                return await Trizetto(request276.Response);
            }
            catch (Exception ex)
            {
                output.ErrorMessage = ex.Message;
                return output;
            }
        }

        public async Task<Output277> Trizetto(string request276)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var binding = new CustomBinding("CoreSoapBindingCore");
            var endpointAddress = new EndpointAddress("https://api.gatewayedi.com/v2/CORE_CAQH/soap");
            var client = new CORETransactionsClient(binding, endpointAddress);

            client.ClientCredentials.UserName.UserName = "V313";
            client.ClientCredentials.UserName.Password = "Sn^TPknajEU76Pt";

            var request = new COREEnvelopeRealTimeRequest
            {
                PayloadType = "X12_276_Request_005010X212",
                ProcessingMode = "RealTime",
                PayloadID = Guid.NewGuid().ToString(),
                TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                SenderID = "V313",
                ReceiverID = "263923727",
                CORERuleVersion = "2.2.0",
                Payload = request276
            };

            COREEnvelopeRealTimeResponse response;

            try
            {
                client.Open();
                response = client.RealTimeTransaction(request);
                var payload = response.Payload;
                var stcCodeCategory = ExtractSTCCodeCategory(payload);
                var stcDescription = ExtractSTCCode(payload);
                var stcStatusCode = ExtractSTCCodeForStatus(payload);
                var CptstcStatusCode = ExtractCptStcDetails(payload);
                List<string> dtpDates = new List<string>();

                var segments = payload.Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries);
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
                var stcCodes = ExtractCptStcDetails(payload);
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
                var stcCode = ExtractCptStcCategoryDetails(payload);
                foreach (var code in stcCode)
                {
                    var description = GetCptCategorycodeFromDatabase(code);
                    if (!string.IsNullOrEmpty(description))
                    {
                        StcCategoryDescription[code] = description;
                    }
                }
                string stcCateogoryDescriptionOutput = string.Join(", ", StcCategoryDescription.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                var responseTrns = ExtractTRNs(response);
                string selectedTrn = responseTrns.FirstOrDefault();

                if (payload.Contains("ST*277"))
                {
                    if (selectedTrn != null)
                    {
                        bool isInserted277 = InsertResponseData(selectedTrn, payload, "277");

                        if (isInserted277)
                        {
                            UpdateStatus(selectedTrn, "successfully processed ST*277", 1);
                        }
                    }
                }
                string enteredByValue = GetEnteredByData(selectedTrn);
                if (payload.Contains("ST*999"))
                {
                    string trnFromCTX = ExtractTrn02FromPayload(payload);
                    if (!string.IsNullOrEmpty(trnFromCTX))
                    {
                        bool isInserted999 = InsertResponseData(trnFromCTX, payload, "999");

                        if (isInserted999)
                        {
                            UpdateStatus(trnFromCTX, "successfully processed ST*999", 1);
                        }
                    }
                    else
                    {
                        UpdateStatusForError(request276, "TRN02 not found for ST*999", 1);
                    }
                }
                else
                {

                }
                var output = new Output277
                {
                    Transaction276 = request276,
                    Transaction277 = payload,
                    ErrorMessage = response.ErrorCode + (!string.IsNullOrEmpty(response.ErrorMessage) ? ": " + response.ErrorMessage : string.Empty),
                    ClaimStatusData = new List<_277Header>(),
                    STCStatus = stcStatus,
                    STCDescription = stcStatusDescription,
                    stcStatusCategoryDescription = stcStatusCategoryDescription,
                    DTPDates = concatenatedDates,
                    EnteredBy = enteredByValue,
                    StcStatusDescription = stcStatusDescriptionOutput,
                    StcCategoryDescription = stcCateogoryDescriptionOutput
                };

                return await Task.FromResult(output);
            }
            catch (FaultException faultEx)
            {
                throw new Exception("SOAP Fault: " + faultEx.Message, faultEx);
            }
            catch (CommunicationException commEx)
            {
                throw new Exception("Communication error: " + commEx.Message, commEx);
            }
            finally
            {
                if (client.State == CommunicationState.Faulted)
                {
                    client.Abort();
                }
                else
                {
                    client.Close();
                }
            }
        }
        private List<string> ExtractCptStcCategoryDetails(string payload)
        {
            var stcCode = new List<string>();
            var segments = payload.Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries);

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
        


        private List<string> ExtractCptStcDetails(string payload)
        {
            var stcCodes = new List<string>();
            var segments = payload.Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries);

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
        private string GetStatusCategoryDescriptionFromDatabase(string code)
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
        private string ExtractSTCCode(string payload)
        {
            var lines = payload.Split('~');

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
        private string ExtractSTCCodeForStatus(string payload)
        {
            var lines = payload.Split('~');

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
        private string ExtractSTCCodeCategory(string payload)
        {
            var lines = payload.Split('~');

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
                var query = "SELECT Status FROM Packet277CA_ClaimStatusCategoryCodes WHERE Code = @Code";
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
        private bool InsertResponseData(string selectedTrn, string payload, string segmentType)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            bool isInsertedOrUpdated = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string checkRecordQuery = "SELECT COUNT(1) FROM CSI_REQUESTS_DATA WHERE TRN = @TRN";

                using (SqlCommand cmd = new SqlCommand(checkRecordQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@TRN", selectedTrn);
                    int count = (int)cmd.ExecuteScalar();

                    string responseString277 = segmentType == "277" ? payload : null;
                    string responseString999 = segmentType == "999" ? payload : null;

                    if (count > 0)
                    {
                        string updateQuery = @"UPDATE CSI_REQUESTS_DATA 
                                       SET RESPONSE_STRING_277 = COALESCE(@RESPONSE_STRING_277, RESPONSE_STRING_277),
                                           STRING_276_999 = COALESCE(@STRING_276_999, STRING_276_999)
                                       WHERE TRN = @TRN";

                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@RESPONSE_STRING_277", (object)responseString277 ?? DBNull.Value);
                            updateCmd.Parameters.AddWithValue("@STRING_276_999", (object)responseString999 ?? DBNull.Value);
                            updateCmd.Parameters.AddWithValue("@TRN", selectedTrn);
                            updateCmd.ExecuteNonQuery();
                            isInsertedOrUpdated = true;
                        }
                    }
                    else
                    {
                        string insertQuery = @"INSERT INTO CSI_REQUESTS_DATA (TRN, RESPONSE_STRING_277, STRING_276_999) 
                                       VALUES (@TRN, @RESPONSE_STRING_277, @STRING_276_999)";

                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, connection))
                        {
                            insertCmd.Parameters.AddWithValue("@TRN", selectedTrn);
                            insertCmd.Parameters.AddWithValue("@RESPONSE_STRING_277", (object)responseString277 ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@STRING_276_999", (object)responseString999 ?? DBNull.Value);
                            insertCmd.ExecuteNonQuery();
                            isInsertedOrUpdated = true;
                        }
                    }
                }
            }
            return isInsertedOrUpdated;
        }
        private string ExtractTrn02FromPayload(string payload)
        {
            string pattern = @"CTX\*TRN02\+(\d+)\~";
            System.Text.RegularExpressions.Match match = Regex.Match(payload, pattern);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }
        private IEnumerable<Output277> ProcessResponse(COREEnvelopeRealTimeResponse response)
        {
            List<Output277> output = new List<Output277>();
            return output;
        }
        private List<string> ExtractTRNs(COREEnvelopeRealTimeResponse response)
        {
            string responseString = response.Payload;
            List<string> trns = new List<string>();
            var pattern = @"TRN\*2\*(\d+)";
            var matches = Regex.Matches(responseString, pattern);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    trns.Add(match.Groups[1].Value);
                }
            }

            return trns;
        }
        private bool InsertResponseData(IEnumerable<Output277> outputData, string selectedTrn, string payload)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            bool isInserted = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string checkRecordQuery = "SELECT COUNT(1) FROM CSI_REQUESTS_DATA WHERE TRN = @TRN";

                using (SqlCommand cmd = new SqlCommand(checkRecordQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@TRN", selectedTrn);
                    int count = (int)cmd.ExecuteScalar();

                    if (count > 0)
                    {

                        string updateQuery = @"UPDATE CSI_REQUESTS_DATA 
                                       SET RESPONSE_STRING_277 = @RESPONSE_STRING_277
                                       WHERE TRN = @TRN";

                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@RESPONSE_STRING_277", payload);
                            updateCmd.Parameters.AddWithValue("@TRN", selectedTrn);

                            updateCmd.ExecuteNonQuery();
                            isInserted = true;
                        }
                    }
                    else
                    {

                    }
                }
            }
            return isInserted;
        }
        private void UpdateStatus(string selectedTrn, string parsingStatus, int status277)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string updateStatusQuery = @"UPDATE CSI_REQUESTS_DATA 
                                      SET PARSING_STATUS = @PARSING_STATUS, 
                                          STATUS_277 = @STATUS_277 
                                      WHERE TRN = @TRN";

                using (SqlCommand cmd = new SqlCommand(updateStatusQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@PARSING_STATUS", parsingStatus);
                    cmd.Parameters.AddWithValue("@STATUS_277", status277);
                    cmd.Parameters.AddWithValue("@TRN", selectedTrn);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void UpdateStatusForError(string request276, string parsingStatus, int status277)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string updateStatusQuery = @"UPDATE CSI_REQUESTS_DATA 
                                      SET PARSING_STATUS = @PARSING_STATUS, 
                                          STATUS_277 = @STATUS_277 
                                      WHERE TRN = @TRN";

                using (SqlCommand cmd = new SqlCommand(updateStatusQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@PARSING_STATUS", parsingStatus);
                    cmd.Parameters.AddWithValue("@STATUS_277", status277);
                    cmd.Parameters.AddWithValue("@TRN", ExtractTRNsFromRequest(request276));
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private string ExtractTRNsFromRequest(string request)
        {
            return "YourExtractedTRN";
        }
        private string GetEnteredByData(string selectedTrn)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            string enteredByValue = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (selectedTrn != null)
                {
                    connection.Open();
                    string checkRecordQuery = "SELECT ENTERED_BY FROM CSI_REQUESTS_DATA WHERE TRN = @TRN";

                    using (SqlCommand cmd = new SqlCommand(checkRecordQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@TRN", selectedTrn);
                        var result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            enteredByValue = result.ToString();
                        }
                    }
                }
                else
                {
                    enteredByValue = "";
                }


            }

            return enteredByValue;
        }
        public async Task<ResponseModel> GetCSIReport(long practiceCode, long claimNo)
        {
            var res = new ResponseModel();

            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    var violatedClaims = await ctx.Database.SqlQuery<CSIReportModel>(
                        "Sp_GetCSIStatus @practiceCode, @ClaimNo",
                        new SqlParameter("@practiceCode", practiceCode),
                        new SqlParameter("@ClaimNo", claimNo)

                    ).ToListAsync();

                    if (violatedClaims != null && violatedClaims.Count > 0)
                    {
                        res.Status = "Success";
                        res.Response = violatedClaims;
                    }
                    else
                    {
                        res.Status = "Error";
                        res.Response = "No claims found.";
                    }
                }
                catch (Exception ex)
                {
                    res.Status = "Error";
                    res.Response = $"An error occurred: {ex.Message}";
                }
            }

            return res;
        }

    }
}


























