namespace NPMAPI.Models
{
    public class DepartmentViewModel
    {
        public long DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public long CompanyId { get; set; }
        public long OfficeId { get; set; }
        public string CompanyName { get; set; }
        public string OfficeName { get; set; }

        public bool Deleted { get; set; }
    }
}