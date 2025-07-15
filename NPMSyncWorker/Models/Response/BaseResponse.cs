namespace NPMSyncWorker.Models.Response
{
    public abstract class BaseResponse
    {
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
    }
}
