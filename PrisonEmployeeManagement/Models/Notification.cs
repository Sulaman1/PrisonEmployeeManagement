using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrisonEmployeeManagement.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual Employee? User { get; set; }

        [Required]
        [Display(Name = "Title")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Notification Type")]
        [StringLength(50)]
        public string NotificationType { get; set; } = string.Empty; // FileReceived, FileForwarded, RemarkAdded, FileApproved, FileRejected, FileClosed

        [Display(Name = "Related Workflow Id")]
        public int? WorkflowId { get; set; }

        [ForeignKey("WorkflowId")]
        public virtual FileWorkflow? Workflow { get; set; }

        [Display(Name = "Related File Id")]
        public int? FileId { get; set; }

        [ForeignKey("FileId")]
        public virtual EFile? File { get; set; }

        [Display(Name = "Is Read")]
        public bool IsRead { get; set; } = false;

        [Display(Name = "Is Sent")]
        public bool IsSent { get; set; } = false; // For email notifications

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Read Date")]
        public DateTime? ReadAt { get; set; }
    }

    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? WorkflowId { get; set; }
        public int? FileId { get; set; }
    }
}