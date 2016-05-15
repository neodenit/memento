using Memento.Interfaces;

namespace Memento.Additional
{
    public class Statistics : IStatistics
    {
        public IChartData Answers { get; set; }
        public IChartData CorrectAnswers { get; set; }
        public int NewQuestionsCount { get; set; }
        public int OldQuestionsCount { get; set; }
    }
}