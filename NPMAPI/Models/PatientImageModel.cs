namespace NPMAPI.Models
{
    public class PatientImageModel
    {
        public long PatientAccount { get; set; }
        public byte[] PatientPictureBytes { get; set; }
    }
}