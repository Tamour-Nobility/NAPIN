using NPMAPI.Models;
using NPMAPI.Repositories;
using PracticeSaopApi.ServiceReferenceCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Web;

namespace NPMAPI.Services
{
    public class ClaimBatch276: IClaimBatch276
    {
        private readonly IGenerateBatch276Service _claimStatusService;
        private readonly IPracticeRepository _practiceService;
        public string _username = "****";
        public string _password = "*****";
        private readonly Vendor _vendor;
        private const string _payLoadType = "X12_276_Request_005010X212";
        public ClaimBatch276(IGenerateBatch276Service claimStatusService, IPracticeRepository practiceService)
        {
            _claimStatusService = claimStatusService ?? throw new ArgumentNullException(nameof(claimStatusService));
            _practiceService = practiceService ?? throw new ArgumentNullException(nameof(practiceService));
        }

        public async Task<Output277> SendRequest(long practice_id, long claim_id)
        {

            var output = new Output277();

            try
            {
                //var response1 = _claimStatusService.GenerateSerialNumber(10);
                //var response2 = _claimStatusService.GenerateSerialNumberISA(9);
                //var response3 = _claimStatusService.GenerateSerialNumberBHTGS(9);
                //var ResponseModelSerialNumber = _claimStatusService.SequanceNumber();

                var request276 = _claimStatusService.GenerateBatch_276277(practice_id, claim_id);
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

            client.ClientCredentials.UserName.UserName = "****";
            client.ClientCredentials.UserName.Password = "******";

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
                var output = new Output277
                {
                    Transaction276 = request276,
                    Transaction277 = payload,
                    ErrorMessage = response.ErrorCode + (!string.IsNullOrEmpty(response.ErrorMessage) ? ": " + response.ErrorMessage : string.Empty),
                    ClaimStatusData = new List<_277Header>(),
                    //STCStatus = stcStatus,
                    //STCDescription = stcStatusDescription,
                    //stcStatusCategoryDescription = stcStatusCategoryDescription,
                    //DTPDates = concatenatedDates,
                    //EnteredBy = enteredByValue,

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



        //Task<ResponseModel> IClaimBatch276.UploadBatches(BatchUploadRequest model, long v)
        //{
        //    var request276 = _claimStatusService.UploadBatches(model,v);
        //    return request276.Response;
        //}
        public async Task<ResponseModel> UploadBatches(BatchUploadRequest model, long v)
        {
            // Assuming _claimStatusService.UploadBatches returns a Task<ResponseModel>
            var request276 = await _claimStatusService.UploadBatches(model, v); // Await the result
            return request276; // Return the ResponseModel object
        }

        public async Task<ResponseModel> SingleCSIClaimBatchUpload(CSIClaimBatchUploadRequest model, long v)
        {
            var request276 = await _claimStatusService.SingleCSIClaimBatchUpload(model, v); // Await the result
            return request276; // Return the ResponseModel object
        }
        Task<RegenerateBatchCSIFileModel> IClaimBatch276.RegenerateBatchCSI(RegenerateBatchCSIFileModel model, long userId)
        {
            var request276 = _claimStatusService.RegenerateBatchCSIFile(model, userId);
            return request276.Response;
        }
    }
}