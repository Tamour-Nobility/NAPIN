using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class BatchStatusModel
    {
        public long BatchId { get; set; } // Assuming `batch_id` is BIGINT
        public string Status { get; set; } // Add other columns as needed
        public bool IsMedicare { get; set; } // This maps to the `IS_Medicare` column (assuming 1 is true, 0 is false)
    }
}