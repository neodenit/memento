namespace Memento.Core.Validators
{
    public class CombinedValidator : IValidator
    {
        private readonly IConverter converter;

        public CombinedValidator(IConverter converter)
        {
            this.converter = converter;
        }

        public bool Validate(string field, string clozeName)
        {
            return new ExistenceValidator(converter).Validate(field, clozeName) && new LengthValidator(converter).Validate(field, clozeName);
        }
    }
}
