using System.Collections.Generic;

namespace NPMAPI.Models
{
    public class DashboardViewModel
    {
        public List<SP_AGINGCOMPDEMO_Result> AgingWithlastReport { get; set; }
        public List<SP_CPADEMO_Result> ChargesPaymentComparisonReport { get; set; }
        public List<SP_AGINGDEMO_Result> AgingAnalysis { get; set; }

        public List<dynamic> AgingComparisonChartLabels { get; set; }
        public List<dynamic> AgingComparisonChartData { get; set; }


        public List<dynamic> ChargesVsPaymentsChartLabels { get; set; }
        public List<dynamic> ChargesVsPaymentsChartData { get; set; }
    }

    public class ReportData
    {
        public string[] data { get; set; }
        public string lablel { get; set; }
    }
    public class PanelCPTCodeList
    {
        public long PracticeCode { get; set; }
        public long ProviderCode { get; set; }
        public long LocationCode { get; set; }
        public string PanelCode { get; set; }
        public long Panel_Billing_Code_Id { get; set; }
        public List<PanelCPTCode> CPTCodes { get; set; } // List to hold CPT details
                                                         // Add RowsToDelete property as a list of CPT Code IDs to be deleted
        //public List<long> RowsToDelete { get; set; } = new List<long>();  // List of CPT Code IDs to delete

    }

    public class PanelCPTCode
    {
       public long Panel_Billing_Code_Id { get; set ; }
        public long PanelBillingCodeId { get; set; }
        public long Panel_Billing_Code_CPTId { get; set; }
        public string CPTCode { get; set; }
        public string CPTDescription { get; set; }
        public string M_1 { get; set; }
        public string M_2 { get; set; }
        public string M_3 { get; set; }
        public string M_4 { get; set; }
        public string AlternateCode { get; set; }
        public decimal? Charges { get; set; }
        public int? Units { get; set; }
    }

    public  class GetPanelCptDetails_Result
    {
        public long Provider_Cpt_Plan_Detail_Id { get; set; }
        public string Provider_Cpt_Plan_Id { get; set; }
        public string Cpt_Code { get; set; }
        public string Cpt_Description { get; set; }
        public string Alternate_Code { get; set; }
        public string Charges { get; set; }
    }

}