using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Memento.Interfaces;

namespace Memento.Common
{
    public class Helpers
    {
        public static bool ArePositionsValid(IEnumerable<IUserRepetition> repetitions)
        {
            var repetitionPositions = from r in repetitions orderby r.Position select r.Position;
            var n = repetitions.Count();
            var result = repetitionPositions.SequenceEqual(Enumerable.Range(0, n));

            return result;
        }

        public static IEnumerable<string> GetWords(string text) => Regex.Split(text, @"\W");

        public static int GetWordsCount(string text) => GetWords(text).Count();
    }
}
