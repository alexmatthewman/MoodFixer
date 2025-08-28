using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace AIRelief.Models
{
    public class Question
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "order")]
        public int order { get; set; }

        [StringLength(1000)]
        [Display(Name = "Heading")]
        public string heading { get; set; }

        [StringLength(1000)]
        [Display(Name = "Main Text")]
        public string maintext { get; set; }

        [StringLength(1000)]
        [Display(Name = "Image")]
        public string image { get; set; }

        [StringLength(100)]
        [Display(Name = "Back Value")]
        public string? backvalue { get; set; } // Optional

        [StringLength(100)]
        [Display(Name = "Forward Value")]
        public string? nextvalue { get; set; } // Optional

        // 8 optional fields for question answers
        [Display(Name = "Answer 1")]
        public string? Answer1 { get; set; }

        [Display(Name = "Answer 2")]
        public string? Answer2 { get; set; }

        [Display(Name = "Answer 3")]
        public string? Answer3 { get; set; }

        [Display(Name = "Answer 4")]
        public string? Answer4 { get; set; }

        [Display(Name = "Answer 5")]
        public string? Answer5 { get; set; }

        [Display(Name = "Answer 6")]
        public string? Answer6 { get; set; }

        [Display(Name = "Answer 7")]
        public string? Answer7 { get; set; }

        [Display(Name = "Answer 8")]
        public string? Answer8 { get; set; }
    }
}
