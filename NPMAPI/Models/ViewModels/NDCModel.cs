
using NPOI.SS.Formula.Functions;
using NPOI.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NPMAPI.Models.ViewModels
{
    public class NDCModel
    {
        public int NDC_ID { get; set; }
        public string HCPCS_code { get; set; }
        public string ndc_code { get; set; }
        public string labeler_name { get; set; }
        public string drug_name { get; set; }
        public string PKG_Qty { get; set; }
        public DateTime? effectivefrom { get; set; }
        public DateTime? effectiveto { get; set; }
        public string qualifer { get; set; }

        //mychanges
        public long practice_code { get; set; }
        public string description { get; set; }

    }
}