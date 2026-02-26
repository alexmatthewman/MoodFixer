using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AIRelief.Models
{
    /// <summary>
    /// Tracks performance statistics for a user across six cognitive domains.
    /// Each user has exactly one UserStatistics record with weighted averages calculated for performance tracking.
    /// </summary>
    public class UserStatistics
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [ForeignKey("User")]
        [Display(Name = "User ID")]
        public int UserId { get; set; }

        public User User { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // ===== Cognitive Reflection =====
        [Display(Name = "Cognitive Reflection Attempts")]
        public int CognitiveReflectionAttempts { get; set; }

        [Display(Name = "Cognitive Reflection Passed")]
        public int CognitiveReflectionPassed { get; set; }

        [Range(0, 100)]
        [Display(Name = "Cognitive Reflection Score")]
        public int CognitiveReflectionScore { get; set; }

        [Range(0, 100)]
        [Display(Name = "Cognitive Reflection Weighted Average")]
        public decimal CognitiveReflectionWeightedAverage { get; set; }

        // ===== Reading Comprehension =====
        [Display(Name = "Reading Comprehension Attempts")]
        public int ReadingComprehensionAttempts { get; set; }

        [Display(Name = "Reading Comprehension Passed")]
        public int ReadingComprehensionPassed { get; set; }

        [Range(0, 100)]
        [Display(Name = "Reading Comprehension Score")]
        public int ReadingComprehensionScore { get; set; }

        [Range(0, 100)]
        [Display(Name = "Reading Comprehension Weighted Average")]
        public decimal ReadingComprehensionWeightedAverage { get; set; }

        // ===== Causal Reasoning =====
        [Display(Name = "Causal Reasoning Attempts")]
        public int CausalReasoningAttempts { get; set; }

        [Display(Name = "Causal Reasoning Passed")]
        public int CausalReasoningPassed { get; set; }

        [Range(0, 100)]
        [Display(Name = "Causal Reasoning Score")]
        public int CausalReasoningScore { get; set; }

        [Range(0, 100)]
        [Display(Name = "Causal Reasoning Weighted Average")]
        public decimal CausalReasoningWeightedAverage { get; set; }

        // ===== Metacognition =====
        [Display(Name = "Metacognition Attempts")]
        public int MetacognitionAttempts { get; set; }

        [Display(Name = "Metacognition Passed")]
        public int MetacognitionPassed { get; set; }

        [Range(0, 100)]
        [Display(Name = "Metacognition Score")]
        public int MetacognitionScore { get; set; }

        [Range(0, 100)]
        [Display(Name = "Metacognition Weighted Average")]
        public decimal MetacognitionWeightedAverage { get; set; }

        // ===== Short-Term Memory =====
        [Display(Name = "Short-Term Memory Attempts")]
        public int ShortTermMemoryAttempts { get; set; }

        [Display(Name = "Short-Term Memory Passed")]
        public int ShortTermMemoryPassed { get; set; }

        [Range(0, 100)]
        [Display(Name = "Short-Term Memory Score")]
        public int ShortTermMemoryScore { get; set; }

        [Range(0, 100)]
        [Display(Name = "Short-Term Memory Weighted Average")]
        public decimal ShortTermMemoryWeightedAverage { get; set; }

        // ===== Confidence Calibration =====
        [Display(Name = "Confidence Calibration Attempts")]
        public int ConfidenceCalibrationAttempts { get; set; }

        [Display(Name = "Confidence Calibration Passed")]
        public int ConfidenceCalibrationPassed { get; set; }

        [Range(0, 100)]
        [Display(Name = "Confidence Calibration Score")]
        public int ConfidenceCalibrationScore { get; set; }

        [Range(0, 100)]
        [Display(Name = "Confidence Calibration Weighted Average")]
        public decimal ConfidenceCalibrationWeightedAverage { get; set; }

        // ===== Overall Performance =====
        [Range(0, 100)]
        [Display(Name = "Overall Weighted Average")]
        public decimal OverallWeightedAverage { get; set; }

        /// <summary>
        /// Calculates weighted average for a given domain based on attempts and score.
        /// Formula: (Passed / Attempts) * 0.4 + (Score / 100) * 0.6
        /// This weights recent accuracy at 40% and current score proficiency at 60%.
        /// </summary>
        public decimal CalculateWeightedAverage(int passed, int attempts, int score)
        {
            if (attempts == 0)
                return 0;

            decimal accuracyWeight = ((decimal)passed / attempts) * 0.4m;
            decimal scoreWeight = ((decimal)score / 100) * 0.6m;

            return Math.Round(accuracyWeight + scoreWeight, 2);
        }

        /// <summary>
        /// Calculates the overall weighted average across all six domains.
        /// </summary>
        public void CalculateOverallWeightedAverage()
        {
            var averages = new[]
            {
                CognitiveReflectionWeightedAverage,
                ReadingComprehensionWeightedAverage,
                CausalReasoningWeightedAverage,
                MetacognitionWeightedAverage,
                ShortTermMemoryWeightedAverage,
                ConfidenceCalibrationWeightedAverage
            };

            OverallWeightedAverage = Math.Round(averages.Average(), 2);
        }
    }
}