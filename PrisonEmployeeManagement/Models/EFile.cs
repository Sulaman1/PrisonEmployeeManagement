using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrisonEmployeeManagement.Models
{
    public class EFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "File Number")]
        [StringLength(50)]
        public string FileNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "File Title")]
        [StringLength(200)]
        public string FileTitle { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Category")]
        [StringLength(50)]
        public string? Category { get; set; } // Personnel, Disciplinary, Training, Medical, etc.

        [Display(Name = "File Type")]
        [StringLength(50)]
        public string? FileType { get; set; } // PDF, DOC, XLS, Image, etc.

        [Display(Name = "File Path")]
        public string? FilePath { get; set; }

        [Display(Name = "File Size (KB)")]
        public long? FileSize { get; set; }

        [Display(Name = "Related Employee ID")]
        public int? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [Display(Name = "Related Conduct ID")]
        public int? ConductId { get; set; }

        [ForeignKey("ConductId")]
        public virtual EmployeeConduct? Conduct { get; set; }

        [Display(Name = "Related Posting ID")]
        public int? PostingId { get; set; }

        [ForeignKey("PostingId")]
        public virtual EmployeePosting? Posting { get; set; }

        [Display(Name = "Upload Date")]
        [DataType(DataType.DateTime)]
        public DateTime UploadDate { get; set; }

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Status")]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        [Display(Name = "Confidential Level")]
        [StringLength(20)]
        public string? ConfidentialLevel { get; set; } // Public, Internal, Confidential, Secret, Top Secret

        [Display(Name = "Uploaded By")]
        [StringLength(100)]
        public string? UploadedBy { get; set; }

        [Display(Name = "Last Accessed")]
        [DataType(DataType.DateTime)]
        public DateTime? LastAccessed { get; set; }

        [Display(Name = "Access Count")]
        public int AccessCount { get; set; }

        [Display(Name = "Tags")]
        public string? Tags { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Modified")]
        public DateTime UpdatedAt { get; set; }
    }

    public class EFileViewModel
    {
        public int Id { get; set; }
        public string FileNumber { get; set; } = string.Empty;
        public string FileTitle { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? FileType { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime UploadDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ConfidentialLevel { get; set; }
        public long? FileSize { get; set; }
    }

    public class EFileSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
        public string? ConfidentialLevel { get; set; }
        public int? EmployeeId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<EFileViewModel> Files { get; set; } = new();
    }
}