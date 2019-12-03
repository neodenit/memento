using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Neodenit.Memento.Common;
using Neodenit.Memento.Interfaces;
using static Neodenit.Memento.Common.ConverterPatterns;

namespace Neodenit.Memento.Services.Converter
{
    public class RawCardOperationService : IRawCardOperationService
    {
        public IEnumerable<string> GetCards(string deckText) =>
           deckText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        public IEnumerable<Tuple<string, string>> RawCardsToClozes(IEnumerable<string> cards) =>
            from card in cards select CardToCloze(card);

        private string GetFirstField(string card) => GetField(card, 0);

        private string GetSecondField(string card) => GetField(card, 1);

        private string GetThirdField(string card) => GetField(card, 2);

        private int CountFields(string card)
        {
            var fields = GetFields(card);
            return fields.Count();
        }

        private bool IsClozeCard(string card)
        {
            var first = GetFirstField(card);
            var isCloze = IsClozeField(first);
            return isCloze;
        }

        private Tuple<string, string> CardToCloze(string card)
        {
            var unescapedCard = TextOperationService.IsInQuotationMarks(card) ? TextOperationService.UnescapeQuotationMarks(card) : card;
            var cardWithoutTags = TextOperationService.TagsToLineBreaks(unescapedCard);
            var trimmedCard = TextOperationService.TrimNewLines(cardWithoutTags);
            var result = ConvertToCloze(trimmedCard);
            return result;
        }

        private Tuple<string, string> ConvertToCloze(string card)
        {
            var isCloze = IsClozeCard(card);

            if (isCloze)
            {
                var cloze = GetFirstField(card);
                var comment = GetSecondField(card);

                return Tuple.Create(cloze, comment);
            }
            else if (CountFields(card) == 3)
            {
                var question = GetFirstField(card);
                var answer = GetSecondField(card);
                var comment = GetThirdField(card);

                var wordsNumber = Helpers.GetWordsCount(answer);

                var correctedAnswer =
                    wordsNumber <= Settings.Default.DefaultValidLength ?
                    $"{{{{c1::{answer}}}}}" :
                    answer;

                var result = question + EmptyLine + correctedAnswer;

                return Tuple.Create(result, comment);
            }
            else
            {
                return Tuple.Create(card, string.Empty);
            }
        }

        private string GetField(string card, int index) =>
            GetFields(card).ElementAtOrDefault(index)?.Trim();

        private IEnumerable<string> GetFields(string card) =>
            card.Split(new[] { RawDelimiter }, StringSplitOptions.None);

        private bool IsClozeField(string text) => Regex.IsMatch(text, ClozePattern);
    }
}
