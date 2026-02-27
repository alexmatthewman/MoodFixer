#nullable enable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIRelief.Models
{
    public static class QuestionCategory
    {
        public const string CausalReasoning       = "Causal Reasoning";
        public const string CognitiveReflection   = "Cognitive Reflection";
        public const string ConfidenceCalibration = "Confidence Calibration";
        public const string Metacognition         = "Metacognition";
        public const string ReadingComprehension  = "Reading Comprehension";
        public const string ShortTermMemory       = "Short Term Memory";
        public const string Trial                 = "Trial";

        public static readonly string[] All =
        {
            CausalReasoning, CognitiveReflection, ConfidenceCalibration,
            Metacognition, ReadingComprehension, ShortTermMemory, Trial
        };
    }

    public class Question
    {
        [Key]
        public int ID { get; set; }

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

        [StringLength(2000, ErrorMessage = "Explanation Text cannot exceed 2000 characters.")]
        [Display(Name = "Explanation Text")]
        public string? explanationtext { get; set; }

        [StringLength(300, ErrorMessage = "Explanation Image path cannot exceed 300 characters.")]
        [Display(Name = "Explanation Image")]
        public string? explanationimage { get; set; }

        // Required options (first two)
        [Required(ErrorMessage = "Option 1 is required.")]
        [StringLength(500, ErrorMessage = "Option 1 cannot exceed 500 characters.")]
        [Display(Name = "Option 1")]
        public string Option1 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Option 2 is required.")]
        [StringLength(500, ErrorMessage = "Option 2 cannot exceed 500 characters.")]
        [Display(Name = "Option 2")]
        public string Option2 { get; set; } = string.Empty;

        // Optional options (3-5)
        [StringLength(500, ErrorMessage = "Option 3 cannot exceed 500 characters.")]
        [Display(Name = "Option 3")]
        public string? Option3 { get; set; }

        [StringLength(500, ErrorMessage = "Option 4 cannot exceed 500 characters.")]
        [Display(Name = "Option 4")]
        public string? Option4 { get; set; }

        [StringLength(500, ErrorMessage = "Option 5 cannot exceed 500 characters.")]
        [Display(Name = "Option 5")]
        public string? Option5 { get; set; }

        // Correct Answer field
        [Required(ErrorMessage = "Correct Answer is required.")]
        [StringLength(500, ErrorMessage = "Correct Answer cannot exceed 500 characters.")]
        [Display(Name = "Correct Answer")]
        public string CorrectAnswer { get; set; } = string.Empty;

        [Display(Name = "Attempts Shown")]
        public int? AttemptsShown { get; set; }

        [Display(Name = "Attempts Correct")]
        public int? AttemptsCorrect { get; set; }

        // Stored in DB as a comma-separated string, e.g. "1,3"
        [Display(Name = "Best Answers")]
        public string? BestAnswersRaw { get; set; }

        [NotMapped]
        [Display(Name = "Best Answers")]
        public int[]? BestAnswers
        {
            get
            {
                if (string.IsNullOrWhiteSpace(BestAnswersRaw))
                    return null;
                var parts = BestAnswersRaw.Split(',', System.StringSplitOptions.RemoveEmptyEntries);
                var result = new System.Collections.Generic.List<int>();
                foreach (var p in parts)
                    if (int.TryParse(p.Trim(), out int v))
                        result.Add(v);
                return result.ToArray();
            }
            set => BestAnswersRaw = value is { Length: > 0 } ? string.Join(",", value) : null;
        }

        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
        [Display(Name = "Category")]
        public string? Category { get; set; }
    }
}

#nullable restore