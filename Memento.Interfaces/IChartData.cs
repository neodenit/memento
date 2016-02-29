using System.Collections.Generic;

namespace Memento.Interfaces
{
    public interface IChartData
    {
        IEnumerable<string> Labels { get; set; }
        IEnumerable<int> Values { get; set; }
    }
}