namespace NPMAPI.Models
{
    public class PatientSearchModel
    {
        public long PracticeCode { get; set; }
        public long PatientAccount { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SSN { get; set; }
        public long ClaimNo { get; set; }
        public string PolicyNo { get; set; }
        public string HomePhone { get; set; }
        public string ZIP { get; set; }
        public string dateType { get; set; }
        public string dateFrom { get; set; }
        public string dateTo { get; set; }
        public bool IncludePTLPatients { get; set; }
        public bool inActive { get; set; }
        public bool IncludePTLClaims { get; set; }

        public string SearchAllAssignedPractices { get; set; }
        public string dob { get; set; }


    }

    public class CityStateModel
    {
        public string CityName { get; set; }
        public string State { get; set; }
    }
}