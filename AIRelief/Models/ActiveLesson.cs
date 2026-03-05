using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AIRelief.Models
{
    /// <summary>
    /// One question slot in an <see cref="ActiveLesson"/>, built at request time
    /// by <see cref="ActiveLesson.GetRemainingQuestions"/>.
    /// </summary>
    public class LessonQuestionItem
    {
        public Question Question { get; set; }
        public string Category { get; set; }
        public int IndexInSet { get; set; }
    }

    /// <summary>
    /// Persists the current in-progress lesson for a user across sessions.
    /// A row exists only while a lesson is underway; it is deleted once the
    /// user completes (or explicitly abandons) the lesson.
    /// </summary>
    public class ActiveLesson
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }

        /// <summary>Comma-separated Question IDs in presentation order (e.g. "12,7,34,2,19,55").</summary>
        [Required]
        public string QuestionIds { get; set; }

        /// <summary>Pipe-separated category names matching QuestionIds order.</summary>
        [Required]
        public string Categories { get; set; }

        /// <summary>
        /// Index of the question the user must answer next (0-based).
        /// Incremented after each answer is accepted.
        /// </summary>
        public int CurrentIndex { get; set; }

        /// <summary>
        /// JSON-serialised list of <see cref="ActiveLessonResult"/> recording each answered
        /// question so far (used to build the summary and update statistics on completion).
        /// </summary>
        public string ResultsJson { get; set; } = "[]";

        /// <summary>UTC timestamp when this lesson set was first created.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>True when this is the very first lesson the user has ever started.</summary>
        public bool IsFirstEver { get; set; }

        // ?? Computed helpers (not mapped to DB columns) ??????????????

        /// <summary>Total number of questions in this lesson set.</summary>
        [NotMapped]
        public int TotalQuestions =>
            string.IsNullOrEmpty(QuestionIds)
                ? 0
                : QuestionIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;

        /// <summary>
        /// Returns the questions the user has not yet answered, hydrated from
        /// <paramref name="allQuestions"/>. The caller must supply a lookup of
        /// all <see cref="Question"/> rows that appear in <see cref="QuestionIds"/>
        /// to avoid additional DB calls inside the model.
        /// </summary>
        public List<LessonQuestionItem> GetRemainingQuestions(IEnumerable<Question> allQuestions)
        {
            var ids = QuestionIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out int v) ? v : -1)
                .Where(v => v > 0).ToArray();

            var cats = Categories
                .Split('|', StringSplitOptions.RemoveEmptyEntries);

            var lookup = allQuestions.ToDictionary(q => q.ID);
            var items = new List<LessonQuestionItem>();

            for (int i = CurrentIndex; i < ids.Length; i++)
            {
                if (!lookup.TryGetValue(ids[i], out var q)) continue;
                items.Add(new LessonQuestionItem
                {
                    Question   = q,
                    Category   = i < cats.Length ? cats[i] : (q.Category ?? string.Empty),
                    IndexInSet = i
                });
            }

            return items;
        }
    }

    /// <summary>
    /// A single answered-question record stored inside <see cref="ActiveLesson.ResultsJson"/>.
    /// </summary>
    public class ActiveLessonResult
    {
        public int QuestionId { get; set; }
        public string Category { get; set; }
        public bool Correct { get; set; }
    }
}
