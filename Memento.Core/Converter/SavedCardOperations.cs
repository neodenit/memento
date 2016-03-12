using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Memento.Core.Converter.ConverterPatterns;

namespace Memento.Core.Converter
{
    static class SavedCardOperations
    {
        internal static string GetQuestionForCloze(string field, string clozeName)
        {
            var cloze1 = ReplaceClozeWithSquareBrackets(field, clozeName);
            var cloze2 = StripClozes(cloze1);
            return cloze2;
        }

        internal static string GetAnswerForCloze(string field, string clozeName)
        {
            var currentPattern = GetCurrentClozePattern(clozeName);
            var answer1 = Regex.Replace(field, currentPattern, "[$2]");
            var answer2 = StripClozes(answer1);
            return answer2;
        }

        internal static string GetCurrentClozePattern(string clozeName) =>
            ClozePattern.Replace(LabelPattern, clozeName);

        internal static string GetQuestionPart(string card) => GetPart(card, 0);

        internal static string GetCommentPart(string card) => GetPart(card, 1);

        private static string GetPart(string card, int index) =>
            GetParts(card).ElementAtOrDefault(index)?.Trim();

        private static IEnumerable<string> GetParts(string card) =>
            card.Split(new[] { Delimeter }, StringSplitOptions.None);

        private static string ReplaceClozeWithSquareBrackets(string field, string clozeName)
        {
            var clozePattern = GetCurrentClozePattern(clozeName);
            var match = Regex.Match(field, clozePattern);

            if (!string.IsNullOrEmpty(match.Groups[4].Value))
            {
                var hint = match.Groups[4];
                var result = Regex.Replace(field, clozePattern, $"[{hint}]");
                return result;
            }
            else if (!string.IsNullOrEmpty(match.Groups[2].Value))
            {
                var result = Regex.Replace(field, clozePattern, $"[{Mask}]");
                return result;
            }
            else
            {
                return field;
            }
        }

        private static string StripClozes(string field) =>
            Regex.Replace(field, ClozePattern, "$2");
    }
}
