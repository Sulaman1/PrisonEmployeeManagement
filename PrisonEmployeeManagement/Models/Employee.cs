using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrisonEmployeeManagement.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee ID is required")]
        [Display(Name = "Employee ID")]
        [StringLength(20)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Middle Name")]
        [StringLength(50)]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Display(Name = "National ID")]
        [StringLength(50)]
        public string? NationalId { get; set; }

        [Display(Name = "Passport Number")]
        [StringLength(20)]
        public string? PassportNumber { get; set; }

        [Required(ErrorMessage = "Position is required")]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Rank/Grade")]
        [StringLength(50)]
        public string? Rank { get; set; }

        [Display(Name = "Hire Date")]
        [Required(ErrorMessage = "Hire date is required")]
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; }

        [Display(Name = "Confirmation Date")]
        [DataType(DataType.Date)]
        public DateTime? ConfirmationDate { get; set; }

        [Display(Name = "Retirement Date")]
        [DataType(DataType.Date)]
        public DateTime? RetirementDate { get; set; }

        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Alternative Phone")]
        [Phone]
        [StringLength(20)]
        public string? AlternativePhone { get; set; }

        [Display(Name = "Emergency Contact Name")]
        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [Display(Name = "Emergency Contact Phone")]
        [Phone]
        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [Display(Name = "Emergency Contact Relationship")]
        [StringLength(50)]
        public string? EmergencyContactRelationship { get; set; }

        [Display(Name = "Address")]
        [StringLength(200)]
        public string? Address { get; set; }

        [Display(Name = "City")]
        [StringLength(50)]
        public string? City { get; set; }

        [Display(Name = "State")]
        [StringLength(50)]
        public string? State { get; set; }

        [Display(Name = "Zip Code")]
        [StringLength(10)]
        public string? ZipCode { get; set; }

        [Display(Name = "Country")]
        [StringLength(50)]
        public string? Country { get; set; } = "USA";

        [Display(Name = "Employment Status")]
        [StringLength(20)]
        public string EmploymentStatus { get; set; } = "Active";

        [Display(Name = "Employee Type")]
        [StringLength(30)]
        public string? EmployeeType { get; set; } = "Permanent";

        [Display(Name = "Security Clearance Level")]
        [StringLength(50)]
        public string? SecurityClearanceLevel { get; set; }

        [Display(Name = "Clearance Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? ClearanceExpiryDate { get; set; }

        [Display(Name = "Badge Number")]
        [StringLength(20)]
        public string? BadgeNumber { get; set; }

        [Display(Name = "Shift Schedule")]
        [StringLength(50)]
        public string? ShiftSchedule { get; set; }

        [Display(Name = "Qualifications")]
        public string? Qualifications { get; set; }

        [Display(Name = "Skills")]
        public string? Skills { get; set; }

        [Display(Name = "Languages")]
        public string? Languages { get; set; }

        [Display(Name = "Medical Conditions")]
        public string? MedicalConditions { get; set; }

        [Display(Name = "Blood Type")]
        [StringLength(5)]
        public string? BloodType { get; set; }

        [Display(Name = "Date Created")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "Created By")]
        [StringLength(100)]
        public string? CreatedBy { get; set; }

        [Display(Name = "Last Modified By")]
        [StringLength(100)]
        public string? LastModifiedBy { get; set; }

        // Navigation Properties
        public virtual ICollection<EmployeeConduct> ConductRecords { get; set; } = new List<EmployeeConduct>();
        public virtual ICollection<EmployeePosting> Postings { get; set; } = new List<EmployeePosting>();
        public virtual ICollection<EmployeeTraining> Trainings { get; set; } = new List<EmployeeTraining>();
        public virtual ICollection<EmployeeAward> Awards { get; set; } = new List<EmployeeAward>();
        public virtual ICollection<EmployeeLeave> Leaves { get; set; } = new List<EmployeeLeave>();

        [NotMapped]
        [Display(Name = "Full Name")]
        public string FullName => $"{LastName}, {FirstName} {MiddleName}".Trim();

        [NotMapped]
        [Display(Name = "Age")]
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Year;
                if (DateOfBirth.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        [NotMapped]
        [Display(Name = "Years of Service")]
        public double YearsOfService
        {
            get
            {
                var endDate = RetirementDate ?? DateTime.Today;
                return (endDate - HireDate).TotalDays / 365.25;
            }
        }

        [NotMapped]
        public string CurrentPosting => Postings?.FirstOrDefault(p => p.IsCurrent)?.Location ?? "Not Assigned";
    }
}