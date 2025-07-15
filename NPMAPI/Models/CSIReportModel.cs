using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class CSIReportModel
    {
        public bool? CSI_Feature { get; set; }
        public bool? CSI_Non_Par_Access { get; set; }
        public bool? CSI_Service_Supported { get; set; }
        public bool? CSI_Par_Payer { get; set; }
        public bool? CSI_Enrollment_Required { get; set; }
        public string EnrollmentCompleted { get; set; }
    }
}