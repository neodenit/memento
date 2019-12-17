namespace Neodenit.Memento.Services.API
{
    public interface IValidatorService
    {
        string ErrorMessage { get; }

        bool Validate(string field, string clozeName);
    }
}