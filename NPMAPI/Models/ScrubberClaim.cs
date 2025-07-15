using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class ScrubberClaim
    {
        public Int64 ClaimNo { get; set; }
        public string PatientName { get; set; }
        public Int64 FacilityCode { get; set; }
        public string FacilityName { get; set; }
        public DateTime DOS { get; set; }
        public DateTime CreatedDate { get; set; }
        public Int64 PatientAccount { get; set; }
        public string ErrorMessage { get; set; }
        public Char Status { get; set; }
        public int ClaimTotal { get; set; }
    }
}