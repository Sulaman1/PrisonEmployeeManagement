using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrisonEmployeeManagement.Models
{
    public class EmployeeTraining
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [Required]
        [Display(Name = "Training Name")]
        [StringLength(200)]
        public string TrainingName { get; set; } = string.Empty;

        [Display(Name = "Training Provider")]
        [StringLength(100)]
        public string? Provider { get; set; }

        [Display(Name = "Training Type")]
        [StringLength(50)]
        public string? TrainingType { get; set; } // Mandatory, Professional Development, Technical, etc.

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Duration (Hours)")]
        public int? DurationHours { get; set; }

        [Display(Name = "Certificate Number")]
        [StringLength(50)]
        public string? CertificateNumber { get; set; }

        [Display(Name = "Certificate Expiry")]
        [DataType(DataType.Date)]
        public DateTime? CertificateExpiry { get; set; }

        [Display(Name = "Status")]
        [StringLength(20)]
        public string? Status { get; set; } // Completed, In Progress, Pending, Failed

        [Display(Name = "Score/Grade")]
        [StringLength(20)]
        public string? Score { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }
    }
}