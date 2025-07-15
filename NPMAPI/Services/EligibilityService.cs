using Newtonsoft.Json;
using NPMAPI.App_Start;
using NPMAPI.com.gatewayedi.services;
using System;
using System.Collections.Generic;
using System.Linq;
using NPMAPI.com.gatewayedi.services;
using NPMAPI.Enums;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;
using Microsoft.AspNet.SignalR.Hosting;

namespace NPMAPI.Services
{
    public class EligibilityService : IEligibility
    {
        private readonly IPracticeRepository _practiceService;
        Eligibility eligObj;
        public EligibilityService(IPracticeRepository practiceRepository)
        {
            eligObj = new Eligibility();
            eligObj.AuthSOAPHeaderValue = new AuthSOAPHeader();
            _practiceService = practiceRepository;
        }

        public WSX12EligibilityResponse DoInquiryByX12Data(DoInquiryByX12DataModel model)
        {
            WSX12EligibilityInquiry inquiry = new WSX12EligibilityInquiry();
            inquiry.GediPayerID = model.GediPayerID;
            inquiry.X12Input = model.X12Input;
            inquiry.ResponseDataType = WSResponseDataType.RawPayerData;
            return eligObj.DoInquiryByX12Data(inquiry);
        }

        public WSEligibilityResponse DoInquiry(DoInquiryModel model, long PracticeCode)
        {
            WSEligibilityInquiry inquiry = new WSEligibilityInquiry();
            List<MyNameValue> parameters = new List<MyNameValue>();

            MyNameValue param = new MyNameValue();
            param.Name = "GediPayerId";
            param.Value = model.GediPayerID;
            parameters.Add(param);

            param = new MyNameValue();
            param.Name = "ProviderId";
            param.Value = model.ProviderId;
            parameters.Add(param);

            param = new MyNameValue();
            param.Name = "Npi";
            param.Value = model.Npi;
            parameters.Add(param);

            param = new MyNameValue();
            param.Name = "InsuranceNum";
            param.Value = model.InsuranceNum;
            parameters.Add(param);

            param = new MyNameValue();
            param.Name = "InsuredFirstName";
            param.Value = model.InsuredFirstName;
            parameters.Add(param);

            param = new MyNameValue();
            param.Name = "InsuredLastName";
            param.Value = model.InsuredLastName;
            parameters.Add(param);

            param = new MyNameValue();
            param.Name = "InsuredSSN";
            param.Value = model.InsuredSSN;
            parameters.Add(param);

            param = new MyNameValue();
            param.Name = "InsuredState";
            param.Value = model.InsuredState;
            parameters.Add(param);

            param = new MyNameValue();
            param.Name = "InsuredDOB";
            param.Value = model.InsuredDOB;
            parameters.Add(param);

            param = new MyNameValue();
            param.Name = "ProviderLastName";
            param.Value = model.ProviderLastName;
            parameters.Add(param);

            param = new MyNameValue();
            param.Name = "ProviderFirstName";
            param.Value = model.ProviderFirstName;
            parameters.Add(param);

            inquiry.Parameters = parameters.ToArray();
            inquiry.ResponseDataType = WSResponseDataType.Xml;

            // Setting username and password of SOAP Service from database for selected practice
            var pracInfo = _practiceService.GetPracticeFTPInfo(PracticeCode, FTPType.EDI);
            if (pracInfo == null && string.IsNullOrEmpty(pracInfo.Username) && string.IsNullOrEmpty(pracInfo.Password))
            {
                eligObj.AuthSOAPHeaderValue.User = "******";
                eligObj.AuthSOAPHeaderValue.Password = "****";
            }
            else
            {
                eligObj.AuthSOAPHeaderValue.User = pracInfo.Username;
                eligObj.AuthSOAPHeaderValue.Password = pracInfo.Password;
            }
            try
            {
                var response = eligObj.DoInquiry(inquiry);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //Console.WriteLine(response.ResponseAsRawString);
            //Console.WriteLine(response.ResponseAsXml);

            
        }

        public ResponseModel GetEligibilityModel(long PracticeCode, long PatientAccount, long ProviderCode, long insurance_id)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    responseModel.Response = ctx.EligRequests(PatientAccount, PracticeCode, ProviderCode, insurance_id).FirstOrDefault();
                    responseModel.Status = "Success";
                }
            }
            catch (Exception)
            {

            }
            return responseModel;
        }

    }
}