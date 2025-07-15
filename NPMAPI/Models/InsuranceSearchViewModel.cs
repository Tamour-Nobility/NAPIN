using System.Collections.Generic;

namespace NPMAPI.Models
{
    public class InsuranceSearchViewModel
    {
        public long PracticeCode { get; set; }
        public string PayerId { get; set; }
        public string PayerDescription { get; set; }
        public long NameId { get; set; }
        public string InsuranceName { get; set; }
        public long InsuranceId { get; set; }
        public string GroupName { get; set; }
        public string InsuranceAddress { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string ZIP { get; set; }

        /// <summary>
        ///  1 means Practice Insurance, 2 means with search from practice state, 3 means search from all database
        /// </summary>
        public int SearchFrom { get; set; }

        /// <summary>
        /// It contains list of group names
        /// </summary>
        public List<SelectListViewModel> GroupNameListModel { get; set; }
    }
}