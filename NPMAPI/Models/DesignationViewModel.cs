using System;
using System.Collections.Generic;

namespace NPMAPI.Models
{
    public class DesignationViewModel
    {
        public int DesignationId { get; set; }
        public string DesignationName { get; set; }

        public bool Deleted { get; set; }

        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTimeOffset> CreatedDate { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTimeOffset> ModifiedDate { get; set; }


        public Nullable<long> DepartmentID { get; set; }

        public string DepartmentName { get; set; }
        public Nullable<bool> Department_Head { get; set; }

        public List<SelectListViewModel> DepartmentList { get; set; }

    }
}