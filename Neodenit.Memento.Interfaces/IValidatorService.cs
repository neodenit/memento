namespace Neodenit.Memento.Interfaces
{
    public interface IValidatorService
    {
        string ErrorMessage { get; }

        bool Validate(string field, string clozeName);
    }
}