using System.Linq;
using System.Text.RegularExpressions;
using Neodenit.Memento.Services.API;

namespace Neodenit.Memento.Services.Validators
{
    public class ExistenceValidatorService : BaseValidatorService
    {
        protected readonly IConverterService converter;

        public override string ErrorMessage { get; } = "Please add cloze deletions.";

        public ExistenceValidatorService(IConverterService converter)
        {
            this.converter = converter;
        }

        protected override bool ValidateThis(string field, string clozeName)
        {
            var currentPattern = converter.GetCurrentClozePattern(clozeName);

            var firstValue = Regex.Match(field, currentPattern).Groups[2].Value;

            var values = from Match m in Regex.Matches(field, currentPattern) select m.Groups[2].Value;

            var result = values.All(item => item == firstValue);

            return result;
        }
    }
}
