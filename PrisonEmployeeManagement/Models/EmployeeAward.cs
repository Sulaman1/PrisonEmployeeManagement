using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrisonEmployeeManagement.Models
{
    public class EmployeeAward
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [Required]
        [Display(Name = "Award Name")]
        [StringLength(200)]
        public string AwardName { get; set; } = string.Empty;

        [Display(Name = "Award Category")]
        [StringLength(50)]
        public string? Category { get; set; }

        [Display(Name = "Date Awarded")]
        [DataType(DataType.Date)]
        public DateTime AwardDate { get; set; }

        [Display(Name = "Presented By")]
        [StringLength(100)]
        public string? PresentedBy { get; set; }

        [Display(Name = "Reason")]
        public string? Reason { get; set; }

        [Display(Name = "Certificate Number")]
        [StringLength(50)]
        public string? CertificateNumber { get; set; }

        [Display(Name = "Monetary Value")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonetaryValue { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }
    }
}