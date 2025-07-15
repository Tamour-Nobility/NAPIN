using Newtonsoft.Json;
using NPMSyncWorker.Helpers;
using NPMSyncWorker.Models.Request;
using NPMSyncWorker.Models.Response;
using NPMSyncWorker.Repositories.Interfaces;
using System.Threading.Tasks;

namespace NPMSyncWorker.Repositories
{
    internal class InboxHealthSyncService : IExternalSynchronization
    {
        public async Task<EnterpriseCreateResponse> AddPractice(EnterpriseCreateRequest enterprise)
        {
            var response = new EnterpriseCreateResponse();
            var task = await InboxHealthAPI.Post(enterprise, "/api/partner/v2/enterprises");
            var jsonResult = await task.Content.ReadAsStringAsync();
            if (task.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<EnterpriseCreateResponse>(jsonResult);
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = response.ErrorMessage;
            }
            return response;
        }

        public async Task<PracticeCreateResponse> AddPracticeAttribute(PracticeCreateRequest practiceCreate)
        {
            var response = new PracticeCreateResponse();
            var task = await InboxHealthAPI.Post(practiceCreate, "/api/partner/v2/practices");
            var jsonResult = await task.Content.ReadAsStringAsync();
            if (task.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<PracticeCreateResponse>(jsonResult);
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = response.ErrorMessage;
            }
            return response;
        }

        public async Task<PatientCreateResponse> AddPatient(PatientCreateRequest patient)
        {
            var response = new PatientCreateResponse();
            var task = await InboxHealthAPI.Post(patient, "/api/partner/v2/patients");
            var jsonResult = await task.Content.ReadAsStringAsync();
            if (task.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<PatientCreateResponse>(jsonResult);
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = response.ErrorMessage;
            }
            return response;
        }

        public async Task<ProviderCreateResponse> AddProvider(ProviderCreateRequest provider)
        {
            var response = new ProviderCreateResponse();
            var task = await InboxHealthAPI.Post(provider, "/api/partner/v2/doctors");
            var jsonResult = await task.Content.ReadAsStringAsync();
            if (task.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<ProviderCreateResponse>(jsonResult);
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
