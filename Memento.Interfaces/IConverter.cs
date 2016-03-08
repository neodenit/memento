using System.Collections.Generic;

namespace Memento.Interfaces
{
    public interface IConverter
    {
        string GetQuestion(string card, string clozeName);
        string GetFullAnswer(string card, string clozeName);
        string GetShortAnswer(string field, string clozeName);
        string GetComment(string text);

        IEnumerable<string> GetCardsFromDeck(string deckText);
        IEnumerable<string> GetClozeNames(string field);

        string ReplaceAnswer(string text, string label, string newAnswers);
        string AddAltAnswer(string text, string label, string newAnswer);

        string FormatForExport(string text);

        string GetCurrentClozePattern(string clozeName);
    }
}