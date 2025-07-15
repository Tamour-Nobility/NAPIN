using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class RegenerateBatchCSIFileModel
    {
        public long Practice_Code { get; set; }
        public long Batch_Id { get; set; }
        public bool Confirmation { get; set; }
    }
}