using System;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models.ViewModels
{
    public class ProviderSchedulesViewModel
    {
        [Required]
        public long PracticeCode { get; set; }
        [Required]
        public long ProviderCode { get; set; }
        [Required]
        public long LocationCode { get; set; }
        [Required]
        public DateTime DateFrom { get; set; }
        [Required]
        public DateTime DateTo { get; set; }
        public int SrNo { get; internal set; }
    }
}