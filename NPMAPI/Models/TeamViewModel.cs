using System;
using System.Collections.Generic;

namespace NPMAPI.Models
{
    public class TeamViewModel
    {
        public string DepartmentName { get; set; }
        public string CompanyName { get; set; }
        public string OfficeName { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public string TeamSupervisor { get; set; }
        public string TeamLead { get; set; }
        public string TeamBackupLead { get; set; }
        public int TeamShift { get; set; }
        public int Company { get; set; }
        public int OfficeId { get; set; }
        public int DepartmentId { get; set; }
        public long CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTimeOffset> ModifiedDate { get; set; }
        public bool Deleted { get; set; }
        public string TeamType { get; set; }
    }

    public class TeamDropdownViewModel
    {
        public List<SelectListViewModel> DepartmentList { get; set; }
        public List<SelectListViewModel> OfficeList { get; set; }
    }
}