using System.Collections.Generic;

namespace Neodenit.Memento.Additional
{
    public class CorrectAnswerChartData
    {
        public IEnumerable<string> Labels { get; set; }

        public IEnumerable<int> Correct { get; set; }

        public IEnumerable<int> Incorrect { get; set; }
    }
}