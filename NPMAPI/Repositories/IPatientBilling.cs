namespace NPMAPI.Repositories
{
    public interface IPatientBilling
    {
        int onPatientPaymentProcessed(dynamic eventData);
    }
}