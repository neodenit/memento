using Neodenit.Memento.Interfaces;

namespace Neodenit.Memento.Services.Validators
{
    public abstract class BaseValidatorService : IValidatorService
    {
        private readonly IValidatorService baseValidator;

        public BaseValidatorService(IValidatorService baseValidator = null)
        {
            this.baseValidator = baseValidator;
        }

        public virtual string ErrorMessage { get; } = "Card is invalid.";

        public bool Validate(string field, string clozeName)
        {
            return ValidateBase(field, clozeName) && ValidateThis(field, clozeName);
        }

        protected abstract bool ValidateThis(string field, string clozeName);

        private bool ValidateBase(string field, string clozeName)
        {
            return baseValidator == null || baseValidator.Validate(field, clozeName);
        }
    }
}
