using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Memento.Core.Validators
{
    public class ExistenceValidator : IValidator
    {
        private readonly IConverter converter;

        public ExistenceValidator(IConverter converter)
        {
            this.converter = converter;
        }

        public bool Validate(string field, string clozeName)
        {
            var currentPattern = converter.GetCurrentClozePattern(clozeName);

            var firstValue = Regex.Match(field, currentPattern).Groups[2].Value;

            var values = from Match m in Regex.Matches(field, currentPattern) select m.Groups[2].Value;

            var result = values.All(item => item == firstValue || item == "*");

            return result;
        }
    }
}
