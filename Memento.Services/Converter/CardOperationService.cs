using System.Text.RegularExpressions;
using Memento.Interfaces;
using static Memento.Common.ConverterPatterns;

namespace Memento.Services.Converter
{
    public class CardOperationService : ICardOperationService
    {
        public string GetQuestionForCloze(string field, string clozeName)
        {
            var cloze1 = ReplaceClozeWithSquareBrackets(field, clozeName);
            var cloze2 = StripClozes(cloze1);
            return cloze2;
        }

        public string GetAnswerForCloze(string field, string clozeName)
        {
            var currentPattern = GetCurrentClozePattern(clozeName);
            var answer1 = Regex.Replace(field, currentPattern, "[$2]");
            var answer2 = StripClozes(answer1);
            return answer2;
        }

        public string GetCurrentClozePattern(string clozeName) =>
            ClozePattern.Replace(LabelPattern, clozeName);

        private string ReplaceClozeWithSquareBrackets(string field, string clozeName)
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

        private string StripClozes(string field) =>
            Regex.Replace(field, ClozePattern, "$2");
    }
}
