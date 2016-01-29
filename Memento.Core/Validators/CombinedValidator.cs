namespace Memento.Core.Validators
{
    public class CombinedValidator : IValidator
    {
        private readonly IValidator validator;

        public virtual string ErrorMessage { get; } = "Card is invalid.";

        public CombinedValidator(IConverter converter)
        {
            var existenceValidator = new ExistenceValidator(converter);
            var existenceAndLengthValidator = new LengthValidator(converter, existenceValidator);

            validator = existenceAndLengthValidator;
        }

        public bool Validate(string field, string clozeName)
        {
            return validator.Validate(field, clozeName);
        }
    }
}
