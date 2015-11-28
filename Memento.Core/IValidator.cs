namespace Memento.Core
{
    public interface IValidator
    {
        bool ValidateBase(string field, string clozeName);
        bool ValidateFull(string field, string clozeName, int? maxLength = null);
        bool ValidateLength(string field, string clozeName, int? maxLength = null);
    }
}