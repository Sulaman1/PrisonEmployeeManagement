using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrisonEmployeeManagement.Models
{
    public class FileAccessLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FileId { get; set; }

        [ForeignKey("FileId")]
        public virtual EFile? File { get; set; }

        [Required]
        [StringLength(100)]
        public string? AccessedBy { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime AccessTime { get; set; }

        [StringLength(50)]
        public string? Action { get; set; } // View, Download, Edit, Delete, Share

        [StringLength(50)]
        public string? IPAddress { get; set; }

        public string? UserAgent { get; set; }

        public string? Reason { get; set; }
    }
}