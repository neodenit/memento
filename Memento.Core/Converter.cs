using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Memento.Core
{
    public static class Converter
    {
        private const string ClozePattern = @"{{(\w+)::((?:(?!}}).)+?)(::((?:(?!}}).)+?))?}}";

        public static IEnumerable<string> GetCardsFromDeck(string deckText, bool justClozes = false)
        {
            var cards = deckText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            var cardsWithoutTags = from card in cards select StripTagsAlt(card);

            var clozeCards = from card in cardsWithoutTags select ConvertToCloze(card, justClozes);

            var notEmptyClozeCards = from card in clozeCards where !string.IsNullOrEmpty(card) select card;

            return notEmptyClozeCards;
        }

        public static IEnumerable<string> GetClozeNames(string field)
        {
            var clozes = from Match m in Regex.Matches(field, ClozePattern) select m.Groups[1].Value;

            var result = clozes.Distinct();

            return result;
        }

        public static IEnumerable<string> GetClozeValues(string field, string clozeName)
        {
            var currentPattern = GetCurrentClozePattern(clozeName);

            var result = from Match m in Regex.Matches(field, currentPattern) select m.Groups[2].Value;

            return result;
        }

        public static string GetQuestion(string card, string clozeName)
        {
            var first = GetFirstField(card);

            var result = GetQuestionFromField(first, clozeName);

            return result;
        }

        public static string GetAnswerValue(string field, string clozeName)
        {
            var currentPattern = GetCurrentClozePattern(clozeName);

            if (Regex.IsMatch(field, currentPattern))
            {
                var result = Regex.Match(field, currentPattern).Groups[2].Value;

                return result;
            }
            else
            {
                throw new Exception("Cloze doesn't found.");
            }
        }

        public static string GetAnswer(string field, string clozeName)
        {
            var result = GetAnswerFromField(field, clozeName);

            return result;
        }

        public static string GetCurrentClozePattern(string clozeName)
        {
            var currentPattern = "{{(" + clozeName + ")::((?:(?!}}).)+?)(::((?:(?!}}).)+?))?}}";
            return currentPattern;
        }

        public static string ReplaceAnswer(string text, string label, string newAnswers)
        {
            var currentPattern = GetCurrentClozePattern(label);

            var newPattern = "{{" + label + "::" + newAnswers + "$3}}";

            return Regex.Replace(text, currentPattern, newPattern);
        }

        public static string ReplaceWithWildcards(string text, string label)
        {
            var currentPattern = GetCurrentClozePattern(label);

            var newPattern = "{{" + label + "::*}}";
            
            var regex = new Regex(currentPattern);

            var firstMatch = regex.Match(text);

            if (firstMatch.Success)
            {
                return regex.Replace(text, newPattern, -1, firstMatch.Index + firstMatch.Length);
            }
            else
            {
                return text;
            }
        }

        public static string ReplaceAllWithWildCards(string text, IEnumerable<string> labels)
        {
            if (labels.Any())
            {
                var newText = ReplaceWithWildcards(text, labels.First());
                
                return ReplaceAllWithWildCards(newText, labels.Skip(1));
            }
            else
            {
                return text;
            }
        }

        private static string ConvertToCloze(string card, bool justClozes)
        {
            var isCloze = IsClozeCard(card);

            if (isCloze)
            {
                return GetFirstField(card);
            }
            else if (!justClozes)
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
            else
            {
                return string.Empty;
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
            var result2 = Regex.Replace(result1, "<div>(.+?)</div>", "$1\r\n");

            return result2;
        }

        private static string StripTagsAlt(string text)
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
                var cloze = GetQuestionFromField(field, c);

                yield return cloze;
            }
        }

        private static string GetQuestionFromField(string field, string clozeName)
        {
            var cloze1 = ReplaceCloze(field, clozeName);

            var cloze2 = StripClozes(cloze1);

            return cloze2;
        }

        private static string StripClozes(string field)
        {
            var result = Regex.Replace(field, ClozePattern, "$2");
            return result;
        }

        private static string ReplaceCloze(string field, string clozeName)
        {
            var clozePattern = GetCurrentClozePattern(clozeName);

            var match = Regex.Match(field, clozePattern);

            if (!string.IsNullOrEmpty(match.Groups[4].Value))
            {
                var hint = match.Groups[4];

                var result = Regex.Replace(field, clozePattern, string.Format("[{0}]", hint));

                return result;
            }
            else if (!string.IsNullOrEmpty(match.Groups[2].Value))
            {
                var result = Regex.Replace(field, clozePattern, "[...]");

                return result;
            }
            else
            {
                return field;
            }
        }

        private static string GetAnswerFromField(string field, string clozeName)
        {
            var currentPattern = GetCurrentClozePattern(clozeName);

            var cloze1 = Regex.Replace(field, currentPattern, "[$2]");

            var cloze2 = StripClozes(cloze1);

            return cloze2;
        }
    }
}
