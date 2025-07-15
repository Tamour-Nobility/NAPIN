using EdiFabric.Templates.Hipaa5010;

namespace NPMAPI.Models
{

    public class SelectListViewModel
    {
        public string IdStr { get; set; }
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public long OfficeId { get; set; }
        public string Name { get; set; }
        public dynamic Meta { get; set; }
        public bool? In_Active { get; set; }
    }


    //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
    public class FacilityResult
    {
        public long Facility_Code { get; set; }
        public string Facility_Name { get; set; }
    }

    public class SelectListViewModelForProvider
    {
        public string IdStr { get; set; }
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public bool? is_active { get; set; }
        public long OfficeId { get; set; }
        public string Name { get; set; }
        public dynamic Meta { get; set; }
        public string SPECIALIZATION_CODE { get; set; }
        public string provider_State { get; set; }


    }
}