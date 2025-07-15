
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NPMAPI.Models
{
    public class CombinedCodeModel
    {

        public List<Condition_Codes> condition_Codes_List { get; set; }
        public List<Occurrence_Codes> occurrence_Codes_List { get; set; }
        public List<Occurrence_Span_Codes> occurrence_Span_Codes_List { get; set; }
        public List<Value_Codes> Value_CodesList { get; set; }



    }
   





    public class UbDrodowns
    {
        public List<Type_of_facility> Type_of_facilityList { get; set; }
        public List<Type_Of_Admission> Type_Of_AdmissionList { get; set; }
        public List<Type_of_Care> Type_of_CareList { get; set; }
        public List<Discharge_Status> Discharge_StatusList { get; set; }
        public List<Source_of_Admission> Source_of_AdmissionList { get; set; }
        public List<sequence_of_care> sequence_of_careList { get; set; }
        
      

    }
}