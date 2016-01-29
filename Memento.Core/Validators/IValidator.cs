namespace Memento.Core.Validators
{
    public interface IValidator
    {
        string ErrorMessage { get; }

        bool Validate(string field, string clozeName);
    }
}