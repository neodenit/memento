using System.Collections.Generic;

namespace Neodenit.Memento.Additional
{
    public class AnswerChartData
    {
        public IEnumerable<string> Labels { get; set; }

        public IEnumerable<int> Values { get; set; }
    }
}