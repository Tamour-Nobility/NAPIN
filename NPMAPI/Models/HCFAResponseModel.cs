using System.Net;

namespace NPMAPI.Models
{
    public class HCFAResponseModel
    {
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public byte[] PdfBytes { get; set; }
    }
}