using System.Collections.Generic;

namespace AIRelief.Models
{
    public class CategoryResult
    {
        public string Category { get; set; }
        public int Attempted { get; set; }
        public int Correct { get; set; }
        public int ScorePercent => Attempted > 0 ? (int)((double)Correct / Attempted * 100) : 0;
    }

    public class QuestionSummaryItem
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string Category { get; set; }
        public bool UserCorrect { get; set; }
        /// <summary>Global success rate across all users (0–100), or null if no attempts recorded.</summary>
        public int? GlobalSuccessPercent { get; set; }
    }

    public class LessonSummaryViewModel
    {
        /// <summary>Results for the questions just answered in this session.</summary>
        public List<CategoryResult> SessionResults { get; set; } = new();

        /// <summary>Per-question breakdown including global success rate.</summary>
        public List<QuestionSummaryItem> QuestionSummaryItems { get; set; } = new();

        /// <summary>Overall statistics per category drawn from UserStatistics.</summary>
        public UserStatistics Statistics { get; set; }

        /// <summary>Number of lessons this user has completed (used to gate the Overall Progress section).</summary>
        public int LessonsCompleted { get; set; }
    }
}
