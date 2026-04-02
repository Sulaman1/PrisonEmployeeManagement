using System.ComponentModel.DataAnnotations;

namespace PrisonEmployeeManagement.Models
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Employee ID")]
        public string EmployeeNumber { get; set; } = string.Empty;
        
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;
        
        [Display(Name = "Position")]
        public string Position { get; set; } = string.Empty;
        
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;
        
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Display(Name = "Status")]
        public string EmploymentStatus { get; set; } = string.Empty;
        
        [Display(Name = "Hire Date")]
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; }
        
        [Display(Name = "Age")]
        public int Age { get; set; }
    }
    
    public class EmployeeFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Department { get; set; }
        public string? EmploymentStatus { get; set; }
        public string? Position { get; set; }
        public List<EmployeeViewModel> Employees { get; set; } = new();
    }
}