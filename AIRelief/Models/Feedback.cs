#nullable enable

using System;
using System.ComponentModel.DataAnnotations;

namespace AIRelief.Models
{
    public enum FeedbackType
    {
        Inquiry,
        Complaint,
        Bug,
        Feedback
    }

    public class Feedback
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(400)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public FeedbackType Type { get; set; }

        [Required]
        [StringLength(5000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(260)]
        public string? ImageFileName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(400)]
        public string? AssignedTo { get; set; }

        [StringLength(5000)]
        public string? AdminReply { get; set; }

        public DateTime? RepliedAt { get; set; }

        [Required]
        [StringLength(50)]
        public string TenantCode { get; set; } = "relief";
    }
}

#nullable restore
