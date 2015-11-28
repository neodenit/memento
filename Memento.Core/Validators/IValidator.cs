namespace Memento.Core.Validators
{
    public interface IValidator
    {
        bool Validate(string field, string clozeName);
    }
}