using System;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models
{
    public class DublicatePatientCheckModel
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Zip { get; set; }

        public long PatientAccount { get; set; }
        [Required]
        public DateTime DOB { get; set; }
        [Required]
        public int PracticeCode { get; set; }
        public bool? PTLStatus { get; set; }
    }
}