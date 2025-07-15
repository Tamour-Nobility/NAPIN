using System.Collections.Generic;
using NPMAPI.Models;
using System.Web.Http;
using NPMAPI.com.gatewayedi.services;
using System;
using System.Threading.Tasks;

namespace NPMAPI.Repositories
{
    public interface IDemographicRepository
    {
        ResponseModel SearchPatient(PatientSearchModel SearchModel);

        ResponseModel GetPatientList();

        ResponseModel GetPatientModel(long practiceCode);

        ResponseModel IsPatientAlreadyExist(DublicatePatientCheckModel model);

        ResponseModel checkDuplicateDOS(long patientAccount, DateTime dos);


        ResponseModel GetPatient(long PatientAccount);
        ResponseModel GetAdmissionType(long PatientAccount, string claim_no);

        ResponseModel DeletePatient(long PatientAccount);

        ResponseModel GetFinancialGurantor(string Name);

        ResponseModel GetFinancialGurantorSubscriber(string Name);

        ResponseModel SearchInsurance(InsuranceSearchViewModel model);

        ResponseModel DeletePatientInsurance(long PatientAccount, long PatientInsuranceId);

        ResponseModel SavePatientInsurance(Patient_Insurance model);
        ResponseModel SavePatientInsurance(PatientInsuranceViewModel model, long userId);


        ResponseModel GetPatientPicture(long PatientAccount);

        ResponseModel GetCityState(string ZipCode);
        ResponseModel GetState();

        ResponseModel AddEditPatient(PatientCreateViewModel PatientModel, long UserId);

        ResponseModel GetPatientNotes(long PatientAccount, int? number);

        ResponseModel GetPatientNote(long PatientNotesId);

        ResponseModel SavePatientNotes(Patient_Notes PatientNote, long userId);

        ResponseModel DeletePatientNote(long PatientAccount, long PatientNotesId);

        ResponseModel GetAppointments(long PatientAccount);

        List<Practice> GetPatientReferrals(long PatientAccount);

        ResponseModel GetClaimModel(long PatientAccount, long ClaimNo = 0);

        Task<ResponseModel> SaveClaim(ClaimsViewModel cr,  long userId);

        ResponseModel DeleteClaim(long ClaimNo);

        ResponseModel GetPatientClaimsSummary(long PatientAccount = 0, bool IncludeDeleted = false, bool isAmountDueGreaterThanZero = false);

        ResponseModel GetPatientClaim(long ClaimNo = 0);

        ResponseModel GetClaimNotes(long ClaimNo, int? number);

        ResponseModel GetClaimNote(long ClaimNotesId);

        ResponseModel GetServiceTypeCodesDescription(long userId);

        ResponseModel GetModifiers();

        ResponseModel SaveClaimNotes(CLAIM_NOTES ClaimNote, long userId);

        ResponseModel DeleteClaimNote(long ClaimNo, long ClaimNotesId);

        // Claim Screen Calls During Claims Generation
        ResponseModel GetFacility(long PracticeCode);

        //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
        ResponseModel GetFacilitySelectList(long practiceCode);

        ResponseModel SearchFacilities(FacilitySearchModel FacilityModel);

        ResponseModel GetDiagnosis(string DiagCode, string DiagDesc, long PracticeCode);

        ResponseModel SavePatientClaimDiagnose(string DiagCode, long ClaimNo, bool IsEdit, int Sequence);

        ResponseModel EditDiagnosis(string DiagCode, long ClaimNo);

        ResponseModel GetProcedures(long PracticeCode);

        ResponseModel GetProcedureCharges(CPTWiseCharges obj);

        ResponseModel ValidateAddress(ValidateAddreesRequestViewModel model);

        ResponseModel GetPracticeClaims(ClaimSearchViewModel model);

        ResponseModel GetPatientSelectList(string searchText, long practiceCode);

        ResponseModel GetInsuranceSelectList(string searchText);

        ResponseModel GetProviderSelectList(string searchText, long practiceCode, bool all = false);

        ResponseModel GetLocationSelectList(string searchText, long practiceCode, bool all = false);

        // for panel billing - pir ubaid
        ResponseModel GetPanelBillingLocationSelectList(long practiceCode, bool all = false);

        ResponseModel GetPatientSummary(long patientAccount, long practiceCode);

        ResponseModel GetClaimSummaryByNo(long claimNo, long practiceCode);

        ResponseModel GetPatientClaimsForStatement(long patientAccount);

        ResponseModel GeneratePatientStatement(PatientStatementRequest model, long v);

        ResponseModel GetStatementPatient(long practiceCode);

        string GetPatientPicturePath(long patientAccount);

        ResponseModel GetPracticeDefaultLocation(long practiceCode);

        ResponseModel GetCitiesByZipCode(string ZipCode);

        //Added By Pir Ubaid (USER STORY : 205 Prior Authorization)
        ResponseModel GetPAByAccount(long aCCOUNT_NO);

        // Added by Pir Ubaid (USER STORY 204 : NPM ALERT )
        ResponseModel GetClaimAndDos(long patientAcc);

        // Added by Pir Ubaid (USER STORY 598 : Collection Status Addition )
        ResponseModel CheckCollectionStatus(long Claim_no);

        //Added by Pir Ubaid (USER STORY 3055 : Claim summary view changes - Dr. Patel)
        ResponseModel getClaimDetails(long Claim_no);
        //panel billing - pir ubaid

        ResponseModel AddOrUpdatePanelCPTCodes(PanelCPTCodeList panelCPTCodeList, long userId);
        ResponseModel getPanelCodeCpt(long PracticeCode, long ProviderCode, long LocationCode,string cptCode);
        ResponseModel getPanelCodeCptClaim(long PracticeCode, long ProviderCode, long LocationCode, string PanelCode);
        ResponseModel IsAlternateCodeRemoved(string Cpt_Code, string Cpt_Description, string AlernateCode);
        ResponseModel GetPanelAlternateCode(long PracticeCode, long ProviderCode, long LocationCode, string AlernateCode);
        ResponseModel CheckPanelCodeExists(long PracticeCode, long ProviderCode, long LocationCode, string PanelCode, long panelBillingCodeId);
        ResponseModel GetPanelBillingSummaryByPractice(long PracticeCode);
        ResponseModel deleteRows(long[] rowIds);
        ResponseModel GetPanelCodeDetailsForEdits(long panelBillingCodeId);
        ResponseModel PanelCodeStatus(int panelBillingCodeId);
        //

        List<Packet277CAClaimReason_Messages> Show277CAClaimReasons(string claimNo);

        ResponseModel AddPatientStatementNote(PatientStatementResponse patientStatementResponse, long userId);

        ResponseModelForE InquiryByPracPatProvider(long practiceCode, long patAcccount, long providerCode, long insurance_id);
        ResponseModel AddDxToProvider(string diagCode, long practiceCode);

        Task<ResponseModel> AddClaimOverPayment(ClaimOverpayment claimOverpayment, long userId);
        Task  <ResponseModel> GetAllClaimOverPayment(long claimNo);
        //added by samiullah
        Task<ResponseModel> GetStateList();
        //added by pir ubaid - mappd payer population
        ResponseModel ProviderPayerSearchInsurance(InsuranceSearchViewModel model);
        ResponseModel GetClaimsWithPatientDue(long practicecode, long patientaccount);
        ResponseModel InsertOrUpdateClaimPayments(List<Claim_Payments> paymentList, long userId);

    }
}
