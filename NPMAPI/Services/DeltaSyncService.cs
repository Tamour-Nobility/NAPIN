using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using NPMAPI.Models;
using NPMAPI.Models.InboxHealth;
using NPMAPI.Repositories;

namespace NPMAPI.Services
{
    public class DeltaSyncService : IDeltaSyncRepository
    {
        public SyncedPractice GetSyncedPractice(long pracCode)
        {
            SyncedPractice syncedPractice;
            using (var ctx = new NPMDBEntities())
            {
                syncedPractice = ctx.SyncedPractices.Where(x => x.Practice_Code == pracCode).FirstOrDefault<SyncedPractice>();
            }
            return syncedPractice;
        }
        public SyncedClaim GetSyncedclaim(long claimno)
        {
            SyncedClaim Syncedclaim;
            using (var ctx = new NPMDBEntities())
            {
                Syncedclaim = ctx.SyncedClaims.Where(x => x.Claim_no == claimno).FirstOrDefault<SyncedClaim>();
            }
            return Syncedclaim;
        }

        public ClaimUpdateResponse UpdateClaim(ClaimUpdateRequest request)
        {
            var response = new ClaimUpdateResponse();
            var task = InboxHealthAPI.Put(request, "/partner/v2/invoices/" + request.id);
            var jsonResult = task.Result.Content.ReadAsStringAsync();
            jsonResult.Wait();
            if (task.Result.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<ClaimUpdateResponse>(jsonResult.Result);
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = response.ErrorMessage;
            }
            return response;
        }
        public ClaimChargesCreateResponse CreateClaimCharge(ClaimChargesCreateRequest request)
        {
            var response = new ClaimChargesCreateResponse();
            var task = InboxHealthAPI.Create(request, "/partner/v2/line_items");
            var jsonResult = task.Result.Content.ReadAsStringAsync();
            jsonResult.Wait();
            if (task.Result.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<ClaimChargesCreateResponse>(jsonResult.Result);
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = response.ErrorMessage;
            }
            return response;
        }

        public ClaimChargesUpdateResponse UpdateClaimCharge(ClaimChargesUpdateRequest request)
        {
            var response = new ClaimChargesUpdateResponse();
            var task = InboxHealthAPI.Put(request, "/partner/v2/line_items/" + request.id);
            var jsonResult = task.Result.Content.ReadAsStringAsync();
            jsonResult.Wait();
            if (task.Result.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<ClaimChargesUpdateResponse>(jsonResult.Result);
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = response.ErrorMessage;
            }
            return response;
        }
          public ClaimChargesUpdateResponse deleteClaimCharge(long request)
        {
            var response = new ClaimChargesUpdateResponse();

            var task = InboxHealthAPI.Delete("/partner/v2/line_items/" + request);
            var jsonResult = task.Result.Content.ReadAsStringAsync();
            jsonResult.Wait();
            if (task.Result.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<ClaimChargesUpdateResponse>(jsonResult.Result);
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = response.ErrorMessage;
            }
            return response;
        }

        public ClaimPaymentUpdateResponse UpdateClaimPayment(ClaimPaymentUpdateRequest request)
        {
            var response = new ClaimPaymentUpdateResponse();
            var task = InboxHealthAPI.Put(request, "/partner/v2/payments/" + request.id);
            var jsonResult = task.Result.Content.ReadAsStringAsync();
            jsonResult.Wait();
            if (task.Result.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<ClaimPaymentUpdateResponse>(jsonResult.Result);
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = response.ErrorMessage;
            }
            return response;
        }

        public PatientUpdateResponse UpdatePatient(PatientUpdateRequest request)
        {
            var response = new PatientUpdateResponse();
            var task = InboxHealthAPI.Put(request, "/partner/v2/patients/" + request.id);
            var jsonResult = task.Result.Content.ReadAsStringAsync();
            jsonResult.Wait();
            if (task.Result.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<PatientUpdateResponse>(jsonResult.Result);
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = response.ErrorMessage;
            }
            return response;
        }

        public PracticeUpdateResponse UpdatePractice(PracticeUpdateRequest request)
        {
            var response = new PracticeUpdateResponse();
            var task = InboxHealthAPI.Put(request, "/partner/v2/enterprises/" + request.id);
            var jsonResult = task.Result.Content.ReadAsStringAsync();
            jsonResult.Wait();
            if (task.Result.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<PracticeUpdateResponse>(jsonResult.Result);
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = response.ErrorMessage;
            }
            return response;
        }

        public PracticeLocationUpdateResponse UpdatePracticeLocation(PracticeLocationUpdateRequest request)
        {
            var response = new PracticeLocationUpdateResponse();
            var task = InboxHealthAPI.Put(request, "/partner/v2/practices/" + request.id);
            var jsonResult = task.Result.Content.ReadAsStringAsync();
            jsonResult.Wait();
            if (task.Result.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<PracticeLocationUpdateResponse>(jsonResult.Result);
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = response.ErrorMessage;
            }
            return response;
        }

        public ProviderUpdateResponse UpdateProvider(ProviderUpdateRequest request)
        {
            var response = new ProviderUpdateResponse();
            var task = InboxHealthAPI.Put(request, "/partner/v2/doctors/" + request.id);
            var jsonResult = task.Result.Content.ReadAsStringAsync();
            jsonResult.Wait();
            if (task.Result.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<ProviderUpdateResponse>(jsonResult.Result);
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = response.ErrorMessage;
            }
            return response;
        }


        //Added By Hamza Akhlaq For Inbox Delete API
        public InvoicePaymentsResponse GetPyments_Invoice(long InvoiceId)
        {
            var response = new InvoicePaymentsResponse();
            var task = InboxHealthAPI.Get($"/partner/v2/invoices/{InvoiceId}/invoice_payments");
            var jsonResult = task.Result.Content.ReadAsStringAsync();
            jsonResult.Wait();
            if (task.Result.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<InvoicePaymentsResponse>(jsonResult.Result);
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = response.ErrorMessage;
            }
            return response;
        }

     
        public DeleteinvoicepymentResponse DeletePayments(DeleteInvoicepaymentsRequest deleteInvoicepaymentsRequest, long paymentid)
        {
            var response = new DeleteinvoicepymentResponse();
            var task = InboxHealthAPI.Put(deleteInvoicepaymentsRequest,"/partner/v2/payments/" +paymentid );
            var jsonResult = task.Result.Content.ReadAsStringAsync();
            jsonResult.Wait();
            if (task.Result.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<DeleteinvoicepymentResponse>(jsonResult.Result);
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = response.ErrorMessage;
            }
            return response;
        }

        public DeleteInvoiceResponse DeleteInvoice(long InvoiceId)
        {
            var response = new DeleteInvoiceResponse();
            var task = InboxHealthAPI.Delete($"partner/v2/invoices/{InvoiceId}");
            var jsonResult = task.Result.Content.ReadAsStringAsync();
            jsonResult.Wait();
            if (task.Result.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<DeleteInvoiceResponse>(jsonResult.Result);
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = response.ErrorMessage;
            }
            return response;

        }
    }
}