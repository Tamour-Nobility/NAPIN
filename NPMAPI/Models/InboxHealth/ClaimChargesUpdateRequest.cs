namespace NPMAPI.Models.InboxHealth
{
    public class ClaimChargesUpdateRequest
    {
        public long id { get; set; }
        public LineItemsAttribute line_item { get; set; }
    }
    public class ClaimChargesCreateRequest
    {
        public LineItemsAttribute line_item { get; set; }
    }
    public class LineItemsAttribute
    {
        public long invoice_id { get; set; }
        public string service_code { get; set; }
        public string description { get; set; }
        public string date_of_service { get; set; }
        public decimal total_charge_amount_cents { get; set; }
        public decimal insurance_owed_amount_cents { get; set; }
        public decimal covered_amount_cents { get; set; }
        public int quantity { get; set; }
        //Added below one prop by Hamza Ikhlaq for  inbox inbox amount due fixation
        public decimal insurance_balance_cents { get; set; }
    }
    public class LineItemsReponseAttribute
    
    {
        public long id { get; set; }
        public long invoice_id { get; set; }
        public string service_code { get; set; }
        public string description { get; set; }
        public string date_of_service { get; set; }
        public decimal total_charge_amount_cents { get; set; }
        public decimal covered_amount_cents { get; set; }
        public int quantity { get; set; }
    }
    public class ClaimChargesUpdateResponse : BaseResponse
    {
        public LineItemsAttribute line_item { get; set; }
    }
    public class ClaimChargesCreateResponse : BaseResponse
    {
        public LineItemsReponseAttribute line_item { get; set; }
    }
}