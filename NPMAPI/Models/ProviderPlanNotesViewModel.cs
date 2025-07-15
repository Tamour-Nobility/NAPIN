using System;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models
{
    public class ProviderCptPlanNotesVM
    {
        public long Note_Id { get; set; }
        public string Provider_Cpt_Plan_Id { get; set; }
        public DateTime NOTE_DATE { get; set; }
        public string NOTE_CONTENT { get; set; }
        public string NOTE_USER { get; set; }
        public bool? Deleted { get; set; }
        public long? CREATED_BY { get; set; }
        public string CREATED_BY_FULL_NAME { get; set; }
        public DateTimeOffset? CREATED_DATE { get; set; }
        public long? MODIFIED_BY { get; set; }
        public string MODIFIED_BY_FULL_NAME { get; set; }
        public DateTimeOffset? MODIFIED_DATE { get; set; }
    }

    public class ProviderCptPlanNoteCreateVM
    {
        public long Note_Id { get; set; }
        [Required]
        [Display(Name = "Provider CPT Plan ID")]
        [MaxLength(100)]
        public string Provider_Cpt_Plan_Id { get; set; }
        [Required]
        [Display(Name = "Note")]
        [MaxLength(150)]
        [MinLength(10)]
        public string NOTE_CONTENT { get; set; }
    }
}