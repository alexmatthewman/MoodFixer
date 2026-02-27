using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIRelief.Models
{
    public enum QueryFrequency
    {
        Daily,
        Weekly,
        BiWeekly,
        Monthly,
        Quarterly
    }

    public class Group
    {
        [Key]
        public int ID { get; set; }

        
        [StringLength(400, ErrorMessage = "Plan name cannot exceed 400 characters.")]
        [Display(Name = "Plan Name")]
        public string? PlanName { get; set; }

        [Required]
        [StringLength(400, ErrorMessage = "Group name cannot exceed 400 characters.")]
        [Display(Name = "Group/Company Name")]
        public string Name { get; set; }

        [StringLength(2000, ErrorMessage = "Group image URL cannot exceed 2000 characters.")]
        [Display(Name = "Group/Company Logo URL")]
        [Url(ErrorMessage = "Please provide a valid URL.")]
        public string GroupImageUrl { get; set; }

        [StringLength(7, ErrorMessage = "Primary color must be a valid hex color code.")]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Primary color must be a valid hex color code (e.g., #FF5733).")]
        [Display(Name = "Primary Theme Color")]
        public string PrimaryThemeColor { get; set; } = "#dd9292";

        [StringLength(7, ErrorMessage = "Secondary color must be a valid hex color code.")]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Secondary color must be a valid hex color code (e.g., #FF5733).")]
        [Display(Name = "Secondary Theme Color")]
        public string SecondaryThemeColor { get; set; } = "#bfc0df";

        [Required]
        [Display(Name = "Number of User Licenses")]
        public int NumberOfUserLicenses { get; set; }

        [Display(Name = "Account Expiry (Days)")]
        public int? ExpiryDays { get; set; } = 365;

        [Display(Name = "Expiry Date")]
        public DateTime? ExpiryDateTime { get; set; }

        [Required]
        [Display(Name = "Query Time to Complete (Days)")]
        public int QueryTimeToCompleteDays { get; set; } = 1;

        [Required]
        [Display(Name = "Query Frequency")]
        public QueryFrequency QueryFrequency { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Query Passing Grade must be between 0 and 100.")]
        [Display(Name = "Query Passing Grade (%)")]
        public int QueryPassingGrade { get; set; } = 50;

        [Required]
        [Display(Name = "Use Random Questions")]
        public bool QueryQuestionsRandom { get; set; }

        [Required]
        [Display(Name = "Use Focussed Questions")]
        public bool QueryQuestionsFocussed { get; set; } = true;

        [Required]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Modified Date")]
        public DateTime? LastModifiedDate { get; set; }

        // Navigation property
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}