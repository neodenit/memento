using System.Collections.Generic;

namespace Neodenit.Memento.Common.ViewModels
{
    public class AnswerChartViewModel
    {
        public IEnumerable<string> Labels { get; set; }

        public IEnumerable<int> Values { get; set; }
    }
}