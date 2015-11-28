using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Memento.Core
{
    public class Validator : IValidator
    {
        private readonly IConverter converter;

        public Validator(IConverter converter)
        {
            this.converter = converter;
        }

        public bool ValidateFull(string field, string clozeName, int? maxLength = null)
        {
            return ValidateBase(field, clozeName) && ValidateLength(field, clozeName, maxLength);
        }

        public bool ValidateBase(string field, string clozeName)
        {
            var currentPattern = converter.GetCurrentClozePattern(clozeName);

            var firstValue = Regex.Match(field, currentPattern).Groups[2].Value;

            var values = from Match m in Regex.Matches(field, currentPattern) select m.Groups[2].Value;

            var result = values.All(item => item == firstValue || item == "*");

            return result;
        }

        public bool ValidateLength(string field, string clozeName, int? maxLength = null)
        {
            var maxValidLength = maxLength ?? Settings.Default.DefaultValidLength;

            var value = converter.GetAnswerValue(field, clozeName);

            var words = Regex.Split(value, @"\W");

            var wordsNumber = words.Length;

            var result = wordsNumber <= maxValidLength;

            return result;
        }
    }
}
