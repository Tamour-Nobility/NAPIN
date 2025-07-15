using System;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models
{
    public class InsuranceNameViewModel
    {
        public long InsuranceNameId { get; set; }
        public long InsuranceGroupId { get; set; }
        [Display(Name = "Insurance Group name is required")]
        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string InsuranceGroupName { get; set; }
        [Display(Name = "Insurance name is required")]
        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string InsuranceNameDescription { get; set; }
        public bool? Deleted { get; set; }
        public long? Created_By { get; set; }
        public string Created_By_Name { get; set; }
        public DateTimeOffset? Created_Date { get; set; }
        public long? Modified_By { get; set; }
        public string Modified_By_Name { get; set; }
        public DateTimeOffset? Modified_Date { get; set; }
    }


    public class InsuranceNameModelViewModel
    {
        public long Insname_Id { get; set; }
        public long Insgroup_Id { get; set; }
        public string Insname_Description { get; set; }
        public bool? Deleted { get; set; }
        public long? Created_By { get; set; }
        public DateTimeOffset? Created_Date { get; set; }
        public long? Modified_By { get; set; }
        public DateTimeOffset? Modified_Date { get; set; }
        public SelectListViewModel InsuranceGroup { get; set; }
    }
    public class InsuranceTypeCodeModel { 
    public string InsuranceTypeCode { get; set; }
    }
}