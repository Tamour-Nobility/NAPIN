using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class CustomEdits_FrontEnd
    {
        public int ID { get; set; }
        public string Column_Name { get; set; }
        public string Table_Name { get; set; }
        public string Operator { get; set; }
        public string Custom_Values { get; set; }
        public string Practice_Code { get; set; }
        public string userquery { get; set; }
        public string UserErrorMassage { get; set; }
    }
}