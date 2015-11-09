using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Memento.Core
{
    public class Converter : IConverter
    {
        private const string ClozePattern = @"{{(\w+)::((?:(?!}}).)+?)(::((?:(?!}}).)+?))?}}";

        public IEnumerable<string> GetCardsFromDeck(string deckText, bool justClozes = false)
        {
            var cards = GetCards(deckText);

            var clozes = RawCardsToClozes(justClozes, cards);

            return clozes;
        }

        public IEnumerable<string> GetClozeNames(string field)
        {
            var clozes = from Match m in Regex.Matches(field, ClozePattern) select m.Groups[1].Value;

            var result = clozes.Distinct();

            return result;
        }

        public IEnumerable<string> GetClozeValues(string field, string clozeName)
        {
            var currentPattern = GetCurrentClozePattern(clozeName);

            var result = from Match m in Regex.Matches(field, currentPattern) select m.Groups[2].Value;

            return result;
        }

        public string GetQuestion(string card, string clozeName, bool stripWildCards = true)
        {
            var text = stripWildCards ? ReplaceAllWildCardsWithText(card) : card;

            var questionPart = GetQuestionPart(text);

            var question = GetQuestionFromField(questionPart, clozeName);

            var result = LineBreaksToTags(question);

            return result;
        }

        public string GetAnswerValue(string field, string clozeName)
        {
            var currentPattern = GetCurrentClozePattern(clozeName);

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

        public string GetAnswer(string card, string clozeName, bool stripWildCards = true)
        {
            var text = stripWildCards ? ReplaceAllWildCardsWithText(card) : card;

            var answer = GetAnswerFromField(text, clozeName);

            var answerWithDelimeter = DelimiterToTag(answer);

            var result = LineBreaksToTags(answerWithDelimeter);

            return result;
        }

        public string GetCurrentClozePattern(string clozeName)
        {
            var currentPattern = "{{(" + clozeName + ")::((?:(?!}}).)+?)(::((?:(?!}}).)+?))?}}";
            return currentPattern;
        }

        public string ReplaceAnswer(string text, string label, string newAnswers)
        {
            var currentPattern = GetCurrentClozePattern(label);

            var newPattern = "{{" + label + "::" + newAnswers + "$3}}";

            return Regex.Replace(text, currentPattern, newPattern);
        }

        public string ReplaceTextWithWildcards(string text, string label)
        {
            var currentPattern = GetCurrentClozePattern(label);

            var newPattern = "{{" + label + "::*}}";

            var regex = new Regex(currentPattern);

            var firstMatch = regex.Match(text);

            if (firstMatch.Success)
            {
                return regex.Replace(text, newPattern, -1, firstMatch.Index + firstMatch.Length);
            }
            else
            {
                return text;
            }
        }

        public string ReplaceTextWithWildcards(string text, IEnumerable<string> labels)
        {
            if (labels.Any())
            {
                var newText = ReplaceTextWithWildcards(text, labels.First());

                return ReplaceTextWithWildcards(newText, labels.Skip(1));
            }
            else
            {
                return text;
            }
        }

        public string ReplaceAllWildCardsWithText(string text)
        {
            var labels = GetClozeNames(text);

            return ReplaceAllWildCardsWithText(text, labels);
        }

        public string ReplaceAllWildCardsWithText(string text, IEnumerable<string> labels)
        {
            if (labels.Any())
            {
                var newText = ReplaceWildCardsWithText(text, labels.First());

                return ReplaceAllWildCardsWithText(newText, labels.Skip(1));
            }
            else
            {
                return text;
            }
        }

        public string ReplaceWildCardsWithText(string text, string label)
        {
            var currentPattern = GetCurrentClozePattern(label);

            var regex = new Regex(currentPattern);

            var firstMatch = regex.Match(text);

            var result = regex.Replace(text, firstMatch.Value);

            return result;
        }

        public string FormatForExport(string text)
        {
            var text2 = DelimiterToTab(text);

            var text3 = LineBreaksToTagsAlt(text2);

            var result = text3.Contains("\t") ? text3 : text3 + "\t";

            return result;
        }

        private static bool IsInQuotationMarks(string text)
        {
            return text.StartsWith("\"");
        }

        private static string UnescapeQuotationMarks(string text)
        {
            var substitution = "&quot;";
            var doubleMark = "\"{2}";
            var singleMark = "\"";

            var text2 = Regex.Replace(text, doubleMark, substitution);

            var text3 = Regex.Replace(text2, singleMark, string.Empty);

            var text4 = Regex.Replace(text3, substitution, singleMark);

            return text4;
        }

        private static IEnumerable<string> GetCards(string deckText)
        {
            return deckText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        private static IEnumerable<string> RawCardsToClozes(bool justClozes, IEnumerable<string> cards)
        {
            var clozes = from card in cards select CardToCloze(card, justClozes);

            var notEmptyClozeCards = from card in clozes where !string.IsNullOrEmpty(card) select card;

            return notEmptyClozeCards;
        }

        private static string CardToCloze(string card, bool justClozes)
        {
            var unescapedCard = IsInQuotationMarks(card) ? UnescapeQuotationMarks(card) : card;

            var cardWithoutTags = TagsToLineBreaks(unescapedCard);

            var trimmedCard = TrimNewLines(cardWithoutTags);

            var result = ConvertToCloze(trimmedCard, justClozes);

            return result;
        }

        private static string ConvertToCloze(string card, bool justClozes)
        {
            var isCloze = IsClozeCard(card);

            if (isCloze)
            {
                var cloze = GetFirstField(card);
                var comment = GetSecondField(card);

                var result = string.IsNullOrWhiteSpace(comment) ?
                    cloze.Trim() :
                    string.Format("{0}{1}{1}{2}{1}{1}{3}", cloze.Trim(), Environment.NewLine, Settings.Default.CommentDelimiter, comment.Trim());

                return result;
            }
            else if (!justClozes)
            {
                var fieldsCount = CountFields(card);

                if (fieldsCount >= 2)
                {
                    var question = GetFirstField(card);
                    var answer = GetSecondField(card);
                    var comment = GetThirdField(card);

                    var clozedAnswer = "{{c1::" + answer + "}}";

                    var result =
                        string.IsNullOrWhiteSpace(comment) ?
                        string.Format("{0}{1}{1}{2}", question.Trim(), Environment.NewLine, clozedAnswer.Trim()) :
                        string.Format("{0}{1}{1}{2}{1}{1}{3}{1}{1}{4}", question.Trim(), Environment.NewLine, clozedAnswer.Trim(), Settings.Default.CommentDelimiter, comment.Trim());

                    return result;
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        private static string GetQuestionPart(string card)
        {
            var part = GetParts(card).ElementAt(0).Trim();

            return part;
        }

        private static string GetCommentPart(string card)
        {
            var part = GetParts(card).ElementAt(1).Trim();

            return part;
        }

        private static string GetFirstField(string card)
        {
            var field = GetFields(card).ElementAt(0);

            return field;
        }

        private static string GetSecondField(string card)
        {
            var field = GetFields(card).ElementAt(1);

            return field;
        }

        private static string GetThirdField(string card)
        {
            var field = GetFields(card).ElementAt(2);

            return field;
        }

        private static string TagsToLineBreaks(string text)
        {
            var result1 = Regex.Replace(text, "<br />", string.Empty);
            var result2 = Regex.Replace(result1, "<div>", Environment.NewLine);
            var result3 = Regex.Replace(result2, "</div>", string.Empty);

            return result3;
        }

        private static string LineBreaksToTags(string text)
        {
            return Regex.Replace(text, Environment.NewLine, "<br />");
        }

        private static string LineBreaksToTagsAlt(string text)
        {
            return Regex.Replace(text, Environment.NewLine, "<div></div>");
        }

        private static string DelimiterToTag(string text)
        {
            var delimiter = Environment.NewLine + Settings.Default.CommentDelimiter + Environment.NewLine;
            return Regex.Replace(text, delimiter, "<hr />");
        }

        private static string DelimiterToTab(string text)
        {
            var delimiter = Environment.NewLine + Settings.Default.CommentDelimiter + Environment.NewLine;
            return Regex.Replace(text, delimiter, "\t");
        }

        private static string TrimNewLines(string text)
        {
            var result = text.Trim(Environment.NewLine.ToCharArray());

            return result;
        }

        private static string RestictNewLines(string text)
        {
            var pattern = string.Join(string.Empty, Enumerable.Repeat(Environment.NewLine, 3));
            var replacement = string.Join(string.Empty, Enumerable.Repeat(Environment.NewLine, 2));

            var result = Regex.Replace(text, pattern, replacement);

            return result;
        }

        private static bool IsClozeCard(string card)
        {
            var first = GetFirstField(card);

            var isCloze = IsClozeField(first);

            return isCloze;
        }

        private static bool IsClozeField(string text)
        {
            var result = Regex.IsMatch(text, ClozePattern);

            return result;
        }

        private IEnumerable<string> GetClozesFromCard(string card)
        {
            var isCloze = IsClozeCard(card);

            if (isCloze)
            {
                var first = GetFirstField(card);

                var clozes = GetClozePartsFromField(first);

                foreach (var cloze in clozes)
                {
                    yield return cloze;
                }
            }
        }

        private static IEnumerable<string> GetParts(string card)
        {
            var delimiter = Environment.NewLine + Settings.Default.CommentDelimiter + Environment.NewLine;

            var parts = card.Split(new string[] { delimiter }, StringSplitOptions.None);

            return parts;
        }

        private static IEnumerable<string> GetFields(string card)
        {
            var fields = card.Split('\t');

            return fields;
        }

        private static int CountFields(string card)
        {
            var fields = GetFields(card);

            return fields.Count();
        }

        private IEnumerable<string> GetClozePartsFromField(string field)
        {
            var cs = GetClozeNames(field);

            foreach (var c in cs)
            {
                var cloze = GetQuestionFromField(field, c);

                yield return cloze;
            }
        }

        private string GetQuestionFromField(string field, string clozeName)
        {
            var cloze1 = ReplaceClozeWithSquareBrackets(field, clozeName);

            var cloze2 = StripClozes(cloze1);

            return cloze2;
        }

        private static string StripClozes(string field)
        {
            var result = Regex.Replace(field, ClozePattern, "$2");
            return result;
        }

        private string ReplaceClozeWithSquareBrackets(string field, string clozeName)
        {
            var clozePattern = GetCurrentClozePattern(clozeName);

            var match = Regex.Match(field, clozePattern);

            if (!string.IsNullOrEmpty(match.Groups[4].Value))
            {
                var hint = match.Groups[4];

                var result = Regex.Replace(field, clozePattern, string.Format("[{0}]", hint));

                return result;
            }
            else if (!string.IsNullOrEmpty(match.Groups[2].Value))
            {
                var result = Regex.Replace(field, clozePattern, "[...]");

                return result;
            }
            else
            {
                return field;
            }
        }

        private string GetAnswerFromField(string field, string clozeName)
        {
            var currentPattern = GetCurrentClozePattern(clozeName);

            var cloze1 = Regex.Replace(field, currentPattern, "[$2]");

            var cloze2 = StripClozes(cloze1);

            return cloze2;
        }
    }
}
