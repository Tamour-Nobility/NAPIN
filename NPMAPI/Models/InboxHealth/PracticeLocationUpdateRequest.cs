namespace NPMAPI.Models.InboxHealth
{
    public class PracticeLocationUpdateRequest
    {
        public long id { get; set; }
        public PracticesAttribute practice { get; set; }
    }
    public class PracticeLocationUpdateResponse : BaseResponse
    {
        public PracticesAttribute practice { get; set; }
    }
}