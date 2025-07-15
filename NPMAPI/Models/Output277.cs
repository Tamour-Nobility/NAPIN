using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class Output277
    {
        public string Transaction276 { get; set; }
        public string Transaction277 { get; set; }
        public string ErrorMessage { get; set; }
        public List<_277Header> ClaimStatusData { get; set; }
        public string stcStatusCategoryDescription { get; set; }
        public string STCDescription { get; set; }
        public string STCStatus { get; set; }
        public string StcEntitySegments { get; set; }
        public string DTPDates { get; set; }
        public string stcDescriptions { get; set; }
        public string StcStatusDescription { get; set; }
        public string StcCategoryDescription { get; set; }
        public string EnteredBy { get; set; }
    }

    public class _277Header
    {
        public string PayloadType { get; set; }
        public string ProcessingMode { get; set; }
        public string PayloadID { get; set; }
        public DateTime TimeStamp { get; set; }
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public string CORERuleVersion { get; set; }
        public string Payload { get; set; }

    }
}