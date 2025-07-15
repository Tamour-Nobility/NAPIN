using System.Web.Http;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;

namespace NPMAPI.Repositories
{
    public interface IInsuranceSetupRepository
    {
        #region InsuranceGroup
        ResponseModel GetInsuranceGroupList();

        ResponseModel GetInsuranceGroup(long InsuranceGroupId);
        ResponseModel GetSmartInsuranceGroupsSelectList(string searchText);
        ResponseModel GetInsuranceGroupsSelectList();
        ResponseModel SaveInsuranceGroup(Insurance_Groups Model, long userId);
        ResponseModel DeleteInsuranceGroup(long InsuranceGroupId);

        #endregion InsuranceGroup

        #region InsuranceName
        ResponseModel GetInsuranceNameList(long InsuranceGroupId);
        ResponseModel GetInsuranceName(long InsuranceNameId);
        ResponseModel GetInsuranceNameModel(long? insuranceNameId);
        ResponseModel GetInsuranceNameSelectList(long? insuranceGroupId);
        ResponseModel SaveInsuranceName(InsuranceNameModelViewModel Model, long userId);
        ResponseModel DeleteInsuranceName(long InsuranceNameId);
        ResponseModel GetSmartInsuranceNameList(long? insuranceGroupId, string searchText);

        #endregion InsuranceName

        #region InsurancePayer
        ResponseModel GetInsurancePayerList(long InsuranceGroupId, long InsuranceNameId);
        ResponseModel GetInsPayerById(string InsurancePayerId);
        ResponseModel GetInsPayerByState(string InsurancePayerState);
        ResponseModel GetInsurancePayer(long InsurancePayerId);
        ResponseModel GetInsurancePayerModel(long? insurancePayerId);

        ResponseModel SaveInsurancePayer([FromBody] InsurancePayerViewModel Model, long v);

        ResponseModel DeleteInsurancePayer(long InsurancePayerId, long InsuranceNameId);

        ResponseModel GetSmartInsurancePayersList(string searchText);
        #endregion InsurancePayer

        #region Insurances
        ResponseModel GetInsuranceList(long? GroupId, string insuranceName, string insurancePayerName);

        ResponseModel GetInsurance(long InsuranceId);

        ResponseModel SaveInsurance([FromBody] Insurance Model, long v);

        ResponseModel DeleteInsurance(long InsuranceId, long InsurancePayerId);

        ResponseModel GetInsPayerList();

        ResponseModel GetInsuranceModel(long? insuranceId);

        #endregion Insurances
        ResponseModel GetRelationsSelectList();
        #region InsuranceRewamp
        ResponseModel AddInsurance(AddInsuranceRequest addInsuranceRequest, long userId);
        #endregion InsuranceRewamp
    }
}
