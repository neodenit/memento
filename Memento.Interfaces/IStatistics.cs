namespace Memento.Interfaces
{
    public interface IStatistics
    {
        IChartData Answers { get; set; }
        IChartData CorrectAnswers { get; set; }
        int NewQuestionsCount { get; set; }
        int OldQuestionsCount { get; set; }
    }
}