using System.ComponentModel.DataAnnotations;

namespace AIRelief.Models
{
    public class EmailTemplate
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Logical key such as "FeedbackAcknowledgment".
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string TemplateKey { get; set; } = "";

        [Required]
        [MaxLength(10)]
        public string Language { get; set; } = "en";

        [MaxLength(10)]
        public string? Market { get; set; }

        [Required]
        [MaxLength(500)]
        public string Subject { get; set; } = "";

        /// <summary>
        /// Plain-text body with placeholders such as {Name}, {SiteName}, {FeedbackType}.
        /// </summary>
        [Required]
        public string Body { get; set; } = "";
    }
}
