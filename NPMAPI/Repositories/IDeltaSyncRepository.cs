using NPMAPI.Models;
using NPMAPI.Models.InboxHealth;

namespace NPMAPI.Repositories
{
    public interface IDeltaSyncRepository
    {
        SyncedPractice GetSyncedPractice(long pracCode);
        SyncedClaim GetSyncedclaim(long claimno);
        PracticeUpdateResponse UpdatePractice(PracticeUpdateRequest request);
        PracticeLocationUpdateResponse UpdatePracticeLocation(PracticeLocationUpdateRequest request);
        ProviderUpdateResponse UpdateProvider(ProviderUpdateRequest request);
        PatientUpdateResponse UpdatePatient(PatientUpdateRequest request);
        ClaimUpdateResponse UpdateClaim(ClaimUpdateRequest request);
        ClaimChargesUpdateResponse deleteClaimCharge(long request);
        ClaimChargesUpdateResponse UpdateClaimCharge(ClaimChargesUpdateRequest request);
        ClaimPaymentUpdateResponse UpdateClaimPayment(ClaimPaymentUpdateRequest request);
        ClaimChargesCreateResponse CreateClaimCharge(ClaimChargesCreateRequest request);
        InvoicePaymentsResponse GetPyments_Invoice(long InvoiceId);
        DeleteinvoicepymentResponse DeletePayments(DeleteInvoicepaymentsRequest deleteInvoicepaymentsRequest, long paymentid);
        DeleteInvoiceResponse DeleteInvoice(long InvoiceId);
    }
}
