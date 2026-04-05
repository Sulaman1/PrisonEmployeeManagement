using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrisonEmployeeManagement.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Modified")]
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Permission Name")]
        public string PermissionName { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Module")]
        public string? Module { get; set; } // Employees, Files, Workflow, Reports, etc.

        [StringLength(100)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }
    }

    public class UserRole
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual Employee? User { get; set; }

        [Required]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }

        [Display(Name = "Assigned Date")]
        public DateTime AssignedDate { get; set; }

        [Display(Name = "Assigned By")]
        public int? AssignedBy { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }

    public class RolePermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }

        [Required]
        public int PermissionId { get; set; }

        [ForeignKey("PermissionId")]
        public virtual Permission? Permission { get; set; }

        [Display(Name = "Can View")]
        public bool CanView { get; set; } = true;

        [Display(Name = "Can Create")]
        public bool CanCreate { get; set; } = false;

        [Display(Name = "Can Edit")]
        public bool CanEdit { get; set; } = false;

        [Display(Name = "Can Delete")]
        public bool CanDelete { get; set; } = false;

        [Display(Name = "Can Approve")]
        public bool CanApprove { get; set; } = false;
    }

    public class UserDashboard
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual Employee? User { get; set; }

        [Display(Name = "Dashboard Type")]
        [StringLength(50)]
        public string DashboardType { get; set; } = "Employee"; // Employee, Manager, Admin

        [Display(Name = "Theme Preferences")]
        public string? ThemePreferences { get; set; }

        [Display(Name = "Last Login")]
        public DateTime? LastLogin { get; set; }

        [Display(Name = "Login Count")]
        public int LoginCount { get; set; } = 0;
    }
}