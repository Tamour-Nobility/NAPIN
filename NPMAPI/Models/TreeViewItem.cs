using System.Collections.Generic;

namespace NPMAPI.Models
{
    public class TreeViewItem
    {
        public string text { get; set; }
        public long value { get; set; }
        public bool @checked { get; set; }
        public bool disabled { get; set; }
        public bool collapsed { get; set; }
        public List<TreeViewItem1> children { get; set; }
    }
    public class TreeViewItem1
    {
        public string text { get; set; }
        public long value { get; set; }
        public bool @checked { get; set; }
        public bool disabled { get; set; }
        public bool collapsed { get; set; }
        public List<TreeViewItem> children { get; set; }
    }

}