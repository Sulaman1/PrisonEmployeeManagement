using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrisonEmployeeManagement.Models
{
    public class EmployeeLeave
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [Required]
        [Display(Name = "Leave Type")]
        [StringLength(50)]
        public string LeaveType { get; set; } = string.Empty; // Annual, Sick, Maternity, etc.

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Total Days")]
        public int TotalDays { get; set; }

        [Display(Name = "Status")]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Cancelled

        [Display(Name = "Reason")]
        public string? Reason { get; set; }

        [Display(Name = "Approved By")]
        [StringLength(100)]
        public string? ApprovedBy { get; set; }

        [Display(Name = "Approval Date")]
        [DataType(DataType.Date)]
        public DateTime? ApprovalDate { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }
    }
}