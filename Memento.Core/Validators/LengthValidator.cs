using Memento.Common;
using Memento.Interfaces;

namespace Memento.Core.Validators
{
    public class LengthValidator : BaseValidator
    {
        protected readonly IConverter converter;

        public override string ErrorMessage { get; } = "There are too many words in the cloze deletion.";

        public LengthValidator(IConverter converter, IValidator baseValidator = null) : base(baseValidator)
        {
            this.converter = converter;
        }

        protected override bool ValidateThis(string field, string clozeName)
        {
            var maxValidLength = Settings.Default.DefaultValidLength;

            var value = converter.GetShortAnswer(field, clozeName);

            var wordsNumber = Helpers.GetWordsCount(value);

            var result = wordsNumber <= maxValidLength;

            return result;
        }
    }
}
