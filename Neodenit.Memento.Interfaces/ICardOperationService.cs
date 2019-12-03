namespace Neodenit.Memento.Interfaces
{
    public interface ICardOperationService
    {
        string GetAnswerForCloze(string field, string clozeName);

        string GetCurrentClozePattern(string clozeName);

        string GetQuestionForCloze(string field, string clozeName);
    }
}