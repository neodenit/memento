using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Memento.Common
{
    public class Helpers
    {
        public static bool CheckPositions(IEnumerable<ICloze> clozes)
        {
            var n = clozes.Count();
            var result = Enumerable.Range(0, n).All(i => clozes.Any(cloze => cloze.Position == i));

            return result;
        }

        public static IEnumerable<string> GetWords(string text) => Regex.Split(text, @"\W");
        public static int GetWordsNumber(string text) => GetWords(text).Count();
    }
}
