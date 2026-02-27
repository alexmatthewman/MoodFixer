using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIRelief.Models
{
    public class User
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(400, ErrorMessage = "Name cannot exceed 400 characters.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Authentication Level")]
        public AuthLevel AuthLevel { get; set; } = AuthLevel.User;

        [Display(Name = "Group")]
        public int? GroupId { get; set; }

        [ForeignKey("GroupId")]
        public Group Group { get; set; }

        [Display(Name = "Account Expiry Date")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Modified Date")]
        public DateTime? LastModifiedDate { get; set; }

        // Navigation property
        public UserStatistics Statistics { get; set; }
    }
}