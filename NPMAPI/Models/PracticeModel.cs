using System.Collections.Generic;

namespace NPMAPI.Models
{
    public class PracticeViewModel
    {
        public Practice PracticeModel { get; set; }

        public List<PracticeLocationViewModel> PracticeLocations { get; set; }

        public List<GetPracticeFacilites_Result> PracticeFacilities { get; set; }

        public List<Provider> ProvidersList { get; set; }

        public List<SelectListViewModel> ProvidersComboFillingList { get; set; }

        public List<SelectListViewModel> LocationComboFillingList { get; set; }

        public List<Practice_Types> PracticeTypeComboFillingList { get; set; }


        public List<SelectListViewModel> CategoryList { get; set; }

        public List<PracticeFacilitesViewModel> SpecialInstructionHistory { get; set; }

        public List<PracticeVendor> PracticeVendors { get; set; }
        public List<Specialty_Groups> SpecialityGroupsList { get; set; }

        public List<WCBRating> WCBRatingList { get; set; }
        public List<Specialization> Specializations { get; set; }

    }

    public class PracticeLocationViewModel
    {
        public long PracticeLocationCode { get; set; }
        public long LocationCode { get; set; }
        public string LocationName { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string CLIANum { get; set; }

    }

    public class PracticeFacilityViewModel
    {
        public long PracticeFacilityCode { get; set; }
        public long PracticeCode { get; set; }
        public long FacilityCode { get; set; }
        public string Facility_Address { get; set; }
        public string NPI { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public bool? isDefault { get; set; }
        public string POS { get; set; }

    }

    public partial class Practice
    {
        public long GroupNo { get; set; }

    }

    public partial class Provider
    {
        public long GroupNo { get; set; }

    }
}