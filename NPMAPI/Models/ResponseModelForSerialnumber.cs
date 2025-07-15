using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class ClaimStatusTRNNumberModel
    {
        public string SerialNumber { get; set; }
    }

    public class ClaimStatusISAIEANumberModel
    {
        public string SequancerNumber { get; set; }
    }
    public class ResponseModelSerialNumber
    {
        public string Status { get; set; }
        public dynamic Response { get; set; }
        public string Message { get; set; }
        public List<ClaimStatusTRNNumberModel> obj { get; set; }
    }

    public class ResponseModelSequancerNumber
    {
        public string Status { get; set; }
        public dynamic Response { get; set; }
        public string Message { get; set; }
        public List<ClaimStatusISAIEANumberModel> obj { get; set; }
    }
    public class ResponseModelForSerialnumber
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string SequanceNumber { get; set; } = string.Empty;
    }
}