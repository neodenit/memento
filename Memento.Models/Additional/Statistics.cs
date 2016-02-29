using Memento.Interfaces;

namespace Memento.Additional
{
    public class Statistics : IStatistics
    {
        public IChartData Answers { get; set; }
        public IChartData Cards { get; set; }
        public IChartData CorrectAnswers { get; set; }
    }
}