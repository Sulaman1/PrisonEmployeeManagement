using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrisonEmployeeManagement.Models
{
    public class EmployeeConduct
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [Required]
        [Display(Name = "Conduct Type")]
        [StringLength(20)]
        public string ConductType { get; set; } = string.Empty; // "Good" or "Bad"

        [Required]
        [Display(Name = "Category")]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // "Commendation", "Award", "Violation", "Warning", etc.

        [Required]
        [Display(Name = "Title/Description")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Detailed Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Date of Incident")]
        [DataType(DataType.Date)]
        public DateTime IncidentDate { get; set; }

        [Display(Name = "Date Recorded")]
        [DataType(DataType.Date)]
        public DateTime RecordedDate { get; set; }

        [Display(Name = "Severity Level")]
        [StringLength(20)]
        public string? Severity { get; set; } // For bad conduct: "Minor", "Moderate", "Serious", "Critical"

        [Display(Name = "Action Taken")]
        public string? ActionTaken { get; set; }

        [Display(Name = "Disciplinary Action")]
        [StringLength(100)]
        public string? DisciplinaryAction { get; set; } // For bad conduct

        [Display(Name = "Award Type")]
        [StringLength(100)]
        public string? AwardType { get; set; } // For good conduct

        [Display(Name = "Certificate Number")]
        [StringLength(50)]
        public string? CertificateNumber { get; set; }

        [Display(Name = "Issued By")]
        [StringLength(100)]
        public string? IssuedBy { get; set; }

        [Display(Name = "Witnesses")]
        public string? Witnesses { get; set; }

        [Display(Name = "Supporting Documents")]
        public string? SupportingDocuments { get; set; }

        [Display(Name = "Status")]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Resolved, Under Review, Expunged

        [Display(Name = "Resolution Date")]
        [DataType(DataType.Date)]
        public DateTime? ResolutionDate { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Created By")]
        [StringLength(100)]
        public string? CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime UpdatedAt { get; set; }
    }

    public enum ConductType
    {
        Good,
        Bad
    }

    public enum GoodConductCategory
    {
        Commendation,
        OutstandingService,
        Bravery,
        Innovation,
        Leadership,
        CustomerService,
        Teamwork,
        Attendance,
        Performance
    }

    public enum BadConductCategory
    {
        Absenteeism,
        Tardiness,
        Insubordination,
        Misconduct,
        GrossMisconduct,
        Negligence,
        AbuseOfPower,
        Corruption,
        Theft,
        Harassment,
        Violence,
        SubstanceAbuse,
        BreachOfConfidentiality,
        SecurityViolation
    }
}