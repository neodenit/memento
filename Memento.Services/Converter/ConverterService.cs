using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Memento.Interfaces;
using static Memento.Common.ConverterPatterns;

namespace Memento.Services.Converter
{
    public class ConverterService : IConverterService
    {
        private readonly ICardOperationService cardOperationService;
        private readonly IRawCardOperationService rawCardOperationService;

        public ConverterService(ICardOperationService cardOperationService, IRawCardOperationService rawCardOperationService)
        {
            this.cardOperationService = cardOperationService ?? throw new ArgumentNullException(nameof(cardOperationService));
            this.rawCardOperationService = rawCardOperationService ?? throw new ArgumentNullException(nameof(rawCardOperationService));
        }

        public IEnumerable<Tuple<string, string>> GetCardsFromDeck(string deckText)
        {
            var cards = rawCardOperationService.GetCards(deckText);
            var clozes = rawCardOperationService.RawCardsToClozes(cards);
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
            var result = cardOperationService.GetQuestionForCloze(cardText, clozeName);
            return result;
        }

        public string GetShortAnswer(string field, string clozeName)
        {
            var currentPattern = cardOperationService.GetCurrentClozePattern(clozeName);

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
            var result = cardOperationService.GetAnswerForCloze(card, clozeName);
            return result;
        }

        public string ReplaceAnswer(string text, string label, string newAnswers)
        {
            var currentPattern = cardOperationService.GetCurrentClozePattern(label);
            var newPattern = $"{{{{{label}::{newAnswers}$3}}}}";
            return Regex.Replace(text, currentPattern, newPattern);
        }

        public string AddAltAnswer(string text, string label, string newAnswer)
        {
            var oldAnswers = GetShortAnswer(text, label);
            var newAnswers = $"{oldAnswers}|{newAnswer}";
            return ReplaceAnswer(text, label, newAnswers);
        }

        public string FormatForExport(string text, string comment)
        {
            var textWithoutNewLines = TextOperationService.LineBreaksToTags(text);
            var exportText = TextOperationService.TabsToSpaces(textWithoutNewLines);

            var notNullComment = comment ?? string.Empty;
            var commentWithoutNewLines = TextOperationService.LineBreaksToTags(notNullComment);
            var exportComment = TextOperationService.TabsToSpaces(commentWithoutNewLines);

            var result = exportText + RawDelimiter + exportComment;
            return result;
        }

        public string GetCurrentClozePattern(string clozeName) =>
            cardOperationService.GetCurrentClozePattern(clozeName);
    }
}
