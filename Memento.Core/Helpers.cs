using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Memento.Core
{
    public class Helpers
    {
        public static bool CheckPositions(IEnumerable<ICard> cards)
        {
            var n = cards.Count();
            var result = Enumerable.Range(0, n).All(i => cards.Any(card => card.Position == i));

            return result;
        }

        public static IEnumerable<string> GetWords(string text) => Regex.Split(text, @"\W");
        public static int GetWordsNumber(string text) => GetWords(text).Count();
    }
}
