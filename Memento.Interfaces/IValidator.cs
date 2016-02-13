namespace Memento.Interfaces
{
    public interface IValidator
    {
        string ErrorMessage { get; }

        bool Validate(string field, string clozeName);
    }
}