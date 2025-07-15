// Added by Pir Ubaid (USER STORY 204 : NPM ALERT )
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Http;
using NPMAPI.App_Start;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;
using NPMAPI.com.gatewayedi.services;
using System.Data.Entity;
using System.Web.UI;
using NPMAPI.Services;

namespace NPMAPI.Controllers
{
    public class AlertController : BaseController
    {
        private readonly IAlertRepository _alertService;
        public AlertController(IAlertRepository AlertService)
        {
            _alertService = AlertService;
        }
        public AlertController() { }


        [HttpPost]
        public ResponseModel SaveAlert(NpmAlert model)
        {
            return _alertService.SaveAlert(model, GetUserId(), GetUserName());
        }

        //alert enhancement 
        [HttpPost]
        public ResponseModel SaveLoginAlert(NpmAlert model)
        {
            return _alertService.SaveLoginAlert(model, GetUserId(), GetUserName());
        }

        //

        [HttpGet]

        public ResponseModel GetAlertForPatient(long patientaccount)
        {
            return _alertService.GetAlertForPatient(patientaccount);
        }

        //alert enhancement
        [HttpGet]

        public ResponseModel GetLoginAlert()
        {
            return _alertService.GetLoginAlert();
        }

        //

    }
}