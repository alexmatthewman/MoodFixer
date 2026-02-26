using System.ComponentModel.DataAnnotations;

namespace AIRelief.Models
{
    public enum AuthLevel
    {
        [Display(Name = "User")]
        User = 0,

        [Display(Name = "Group Admin")]
        GroupAdmin = 1,

        [Display(Name = "System Admin")]
        SystemAdmin = 2
    }
}