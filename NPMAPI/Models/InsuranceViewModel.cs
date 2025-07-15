using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models
{
    public partial class InsurancesByPayersViewModel
    {
        public List<SelectListViewModel> InsuracePayersList { get; set; }

        public List<SelectListViewModel> InsuraceCardCategoryList { get; set; }

        public List<SelectListViewModel> PhoneTypesList { get; set; }
        public List<SelectListViewModel> InsuraceDepartmentList { get; set; }
        public List<SelectListViewModel> EPSDTWorkerInfoList { get; set; }
        public List<SelectListViewModel> SubmissionMethodList { get; set; }
        public SelectListViewModel InsurancePayer { get; internal set; }
    }
    public class InsuranceGroupViewModel
    {
        public long Insgroup_Id { get; set; }
        [Display(Name = "Group Field is required")]
        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string Insgroup_Name { get; set; }
        public long? Created_By { get; set; }
        public DateTimeOffset? Created_Date { get; set; }
        public string Created_By_Name { get; set; }
        public long? Modified_By { get; set; }
        public string Modified_By_Name { get; set; }
        public DateTimeOffset? Modified_Date { get; set; }
        public bool? Deleted { get; set; }
    }
    //public class InsuranceGroupNameViewModel
    //{
    //    [Display(Name = "Group Field is required")]
    //    [MaxLength(50)]
    //    [Required(AllowEmptyStrings = false)]
    //    public string Insgroup_Name { get; set; }
    //}
}