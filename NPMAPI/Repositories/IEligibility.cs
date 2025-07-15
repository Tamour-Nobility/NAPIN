using NPMAPI.com.gatewayedi.services;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;

namespace NPMAPI.Repositories
{
    public interface IEligibility
    {
        WSX12EligibilityResponse DoInquiryByX12Data(DoInquiryByX12DataModel model);
        WSEligibilityResponse DoInquiry(DoInquiryModel model, long PracticeCode);
        ResponseModel GetEligibilityModel(long PracticeCode, long PatientAccount, long ProviderCode,long insurance_id);
    }
}