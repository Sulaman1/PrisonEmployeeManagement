using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrisonEmployeeManagement.Models
{
    public class EmployeePosting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [Required]
        [Display(Name = "Location/Facility")]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Facility Type")]
        [StringLength(50)]
        public string? FacilityType { get; set; } // Maximum Security, Medium Security, Minimum Security, etc.

        [Display(Name = "Department")]
        [StringLength(100)]
        public string? Department { get; set; }

        [Display(Name = "Position/Designation")]
        [StringLength(100)]
        public string? Position { get; set; }

        [Display(Name = "Rank at Posting")]
        [StringLength(50)]
        public string? RankAtPosting { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Is Current Posting")]
        public bool IsCurrent { get; set; }

        [Display(Name = "Posting Type")]
        [StringLength(30)]
        public string? PostingType { get; set; } // Permanent, Temporary, Acting, Secondment

        [Display(Name = "Reason for Transfer")]
        public string? TransferReason { get; set; }

        [Display(Name = "Order/Reference Number")]
        [StringLength(50)]
        public string? OrderNumber { get; set; }

        [Display(Name = "Approved By")]
        [StringLength(100)]
        public string? ApprovedBy { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Created By")]
        [StringLength(100)]
        public string? CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime UpdatedAt { get; set; }
    }
}