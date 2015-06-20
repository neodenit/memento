using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Memento.SRS
{
    public static class DeckConverter
    {
        private const string ClozePattern = @"{{(\w+)::(.+?)(::(.+?))?}}";

        public static IEnumerable<string> GetCardsFromDeck(string deckText, bool justClozes = false)
        {
            var cards = deckText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            var cleanCards = cards.Select(item => DeckConverter.StripTags2(item));

            var clozeCards = justClozes ?
                cleanCards.Where(item => IsClozeCard(item)).Select(item => GetFirstField(item)) :
                cleanCards.Select(item => DeckConverter.ConvertToCloze(item));

            var notEmptyClozeCards = clozeCards.Where(item => !string.IsNullOrEmpty(item));

            return notEmptyClozeCards;
        }

        public static IEnumerable<string> GetClozeNames(string field)
        {
            var cs = (from Match m in Regex.Matches(field, ClozePattern) select m.Groups[1].Value).Distinct();

            return cs;
        }

        public static string GetClozeByName(string card, string clozeName)
        {
            var first = GetFirstField(card);

            var result = GetClozeFromField(first, clozeName);

            return result;
        }

        private static string ConvertToCloze(string card)
        {
            var isCloze = IsClozeCard(card);

            if (isCloze)
            {
                return GetFirstField(card);
            }
            else
            {
                var fields = GetFields(card);

                if (fields.Count() >= 2)
                {
                    var question = fields.First();
                    var answer = fields.Skip(1).First();

                    var clozedAnswer = "{{c1::" + answer + "}}";

                    var result = string.Format("{0}{1}{1}{2}", question, Environment.NewLine, clozedAnswer);

                    return result;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private static string GetFirstField(string card)
        {
            var firstField = GetFields(card).First();

            return firstField;
        }

        private static string StripTags(string text)
        {
            var result1 = Regex.Replace(text, "<br />", Environment.NewLine);
            var result2 = Regex.Replace(text, "<div>(.+?)</div>", "$1\r\n");

            return result2;
        }

        private static string StripTags2(string text)
        {
            var result1 = Regex.Replace(text, "<br />", Environment.NewLine);
            var result2 = Regex.Replace(result1, "<div>", string.Empty);
            var result3 = Regex.Replace(result2, "</div>", Environment.NewLine);

            var trimmed = result3.Trim();

            return trimmed;
        }

        private static bool IsClozeCard(string card)
        {
            var first = GetFirstField(card);

            var isCloze = IsClozeField(first);

            return isCloze;
        }

        private static bool IsClozeField(string text)
        {
            var result = Regex.IsMatch(text, ClozePattern);

            return result;
        }

        private static IEnumerable<string> GetClozesFromCard(string card)
        {
            var isCloze = IsClozeCard(card);

            if (isCloze)
            {
                var first = GetFirstField(card);

                var clozes = GetClozePartsFromField(first);

                foreach (var cloze in clozes)
                {
                    yield return cloze;
                }
            }
        }

        private static string[] GetFields(string card)
        {
            var fields = card.Split('\t');

            return fields;
        }

        private static IEnumerable<string> GetClozePartsFromField(string field)
        {
            var cs = GetClozeNames(field);

            foreach (var c in cs)
            {
                var cloze = GetClozeFromField(field, c);

                yield return cloze;
            }
        }

        private static string GetClozeFromField(string field, string clozeName)
        {
            var currentPattern = "{{(" + clozeName + ")::(.+?)(::(.+?))?}}";

            var escapedPattern = @"{ {cloze: :(.+?)} }";

            var cloze1 = Regex.Replace(field, currentPattern, @"{ {cloze: :$2} }");

            var cloze2 = Regex.Replace(cloze1, ClozePattern, "$2");

            var cloze3 = Regex.Replace(cloze2, escapedPattern, "{{cloze::$1}}");

            return cloze3;
        }
    }
}
