using System.Collections.Generic;

namespace NPMAPI.Models
{
    public class CPTChargesViewModel
    {
        public string POS { get; set; }
        public string Charges { get; set; }
        public string Description { get; set; }
        public string Revenue_Code { get; set; }
        public int DefaultUnits { get; set; }
        public bool IsAnesthesiaCpt { get; set; }
        public List<SelectListViewModel> NDCCodeList { get; set; }
    }
}