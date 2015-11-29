using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Memento.Core.Validators
{
    public class LengthValidator : BaseValidator
    {
        protected readonly IConverter converter;

        public LengthValidator(IConverter converter, IValidator baseValidator = null) : base(baseValidator)
        {
            this.converter = converter;
        }

        protected override bool ValidateThis(string field, string clozeName)
        {
            var maxValidLength = Settings.Default.DefaultValidLength;

            var value = converter.GetAnswerValue(field, clozeName);

            var words = Regex.Split(value, @"\W");

            var wordsNumber = words.Length;

            var result = wordsNumber <= maxValidLength;

            return result;
        }
    }
}
