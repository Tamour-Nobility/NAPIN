using NPMSyncWorker.Models.Request;
using NPMSyncWorker.Models.Response;
using System.Threading.Tasks;

namespace NPMSyncWorker.Repositories.Interfaces
{
    internal interface IExternalSynchronization
    {
        Task<EnterpriseCreateResponse> AddPractice(EnterpriseCreateRequest practice);
        Task<PracticeCreateResponse> AddPracticeAttribute(PracticeCreateRequest practiceCreate);
        Task<PatientCreateResponse> AddPatient(PatientCreateRequest patient);
        Task<ProviderCreateResponse> AddProvider(ProviderCreateRequest provider);
    }
}
