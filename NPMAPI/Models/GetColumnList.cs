using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class GetColumnList
    {
        public int id { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string Status  { get; set; }
    }
}