
namespace NPMAPI.Models
{
    using System;
    using System.Collections.Generic;

    public  class PatientStatementMessages
    {
        public long Message_ID { get; set; }
        public string Messages { get; set; }
        public Nullable<long> PracticeCode { get; set; }
        public Nullable<bool> Deleted { get; set; }
        public Nullable<long> Created_By { get; set; }
        public Nullable<System.DateTime> Created_Date { get; set; }
        public Nullable<long> Modified_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
    }
}
