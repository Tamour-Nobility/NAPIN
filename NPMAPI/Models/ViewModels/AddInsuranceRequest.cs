using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models.ViewModels
{
    public class AddInsuranceRequest
    {
        public InsuranceNameModelViewModel insuranceNameModelViewModel { get; set; }
        public InsurancePayerViewModel insurancePayerViewModel { get; set; }
        public Insurance Insurance { get; set; }
   //     public InsuranceTypeCodeModel InsuranceTypeCodeModel { get; set; }


    }
}