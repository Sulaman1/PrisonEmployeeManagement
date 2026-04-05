using Microsoft.AspNetCore.Identity;

namespace PrisonEmployeeManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string? FirstName { get; set; }

        [PersonalData]
        public string? LastName { get; set; }

        [PersonalData]
        public int? EmployeeId { get; set; }

        [PersonalData]
        public string? Department { get; set; }

        [PersonalData]
        public string? Position { get; set; }

        [PersonalData]
        public DateTime CreatedAt { get; set; }

        [PersonalData]
        public DateTime LastLoginAt { get; set; }

        [PersonalData]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual Employee? Employee { get; set; }
    }
}