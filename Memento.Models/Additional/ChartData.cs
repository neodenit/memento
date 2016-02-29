using Memento.Interfaces;
using System.Collections.Generic;

namespace Memento.Additional
{
    public class ChartData : IChartData
    {
        public IEnumerable<string> Labels { get; set; }
        public IEnumerable<int> Values { get; set; }

        public ChartData(IEnumerable<string> labels, IEnumerable<int> values)
        {
            Labels = labels;
            Values = values;
        }
    }
}