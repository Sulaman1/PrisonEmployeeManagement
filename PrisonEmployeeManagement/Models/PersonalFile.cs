using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrisonEmployeeManagement.Models
{
    public class PersonalFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "File Number")]
        [StringLength(50)]
        public string FileNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "File Name")]
        [StringLength(200)]
        public string FileName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Received Date")]
        [DataType(DataType.Date)]
        public DateTime ReceivedDate { get; set; }

        [Required]
        [Display(Name = "Sent By")]
        [StringLength(100)]
        public string SentBy { get; set; } = string.Empty;

        [Display(Name = "Sent By Department")]
        [StringLength(100)]
        public string? SentByDepartment { get; set; }

        [Display(Name = "Action Taken")]
        public string? ActionTaken { get; set; }

        [Required]
        [Display(Name = "Date of Sending")]
        [DataType(DataType.Date)]
        public DateTime DateOfSending { get; set; }

        [Display(Name = "Final Decision By")]
        [StringLength(100)]
        public string? FinalDecisionBy { get; set; }

        [Display(Name = "Final Decision Date")]
        [DataType(DataType.Date)]
        public DateTime? FinalDecisionDate { get; set; }

        [Display(Name = "Final Decision")]
        [StringLength(500)]
        public string? FinalDecision { get; set; }

        [Display(Name = "Status")]
        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Priority")]
        [StringLength(20)]
        public string? Priority { get; set; } = "Medium";

        [Display(Name = "Current Location")]
        [StringLength(100)]
        public string? CurrentLocation { get; set; }

        [Display(Name = "Employee ID")]
        public int? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [Display(Name = "Created By")]
        public int? CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Modified")]
        public DateTime UpdatedAt { get; set; }

        // Navigation property for multiple action dates
        public virtual ICollection<PersonalFileAction> Actions { get; set; } = new List<PersonalFileAction>();
    }

    public class PersonalFileAction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PersonalFileId { get; set; }

        [ForeignKey("PersonalFileId")]
        public virtual PersonalFile? PersonalFile { get; set; }

        [Required]
        [Display(Name = "Action Date")]
        [DataType(DataType.Date)]
        public DateTime ActionDate { get; set; }

        [Required]
        [Display(Name = "Action Taken")]
        [StringLength(500)]
        public string ActionTaken { get; set; } = string.Empty;

        [Display(Name = "Action By")]
        [StringLength(100)]
        public string? ActionBy { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Next Action Date")]
        [DataType(DataType.Date)]
        public DateTime? NextActionDate { get; set; }

        [Display(Name = "Status After Action")]
        [StringLength(30)]
        public string? StatusAfterAction { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }
    }

    public class PersonalFileViewModel
    {
        public int Id { get; set; }
        public string FileNumber { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime ReceivedDate { get; set; }
        public string SentBy { get; set; } = string.Empty;
        public string? ActionTaken { get; set; }
        public DateTime DateOfSending { get; set; }
        public string? FinalDecisionBy { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public List<PersonalFileActionViewModel> Actions { get; set; } = new();
    }

    public class PersonalFileActionViewModel
    {
        public int Id { get; set; }
        public DateTime ActionDate { get; set; }
        public string ActionTaken { get; set; } = string.Empty;
        public string? ActionBy { get; set; }
        public string? Remarks { get; set; }
        public DateTime? NextActionDate { get; set; }
    }
}


