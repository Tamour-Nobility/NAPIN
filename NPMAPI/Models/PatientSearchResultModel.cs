using System;
using System.Collections.Generic;

namespace NPMAPI.Models
{
    public class PatientSearchResultModel
    {
        public long PracticeCode { get; set; }
        public long PatientAccount { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SSN { get; set; }
        public long Email { get; set; }
        public string HomePhone { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZIP { get; set; }

        public List<PatientSearchClaimResultModel> objPatientClaimList { get; set; }
    }

    public class PatientSearchClaimResultModel
    {
        public long PracticeCode { get; set; }
        public long PatientAccount { get; set; }
        public long ClaimNo { get; set; }
        public DateTime DOS { get; set; }

    }
}