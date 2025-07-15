using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class CustomEdits_Columns
    {
        public long   Edit_id { get; set; }
        public string Entity1 { get; set; }
        public string Field1 { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public string Entity2 { get; set; }
        public string Field2 { get; set; }
        public string EditConditon { get; set; }

    }

    public class CustomEdits_ColumnsList
    {
        public Nullable<long> Practice_Code { get; set; }
        public Nullable<long> Gcc_id { get; set; }
        public string EditName { get; set; }
        public string EditDescirption { get; set; }
        public string EditErrorMassage { get; set; }
        public Nullable<System.DateTime> Created_By { get; set; }
        public Nullable<System.DateTime> Created_Date { get; set; }
        public Nullable<System.DateTime> Modified_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public Nullable<bool> Status { get; set; }
        public List<CustomEdits_Columns> customedits { get; set; }
    }
}