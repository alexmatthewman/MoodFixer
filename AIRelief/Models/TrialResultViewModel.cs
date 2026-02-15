namespace AIRelief.Models
{
    public class TrialResultViewModel
    {
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public int Percentage { get; set; }
        public string Message { get; set; }
        public bool ShowTrainingSuggestion { get; set; }
    }

}
