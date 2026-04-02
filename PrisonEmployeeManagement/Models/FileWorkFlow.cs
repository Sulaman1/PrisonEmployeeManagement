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
        [Display(Name = "Workflow Number")]
        [StringLength(50)]
        public string WorkflowNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "From Department")]
        [StringLength(100)]
        public string FromDepartment { get; set; } = string.Empty;

        [Required]
        [Display(Name = "To Department")]
        [StringLength(100)]
        public string ToDepartment { get; set; } = string.Empty;

        [Required]
        [Display(Name = "From Employee")]
        public int FromEmployeeId { get; set; }

        [ForeignKey("FromEmployeeId")]
        public virtual Employee? FromEmployee { get; set; }

        [Display(Name = "To Employee")]
        public int? ToEmployeeId { get; set; }  // Made nullable

        [ForeignKey("ToEmployeeId")]
        public virtual Employee? ToEmployee { get; set; }

        [Display(Name = "Subject")]
        [StringLength(200)]
        public string? Subject { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Status")]
        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Priority")]
        [StringLength(20)]
        public string? Priority { get; set; }

        [Display(Name = "Sent Date")]
        [DataType(DataType.DateTime)]
        public DateTime SentDate { get; set; }

        [Display(Name = "Received Date")]
        [DataType(DataType.DateTime)]
        public DateTime? ReceivedDate { get; set; }

        [Display(Name = "Completed Date")]
        [DataType(DataType.DateTime)]
        public DateTime? CompletedDate { get; set; }

        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "IsUrgent")]
        public bool IsUrgent { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Updated")]
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