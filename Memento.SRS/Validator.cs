using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Memento.SRS
{
    public static class Validator
    {
        public static bool ValidateFull(string field, string clozeName)
        {
            return ValidateBase(field, clozeName) && ValidateLength(field, clozeName);
        }

        public static bool ValidateBase(string field, string clozeName)
        {
            var currentPattern = Converter.GetCurrentClozePattern(clozeName);

            var firstValue = Regex.Match(field, currentPattern).Groups[2].Value;

            var values = from Match m in Regex.Matches(field, currentPattern) select m.Groups[2].Value;

            var result = values.All(item => item == firstValue || item == "*");

            return result;
        }

        public static bool ValidateLength(string field, string clozeName, int maxLength = 3)
        {
            var value = Converter.GetAnswerValue(field, clozeName);

            var words = Regex.Split(value, @"\W");

            var wordsNumber = words.Length;

            var result = wordsNumber <= maxLength;

            return result;
        }
    }
}
