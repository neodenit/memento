using System.Collections.Generic;

namespace Neodenit.Memento.Models.ViewModels
{
    public class CorrectAnswerChartViewModel
    {
        public IEnumerable<string> Labels { get; set; }

        public IEnumerable<int> Correct { get; set; }

        public IEnumerable<int> Incorrect { get; set; }
    }
}