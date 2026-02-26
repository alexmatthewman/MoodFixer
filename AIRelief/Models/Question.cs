#nullable enable

using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace AIRelief.Models
{
    public class Question
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "Order")]
        public int? order { get; set; }

        [StringLength(1000, ErrorMessage = "Heading cannot exceed 1000 characters.")]
        [Display(Name = "Heading")]
        public string? heading { get; set; }

        [Required(ErrorMessage = "Main Text is required.")]
        [StringLength(1000, ErrorMessage = "Main Text cannot exceed 1000 characters.")]
        [Display(Name = "Main Text")]
        public string maintext { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Image path cannot exceed 1000 characters.")]
        [Display(Name = "Image")]
        public string? image { get; set; }

        [StringLength(100, ErrorMessage = "Back Value cannot exceed 100 characters.")]
        [Display(Name = "Back Value")]
        public string? backvalue { get; set; }

        [StringLength(100, ErrorMessage = "Forward Value cannot exceed 100 characters.")]
        [Display(Name = "Forward Value")]
        public string? nextvalue { get; set; }

        [StringLength(2000, ErrorMessage = "Correction Text cannot exceed 2000 characters.")]
        [Display(Name = "Correction Text")]
        public string? correctiontext { get; set; }

        [StringLength(300, ErrorMessage = "Correction Image path cannot exceed 300 characters.")]
        [Display(Name = "Correction Image")]
        public string? correctionimage { get; set; }

        // Required options (first two)
        [Required(ErrorMessage = "Option 1 is required.")]
        [StringLength(500, ErrorMessage = "Option 1 cannot exceed 500 characters.")]
        [Display(Name = "Option 1")]
        public string Option1 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Option 2 is required.")]
        [StringLength(500, ErrorMessage = "Option 2 cannot exceed 500 characters.")]
        [Display(Name = "Option 2")]
        public string Option2 { get; set; } = string.Empty;

        // Optional options (3-8)
        [StringLength(500, ErrorMessage = "Option 3 cannot exceed 500 characters.")]
        [Display(Name = "Option 3")]
        public string? Option3 { get; set; }

        [StringLength(500, ErrorMessage = "Option 4 cannot exceed 500 characters.")]
        [Display(Name = "Option 4")]
        public string? Option4 { get; set; }

        [StringLength(500, ErrorMessage = "Option 5 cannot exceed 500 characters.")]
        [Display(Name = "Option 5")]
        public string? Option5 { get; set; }

        [StringLength(500, ErrorMessage = "Option 6 cannot exceed 500 characters.")]
        [Display(Name = "Option 6")]
        public string? Option6 { get; set; }

        [StringLength(500, ErrorMessage = "Option 7 cannot exceed 500 characters.")]
        [Display(Name = "Option 7")]
        public string? Option7 { get; set; }

        [StringLength(500, ErrorMessage = "Option 8 cannot exceed 500 characters.")]
        [Display(Name = "Option 8")]
        public string? Option8 { get; set; }

        // Correct Answer field
        [Required(ErrorMessage = "Correct Answer is required.")]
        [StringLength(500, ErrorMessage = "Correct Answer cannot exceed 500 characters.")]
        [Display(Name = "Correct Answer")]
        public string CorrectAnswer { get; set; } = string.Empty;
    }
}

#nullable restore