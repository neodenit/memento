namespace Memento.Interfaces
{
    public interface IStatistics
    {
        IChartData Answers { get; set; }

        IChartData CorrectAnswers { get; set; }

        int NewQuestionCount { get; set; }

        int OldQuestionCount { get; set; }
    }
}