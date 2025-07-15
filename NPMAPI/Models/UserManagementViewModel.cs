using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models
{
    public class ModuleViewModel
    {
        public long Module_Id { get; set; }
        [Display(Name = "Module Name")]
        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string Module_Name { get; set; }
        public List<SubModuleViewModel> SubModules { get; set; }
        public long? CreatedBy { get; set; }
        public string CreatedByString { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public long? ModifiedBy { get; set; }
        public string ModifiedByString { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public bool? Deleted { get; set; }

    }
    public class RoleViewModel
    {
        public long RoleId { get; set; }
        [Display(Name = "Role Name")]
        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string RoleName { get; set; }
        public long? CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public bool? Deleted { get; set; }
    }
    public class SubModulePropertiesViewModel
    {
        public long Property_Id { get; set; }
        public string Property_Name { get; set; }
        public long Sub_Module_Id { get; set; }
        public string Sub_Module_Name { get; set; }
        public Sub_Module subModule { get; set; }
        public long? CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public bool? Deleted { get; set; }
        public long ModuleId { get; set; }
        public string ModuleName { get; set; }
    }
    public class SubModulePropertiesCreateViewModel
    {
        public long Property_Id { get; set; }
        [Display(Name = "Property Name")]
        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string Property_Name { get; set; }
        [Required(AllowEmptyStrings = false)]
        public long Sub_Module_Id { get; set; }
        public long? CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public bool? Deleted { get; set; }
    }
    public class SubModuleViewModel
    {
        public long Sub_Module_Id { get; set; }
        [Display(Name = "Module")]
        [Required(AllowEmptyStrings = false)]
        public long Module_Id { get; set; }
        [Display(Name = "Sub Module Name")]
        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string Sub_Module_Name { get; set; }
        public string Module_Name { get; set; }
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTimeOffset> CreatedDate { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTimeOffset> ModifiedDate { get; set; }
        public Nullable<bool> Deleted { get; set; }
        public Module module { get; set; }
        public bool? IsDefaultPropertiesCheck { get; set; }
    }
    public class RoleModulePropertiesViewModel
    {
        public long Role_Id { get; set; }
        public long Module_Id { get; set; }
        public long Property_Id { get; set; }
        public long Sub_Module_Id { get; set; }
        public long CREATED_BY { get; set; }
        public Nullable<System.DateTime> CREATED_DATE { get; set; }

    }

    public class RoleModulePropertyCreateViewModel
    {
        public List<RoleModulePropertiesViewModel> Properties { get; set; }
        [Required(AllowEmptyStrings = false)]
        public long RoleId { get; set; }
    }
    public class UserViewModel
    {
        public long UserId { get; set; }
        public string Password { get; set; }
        public Nullable<bool> IsEmployee { get; set; }
        public string Email { get; set; }
        public Nullable<long> CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleInitial { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public Nullable<long> CreatedBy { get; set; }
        public string CreatedByString { get; set; }
        public Nullable<System.DateTimeOffset> CreatedDate { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public string ModfiedByString { get; set; }
        public Nullable<System.DateTimeOffset> ModifiedDate { get; set; }
        public Nullable<System.DateTimeOffset> PasswordChangeDate { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public long? RoleId { get; set; }
        // added by kamran
        public long? Empid { get; set; }
        public List<SelectListViewModel> Practices { get; set; }
        public string UserName { get; set; }
    }
    public class UserCreateViewModel
    {
        public long UserId { get; set; }
        [Display(Name = "First Name")]
        [MaxLength(25)]
        [Required(AllowEmptyStrings = false)]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        [MaxLength(25)]
        [Required(AllowEmptyStrings = false)]
        public string LastName { get; set; }
        [Display(Name = "Middle Initial")]
        [MaxLength(1)]
        public string MiddleInitial { get; set; }
        [Display(Name = "Email Address")]
        [MaxLength(256)]
        [Required(AllowEmptyStrings = false)]
        [EmailAddress]
        public string Email { get; set; }
        [Display(Name = "Password")]
        [MaxLength(100)]
        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
        [Display(Name = "Confirm Password")]
        [MaxLength(100)]
        [Required(AllowEmptyStrings = false)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
        [Display(Name = "Address")]
        [MaxLength(250)]
        [Required(AllowEmptyStrings = false)]
        public string Address { get; set; }
        [Display(Name = "City")]
        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string City { get; set; }
        [Display(Name = "State")]
        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string State { get; set; }
        [Display(Name = "Postal Code")]
        [MaxLength(10)]
        [Required(AllowEmptyStrings = false)]
        public string PostalCode { get; set; }
        public Nullable<long> CompanyId { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsEmployee { get; set; }
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTimeOffset> CreatedDate { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTimeOffset> ModifiedDate { get; set; }
        public Nullable<System.DateTimeOffset> PasswordChangeDate { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public long? RoleId { get; set; }
        // added by kamran
        public long? Empid { get; set; }
        public List<SelectListViewModel> Practices { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Display(Name = "User ID")]
        [Required(ErrorMessage = "User ID is not provided.")]
        public long UserId { get; set; }
        [Display(Name = "Password")]
        [MaxLength(100)]
        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
        [Display(Name = "Confirm Password")]
        [MaxLength(100)]
        [Required(AllowEmptyStrings = false)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
    public class ResetPasswordForuserViewModel
    {
        [Display(Name = "User ID")]
        [Required(ErrorMessage = "User ID is not provided.")]
        public long UserId { get; set; }
        [Display(Name = "UserName")]
        [Required(ErrorMessage = "UserName is not provided.")]
        public string UserName { get; set; }
        [Display(Name = "OldPassword")]
        [MaxLength(100)]
        [Required(AllowEmptyStrings = false)]
        public string OldPassword { get; set; }
        [Display(Name = "Password")]
        [MaxLength(100)]
        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
        [Display(Name = "Confirm Password")]
        [MaxLength(100)]
        [Required(AllowEmptyStrings = false)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

    public class EmployeeCreateViewModel
    {
        [Required]
        public long Employee_Id { get; set; }
        [MaxLength(13)]
        [MinLength(13)]
        public string Nic_Number { get; set; }
        public Nullable<System.DateTime> Date_Of_Birth { get; set; }
        public int? GenderId { get; set; }
        public int? MaritalStatusId { get; set; }
        public string Permanent_Address { get; set; }
        public string Permanent_City { get; set; }
        public string Permanent_State { get; set; }
        public string Permanent_Postal_Code { get; set; }
        public string Personal_Contact_Number { get; set; }
        public string Home_Contact_Number { get; set; }
        public string Emergency_Contact_Number { get; set; }
        [EmailAddress]
        public string Email_Personal { get; set; }
    }

    public class EmployeeViewModel
    {
        public long Employee_Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleInitial { get; set; }
        public string Nic_Number { get; set; }
        public Nullable<System.DateTime> Date_Of_Birth { get; set; }
        public int? GenderId { get; set; }
        public string Gender { get; set; }
        public int? MaritalStatusId { get; set; }
        public string Marital_Status { get; set; }
        public string Permanent_Address { get; set; }
        public string Permanent_City { get; set; }
        public string Permanent_State { get; set; }
        public string Permanent_Postal_Code { get; set; }
        public string Personal_Contact_Number { get; set; }
        public string Home_Contact_Number { get; set; }
        public string Emergency_Contact_Number { get; set; }
        public string Email_Personal { get; set; }
        public Nullable<long> UserId { get; set; }
    }

    public class EmployeeSelectListsViewModel
    {
        public List<SelectListViewModel> Genders { get; set; }
        public List<SelectListViewModel> MaritalStatus { get; set; }
    }
    public class UserStatusChangeViewModel
    {
        [Required(ErrorMessage = "User Id is Required")]
        public long UserId { get; set; }
        [Required(ErrorMessage = "Status is Required")]
        public bool Status { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
    public class UserSelectListsViewModel
    {
        public List<SelectListViewModel> Companies { get; set; }
        public List<SelectListViewModel> Roles { get; set; }
        public List<SelectListViewModel> Practices { get; set; }
    }
    public class UserModulePropertyCreateViewModel
    {
        public List<UserModulePropertiesViewModel> Properties { get; set; }
        [Required(AllowEmptyStrings = false)]
        public long UserId { get; set; }
    }
    public class UserModulePropertiesViewModel
    {
        public long User_Id { get; set; }
        public long Module_Id { get; set; }
        public long Property_Id { get; set; }
        public long Sub_Module_Id { get; set; }
        public long CREATED_BY { get; set; }
        public Nullable<System.DateTime> CREATED_DATE { get; set; }

    }
    public class LoggedInUserViewModel
    {
        public string Token { get; set; }
   
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Username { get; set; }
        public string email { get; set; }
        public long UserId { get; set; }
        public long RoleId { get; internal set; }
        public string Role { get; internal set; }
        public List<UserPracticeViewModel> Practices { get; set; }
    }
    public class LoggedInUserbyCodeViewModel
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Username { get; set; }
        public string email { get; set; }
        public long UserId { get; set; }
    }
    public class UserPracticeViewModel
    {
        public long PracticeCode { get; set; }
        public string Billing_Type { get; set; }
        public string PracticeName { get; set; }
        public long? Division { get; set; }
        public bool? ClaimNotePopUp { get; set; } = false;

    }
}