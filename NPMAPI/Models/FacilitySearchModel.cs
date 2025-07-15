using System;

namespace NPMAPI.Models
{
    public class FacilitySearchModel
    {
        public long? FacilityCode { get; set; }
        public Nullable<long> PracticeCode { get; set; }
        public string FacilityName { get; set; }
        public string FacilityType { get; set; }
        public string NPI { get; set; }
        public string ZIP { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Phone { get; set; }


    }
}