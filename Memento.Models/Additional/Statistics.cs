using Memento.Interfaces;

namespace Memento.Additional
{
    public class Statistics : IStatistics
    {
        public IChartData Answers { get; set; }

        public IChartData CorrectAnswers { get; set; }

        public int NewQuestionCount { get; set; }

        public int OldQuestionCount { get; set; }
    }
}