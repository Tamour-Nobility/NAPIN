using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models.ViewModels
{
    public class PatientNoteViewModel
    {
        public long Patient_Notes_Id { get; set; }
        [Required]
        [DisplayName("Patient Account No")]
        public long Patient_Account { get; set; }
        [DisplayName("Note")]
        [Required(AllowEmptyStrings = false)]
        [MaxLength(500)]
        public string Ptn_Note_Content { get; set; }
        public long? Ptn_Created_By { get; set; }
        public DateTimeOffset? Ptn_Created_Date { get; set; }
        public long Ptn_Modified_By { get; set; }
        public DateTimeOffset? Ptn_Modified_Date { get; set; }

    }
}