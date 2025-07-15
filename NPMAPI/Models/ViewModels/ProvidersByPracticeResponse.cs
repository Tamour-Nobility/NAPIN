using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models.ViewModels
{
    public class ProvidersByPracticeResponse
    {
        public long ProviderId { get; set; }
        public string ProviderFullName { get; set; }
    }
}