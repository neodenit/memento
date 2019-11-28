using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Memento.Common;
using static Memento.Core.Converter.ConverterPatterns;

namespace Memento.Core.Converter
{
    static class RawCardOperations
    {
        internal static IEnumerable<string> GetCards(string deckText) =>
           deckText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        internal static IEnumerable<Tuple<string, string>> RawCardsToClozes(IEnumerable<string> cards) =>
            from card in cards select CardToCloze(card);

        internal static string GetFirstField(string card) => GetField(card, 0);

        internal static string GetSecondField(string card) => GetField(card, 1);

        internal static string GetThirdField(string card) => GetField(card, 2);

        internal static int CountFields(string card)
        {
            var fields = GetFields(card);
            return fields.Count();
        }

        internal static bool IsClozeCard(string card)
        {
            var first = GetFirstField(card);
            var isCloze = IsClozeField(first);
            return isCloze;
        }

        private static Tuple<string, string> CardToCloze(string card)
        {
            var unescapedCard = TextOperations.IsInQuotationMarks(card) ? TextOperations.UnescapeQuotationMarks(card) : card;
            var cardWithoutTags = TextOperations.TagsToLineBreaks(unescapedCard);
            var trimmedCard = TextOperations.TrimNewLines(cardWithoutTags);
            var result = ConvertToCloze(trimmedCard);
            return result;
        }

        private static Tuple<string, string> ConvertToCloze(string card)
        {
            var isCloze = RawCardOperations.IsClozeCard(card);

            if (isCloze)
            {
                var cloze = RawCardOperations.GetFirstField(card);
                var comment = RawCardOperations.GetSecondField(card);

                return Tuple.Create(cloze, comment);
            }
            else if (RawCardOperations.CountFields(card) == 3)
            {
                var question = RawCardOperations.GetFirstField(card);
                var answer = RawCardOperations.GetSecondField(card);
                var comment = RawCardOperations.GetThirdField(card);

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

        private static string GetField(string card, int index) =>
            GetFields(card).ElementAtOrDefault(index)?.Trim();

        private static IEnumerable<string> GetFields(string card) =>
            card.Split(new[] { RawDelimiter }, StringSplitOptions.None);

        private static bool IsClozeField(string text) => Regex.IsMatch(text, ClozePattern);
    }
}
