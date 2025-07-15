namespace NPMAPI.Models.ViewModels
{
    public class ERASearchRequestModel
    {
        public long practiceCode { get; set; }
        public string checkNo { get; set; }
        public string checkAmount { get; set; }
        public string dateFrom { get; set; }
        public string dateTo { get; set; }
        public string dateType { get; set; }
        public string icnNo { get; set; }
        public string patientAccount { get; set; }
        public string status { get; set; }
    }
}