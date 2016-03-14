using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Memento.Core.Converter.ConverterPatterns;

namespace Memento.Core.Converter
{
    class TextOperations
    {
        internal static string ReplaceDelimeters(string card) => card.Replace(RawDelimeter, Delimeter);

        internal static string TagsToLineBreaks(string text)
        {
            var result1 = Regex.Replace(text, "<br />", string.Empty);
            var result2 = Regex.Replace(result1, "<div>", Environment.NewLine);
            var result3 = Regex.Replace(result2, "</div>", string.Empty);
            return result3;
        }

        internal static string LineBreaksToTagsAlt(string text) =>
            Regex.Replace(text, Environment.NewLine, AltLineBreakTag);

        internal static string DelimeterToRaw(string text) =>
            Regex.Replace(text, Delimeter, RawDelimeter);

        internal static string TabsToSpaces(string text) =>
            Regex.Replace(text, Tab, TabReplacement);

        internal static string TrimNewLines(string text) =>
            text.Trim(Environment.NewLine.ToCharArray());

        internal static bool IsInQuotationMarks(string text) => text.StartsWith("\"");

        internal static string UnescapeQuotationMarks(string text)
        {
            var substitution = "&quot;";
            var doubleMark = "\"{2}";
            var singleMark = "\"";

            var text2 = Regex.Replace(text, doubleMark, substitution);
            var text3 = Regex.Replace(text2, singleMark, string.Empty);
            var text4 = Regex.Replace(text3, substitution, singleMark);
            return text4;
        }

        internal static string AddEndingDelimeter(string text) =>
            text.Contains(RawDelimeter) ? text : text + RawDelimeter;

        private static string LineBreaksToTags(string text) =>
            Regex.Replace(text, Environment.NewLine, LineBreakTag);

        private static string DelimeterToTag(string text) =>
            Regex.Replace(text, Delimeter, DelimeterTag);

        private static string RestictNewLines(string text)
        {
            var pattern = string.Join(string.Empty, Enumerable.Repeat(Environment.NewLine, 3));
            var replacement = string.Join(string.Empty, Enumerable.Repeat(Environment.NewLine, 2));
            var result = Regex.Replace(text, pattern, replacement);
            return result;
        }
    }
}
