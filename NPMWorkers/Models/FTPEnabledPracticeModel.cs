using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPMWorkers.Models
{
    public class PracticeFTPViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public string Destination { get; set; }
        public int Port { get; set; }
        public long PracticeCode { get; set; }
    }
}
