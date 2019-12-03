using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Neodenit.Memento.Common
{
    public class Helpers
    {
        public static IEnumerable<string> GetWords(string text) => Regex.Split(text, @"\W");

        public static int GetWordsCount(string text) => GetWords(text).Count();
    }
}
