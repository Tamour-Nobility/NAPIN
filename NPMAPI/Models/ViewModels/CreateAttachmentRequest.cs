using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models.ViewModels
{
    public class CreateAttachmentRequest
    {
        [Required]
        public long Patient_Account { get; set; }
        [Required]
        public string FileName { get; set; }
        [Required]
        public string FilePath { get; set; }
        [Required]
        public int Attachment_TypeCode_Id { get; set; }
        public long? Ticket_id { get; set; } 
    }
}