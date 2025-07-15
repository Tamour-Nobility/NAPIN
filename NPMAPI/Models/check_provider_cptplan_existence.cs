using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class check_provider_cptplan_existence
    {
        public long Practice_Code { get; set; }
        public long Provider_Code { get; set; }

        public long Location_Code { get; set; }
        public string Location_State { get; set; }

    }
}