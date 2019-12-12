namespace Neodenit.Memento.Models.ViewModels
{
    public class StatisticsViewModel
    {
        public AnswerChartViewModel Answers { get; set; }

        public CorrectAnswerChartViewModel CorrectAnswers { get; set; }

        public int NewQuestionCount { get; set; }

        public int OldQuestionCount { get; set; }
    }
}