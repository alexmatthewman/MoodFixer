using System;
using System.Collections.Generic;
using System.Linq;

namespace AIRelief.Models
{
    /// <summary>One row in the per-user table on the Group Statistics page.</summary>
    public class GroupMemberStatRow
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int LessonsCompleted { get; set; }
        public int? OverallPercent { get; set; }

        // Per-category weighted average as 0–100 int, null when the user has no attempts
        public int? CausalReasoning { get; set; }
        public int? CognitiveReflection { get; set; }
        public int? ConfidenceCalibration { get; set; }
        public int? Metacognition { get; set; }
        public int? ReadingComprehension { get; set; }
        public int? ShortTermMemory { get; set; }
    }

    public class GroupStatisticsViewModel
    {
        public string GroupName { get; set; }
        public int GroupId { get; set; }

        /// <summary>One entry per regular user in the group (admins included).</summary>
        public List<GroupMemberStatRow> Members { get; set; } = new();

        /// <summary>Number of members who have completed at least one lesson.</summary>
        public int ActiveMembers => Members.Count(m => m.LessonsCompleted > 0);

        // ?? Group-level averages (across members with at least one attempt) ??

        public int? GroupCausalReasoningAvg =>
            Average(Members, m => m.CausalReasoning);

        public int? GroupCognitiveReflectionAvg =>
            Average(Members, m => m.CognitiveReflection);

        public int? GroupConfidenceCalibrationAvg =>
            Average(Members, m => m.ConfidenceCalibration);

        public int? GroupMetacognitionAvg =>
            Average(Members, m => m.Metacognition);

        public int? GroupReadingComprehensionAvg =>
            Average(Members, m => m.ReadingComprehension);

        public int? GroupShortTermMemoryAvg =>
            Average(Members, m => m.ShortTermMemory);

        public int? GroupOverallAvg =>
            Average(Members, m => m.OverallPercent);

        private static int? Average(List<GroupMemberStatRow> rows, Func<GroupMemberStatRow, int?> selector)
        {
            var values = rows.Select(selector).Where(v => v.HasValue).Select(v => v!.Value).ToList();
            return values.Count > 0 ? (int)Math.Round(values.Average()) : null;
        }
    }
}
