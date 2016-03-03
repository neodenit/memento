using System.Collections.Generic;

namespace Memento.Interfaces
{
    public interface IConverter
    {
        string FormatForExport(string text);
        string GetAnswer(string card, string clozeName, bool stripWildCards = true);
        string GetAnswerValue(string field, string clozeName);
        IEnumerable<string> GetCardsFromDeck(string deckText);
        IEnumerable<string> GetClozeNames(string field);
        IEnumerable<string> GetClozeValues(string field, string clozeName);
        string GetCurrentClozePattern(string clozeName);
        string GetQuestion(string card, string clozeName, bool stripWildCards = true);
        string ReplaceAllWildCardsWithText(string text);
        string ReplaceAllWildCardsWithText(string text, IEnumerable<string> labels);
        string ReplaceAnswer(string text, string label, string newAnswers);
        string AddAltAnswer(string text, string label, string newAnswer);
        string ReplaceTextWithWildcards(string text, string label);
        string ReplaceTextWithWildcards(string text, IEnumerable<string> labels);
        string ReplaceWildCardsWithText(string text, string label);
    }
}