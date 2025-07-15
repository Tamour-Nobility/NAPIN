using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
   public class BatchCSIResponse
{
    public long ClaimId { get; set; }
    public string PracticeCode { get; set; }
    public string response { get; set; }
    public long BatchId { get; set; }
}

}