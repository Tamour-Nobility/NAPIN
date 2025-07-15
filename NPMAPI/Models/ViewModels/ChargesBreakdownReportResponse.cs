namespace NPMAPI.Services
{
    internal class ChargesBreakdownReportResponse
    {
        public string CODE { get; set; }
        public string CPT { get; set; }
        public string DESCRIPTION { get; set; }
        public string Max_RVU { get; set; }
        public int? UNITS { get; set; }
        public double? Total_RVU_Value { get; set; }
        public int? COUNT { get; set; }
        public decimal? CHARGES { get; set; }
        public decimal? PERCENTAGE { get; set; }
        public decimal? AVERAGE { get; set; }
        public decimal? BILLED { get; set; }
        public decimal? PERCENTAGE_SEC { get; set; }
        public decimal? AVERAGE_SEC { get; set; }
        public decimal? CONTRACT_WO { get; set; }
        public decimal? PAYMENTS_IN_PERIOD { get; set; }
        public decimal? ADJUSTMENTS_IN_PERIOD { get; set; }
    }
}