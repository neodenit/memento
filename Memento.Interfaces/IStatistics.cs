namespace Memento.Interfaces
{
    public interface IStatistics
    {
        IChartData Answers { get; set; }
        IChartData Cards { get; set; }
        IChartData CorrectAnswers { get; set; }
    }
}