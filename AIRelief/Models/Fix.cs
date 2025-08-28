using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AIRelief.Models
{
    public class Fix
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "order")]
        public int order { get; set; }

        [StringLength(1000)]
        [Display(Name = "Fix Heading")]
        public string heading { get; set; }

        [StringLength(1000)]
        [Display(Name = "Main Text")]
        public string maintext { get; set; }

        [StringLength(1000)]
        [Display(Name = "Image")]
        public string image { get; set; }

        [StringLength(100)]
        [Display(Name = "Back Value")]
        public string backvalue { get; set; }

        [StringLength(100)]
        [Display(Name = "Forward Value")]
        public string nextvalue { get; set; }
    }

    public class FixViewModel
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "order")]
        public int order { get; set; }

        [StringLength(1000)]
        [Display(Name = "Fix Heading")]
        public string heading { get; set; }

        [StringLength(1000)]
        [Display(Name = "Main Text")]
        public string maintext { get; set; }
        
        [Display(Name = "Image")]
        public IFormFile image { get; set; }

        [StringLength(100)]
        [Display(Name = "Back Value")]
        public string backvalue { get; set; }

        [StringLength(100)]
        [Display(Name = "Forward Value")]
        public string nextvalue { get; set; }
    }



}
