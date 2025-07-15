using System.Collections.Generic;

namespace NPMAPI.Models.InboxHealth
{
    public class PracticeUpdateRequest
    {
        public long id { get; set; }
        public Enterprise enterprise { get; set; }
    }

    public class Enterprise
    {
        public string name { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string address_line_1 { get; set; }
        public object address_line_2 { get; set; }
        public string zip { get; set; }
        public string support_phone_number { get; set; }
        public float sales_tax { get; set; }
        public string default_quick_pay_description { get; set; }
        public object logo_background_color { get; set; }
        public object default_checkin_routes { get; set; }
        public string statement_descriptor { get; set; }
        public string time_zone { get; set; }
        public bool color_statements { get; set; }
        public bool first_class { get; set; }
        public bool return_envelope { get; set; }
        public bool perforation { get; set; }
        public List<PracticesAttribute> practices_attributes { get; set; }
    }

    public class PracticesAttribute
    {
        public int id { get; set; }
        public string name { get; set; }
        public string address_line_1 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string time_zone { get; set; }
    }
    public class PracticeUpdateResponse : BaseResponse
    {
        public Enterprise enterprise { get; set; }
    }
}