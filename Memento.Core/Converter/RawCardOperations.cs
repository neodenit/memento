using Memento.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Memento.Core.Converter.ConverterPatterns;

namespace Memento.Core.Converter
{
    static class RawCardOperations
    {
        internal static IEnumerable<string> GetCards(string deckText) =>
           deckText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        internal static IEnumerable<string> RawCardsToClozes(IEnumerable<string> cards)
        {
            var clozes = from card in cards select CardToCloze(card);
            var notEmptyClozeCards = from card in clozes where !string.IsNullOrEmpty(card) select card;
            return notEmptyClozeCards;
        }

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

        private static string CardToCloze(string card)
        {
            var unescapedCard = TextOperations.IsInQuotationMarks(card) ? TextOperations.UnescapeQuotationMarks(card) : card;
            var cardWithoutTags = TextOperations.TagsToLineBreaks(unescapedCard);
            var trimmedCard = TextOperations.TrimNewLines(cardWithoutTags);
            var result = ConvertToCloze(trimmedCard);
            return result;
        }

        private static string ConvertToCloze(string card)
        {
            var isCloze = RawCardOperations.IsClozeCard(card);

            if (isCloze)
            {
                var cloze = RawCardOperations.GetFirstField(card);
                var comment = RawCardOperations.GetSecondField(card);

                var result = string.IsNullOrWhiteSpace(comment) ?
                    cloze :
                    cloze + Delimeter + comment;

                return result;
            }
            else if (RawCardOperations.CountFields(card) >= 2)
            {
                var question = RawCardOperations.GetFirstField(card);
                var answer = RawCardOperations.GetSecondField(card);
                var comment = RawCardOperations.GetThirdField(card);

                var wordsNumber = Helpers.GetWordsNumber(answer);

                var correctedAnswer =
                    wordsNumber <= Settings.Default.DefaultValidLength ?
                    $"{{{{c1::{answer}}}}}" :
                    answer;

                var result =
                    string.IsNullOrWhiteSpace(comment) ?
                    question + EmptyLine + correctedAnswer :
                    question + EmptyLine + correctedAnswer + Delimeter + comment;

                return result;
            }
            else
            {
                return TextOperations.ReplaceDelimeters(card);
            }
        }

        private static string GetField(string card, int index) =>
            GetFields(card).ElementAtOrDefault(index)?.Trim();

        private static IEnumerable<string> GetFields(string card) =>
            card.Split(new[] { RawDelimeter }, StringSplitOptions.None);

        private static bool IsClozeField(string text) => Regex.IsMatch(text, ClozePattern);
    }
}
