using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class ClaimsDataModel
    {
        public List<string> ConditionCodes { get; set; } = new List<string>();
        public List<OccurrenceCodeModel> OccurrenceCodes { get; set; } = new List<OccurrenceCodeModel>();
        public List<OccurenceSpanModel> OccurrenceSpanCodes { get; set; } = new List<OccurenceSpanModel>();
        public List<ValueeCode> ValueCodes { get; set; } = new List<ValueeCode>();
    }
 

   


}