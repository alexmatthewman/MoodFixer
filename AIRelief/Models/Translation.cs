using System.ComponentModel.DataAnnotations;

namespace AIRelief.Models
{
    public class Translation
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string Key { get; set; } = "";

        [MaxLength(10)]
        public string Language { get; set; } = "en";

        [MaxLength(10)]
        public string? Market { get; set; }

        public string Value { get; set; } = "";
    }
}
