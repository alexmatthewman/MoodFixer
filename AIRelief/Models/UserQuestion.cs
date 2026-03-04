#nullable enable

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIRelief.Models
{
    public class UserQuestion
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Display(Name = "User")]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public User? User { get; set; }

        [Required]
        [Display(Name = "Question")]
        public int QuestionID { get; set; }

        [ForeignKey("QuestionID")]
        public Question? Question { get; set; }

        [Required]
        [Display(Name = "Date First Attempted")]
        public DateTime DateFirstAttempted { get; set; }

        [Display(Name = "Date Last Attempted")]
        public DateTime? DateLastAttempted { get; set; }

        [Required]
        [Display(Name = "Answered Correctly")]
        public bool AnsweredCorrectly { get; set; }
    }
}

#nullable restore
