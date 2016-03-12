using Memento.Common;
using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Memento.Core.Converter.ConverterPatterns;

namespace Memento.Core.Converter
{
    public class Converter : IConverter
    {
        public IEnumerable<string> GetCardsFromDeck(string deckText)
        {
            var cards = RawCardOperations.GetCards(deckText);
            var clozes = RawCardOperations.RawCardsToClozes(cards);
            return clozes;
        }

        public IEnumerable<string> GetClozeNames(string field)
        {
            var clozes = from Match m in Regex.Matches(field, ClozePattern) select m.Groups[1].Value;
            var result = clozes.Distinct();
            return result;
        }

        public string GetQuestion(string cardText, string clozeName)
        {
            var questionPart = SavedCardOperations.GetQuestionPart(cardText);
            var result = SavedCardOperations.GetQuestionForCloze(questionPart, clozeName);
            return result;
        }

        public string GetShortAnswer(string field, string clozeName)
        {
            var currentPattern = SavedCardOperations.GetCurrentClozePattern(clozeName);

            if (Regex.IsMatch(field, currentPattern))
            {
                var result = Regex.Match(field, currentPattern).Groups[2].Value;
                return result;
            }
            else
            {
                throw new Exception("Cloze doesn't found.");
            }
        }

        public string GetFullAnswer(string card, string clozeName)
        {
            var questionText = SavedCardOperations.GetQuestionPart(card);
            var result = SavedCardOperations.GetAnswerForCloze(questionText, clozeName);
            return result;
        }

        public string GetComment(string cardText) =>
            SavedCardOperations.GetCommentPart(cardText);

        public string ReplaceAnswer(string text, string label, string newAnswers)
        {
            var currentPattern = SavedCardOperations.GetCurrentClozePattern(label);
            var newPattern = $"{{{{{label}::{newAnswers}$3}}}}";
            return Regex.Replace(text, currentPattern, newPattern);
        }

        public string AddAltAnswer(string text, string label, string newAnswer)
        {
            var oldAnswers = GetShortAnswer(text, label);
            var newAnswers = $"{oldAnswers}|{newAnswer}";
            return ReplaceAnswer(text, label, newAnswers);
        }

        public string FormatForExport(string text)
        {
            var text2 = TextOperations.DelimeterToRaw(text);
            var text3 = TextOperations.LineBreaksToTagsAlt(text2);
            var result = TextOperations.AddEndingDelimeter(text3);
            return result;
        }

        public string GetCurrentClozePattern(string clozeName) =>
            SavedCardOperations.GetCurrentClozePattern(clozeName);
    }
}
