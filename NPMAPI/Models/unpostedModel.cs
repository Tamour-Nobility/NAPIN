using System;

public class ClaimDetails
{
    public string ClaimNumber { get; set; }
    public DateTime DOS { get; set; } // Date of Service
    public string PatientAccountNumber { get; set; }
    public string PatientName { get; set; }
    public string CPTCode { get; set; }
    public string BillingProvider { get; set; }
    public DateTime EntryDate { get; set; }
    public string PaymentSource { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal SkippedAmount { get; set; }
    public decimal AmountRejected { get; set; }
    public string InsuranceName { get; set; }
    public string CheckNumber { get; set; }
    public DateTime CheckDate { get; set; }
}
