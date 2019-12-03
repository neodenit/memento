using Neodenit.Memento.Interfaces;

namespace Neodenit.Memento.Services.Validators
{
    public class CombinedValidatorService : IValidatorService
    {
        private readonly IValidatorService validator;

        public virtual string ErrorMessage { get; } = "Card is invalid.";

        public CombinedValidatorService(IConverterService converter)
        {
            var existenceValidator = new ExistenceValidatorService(converter);
            var existenceAndLengthValidator = new LengthValidatorService(converter, existenceValidator);

            validator = existenceAndLengthValidator;
        }

        public bool Validate(string field, string clozeName)
        {
            return validator.Validate(field, clozeName);
        }
    }
}
