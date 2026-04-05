using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrisonEmployeeManagement.Models
{
    public class FileWorkflow
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FileId { get; set; }

        [ForeignKey("FileId")]
        public virtual EFile? File { get; set; }

        [Required]
        [StringLength(50)]
        public string WorkflowNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FromDepartment { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ToDepartment { get; set; } = string.Empty;

        [Required]
        public int FromEmployeeId { get; set; }

        [ForeignKey("FromEmployeeId")]
        public virtual Employee? FromEmployee { get; set; }

        public int? ToEmployeeId { get; set; }

        [ForeignKey("ToEmployeeId")]
        public virtual Employee? ToEmployee { get; set; }

        [StringLength(200)]
        public string? Subject { get; set; }

        public string? Description { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        [StringLength(20)]
        public string? Priority { get; set; }

        public DateTime SentDate { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public DateTime? DueDate { get; set; }

        public bool IsUrgent { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public class FileWorkflowRemark
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WorkflowId { get; set; }

        [ForeignKey("WorkflowId")]
        public virtual FileWorkflow? Workflow { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [Required]
        [Display(Name = "Remark")]
        public string Remark { get; set; } = string.Empty;

        [Display(Name = "Action Type")]
        [StringLength(30)]
        public string ActionType { get; set; } = "Remark";

        [Display(Name = "Attachment Path")]
        public string? AttachmentPath { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }
    }

    public class FileWorkflowViewModel
    {
        public int Id { get; set; }
        public string WorkflowNumber { get; set; } = string.Empty;
        public string FileNumber { get; set; } = string.Empty;
        public string FileTitle { get; set; } = string.Empty;
        public string FromDepartment { get; set; } = string.Empty;
        public string ToDepartment { get; set; } = string.Empty;
        public string FromEmployee { get; set; } = string.Empty;
        public string ToEmployee { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime SentDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public List<FileWorkflowRemarkViewModel> Remarks { get; set; } = new();
    }

    public class FileWorkflowRemarkViewModel
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? AttachmentPath { get; set; }
    }
}