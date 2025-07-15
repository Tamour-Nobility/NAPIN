using System;

public class PanelBillingSummary
{
    public string Panel_Code { get; set; }
    public string Provider_Name { get; set; }
    public string Location { get; set; }
    public int No_of_CPTs { get; set; }
    public string Created_By { get; set; }
    public DateTime Created_Date { get; set; }
    public string Modified_By { get; set; }
    public DateTime? Modified_Date { get; set; }
    public string Status { get; set; }
}
