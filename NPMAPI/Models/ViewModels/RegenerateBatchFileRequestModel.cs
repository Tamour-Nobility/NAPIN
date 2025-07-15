namespace NPMAPI.Models.ViewModels
{
    public class RegenerateBatchFileRequestModel
    {
        public long Practice_Code { get; set; }
        public long Batch_Id { get; set; }
        public bool Confirmation { get; set; }
    }
}