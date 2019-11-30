using System;
using System.Text.RegularExpressions;
using static Memento.Common.ConverterPatterns;

namespace Memento.Services.Converter
{
    class TextOperationService
    {
        internal static string TagsToLineBreaks(string text)
        {
            var result1 = Regex.Replace(text, "<br />", Environment.NewLine);
            var result2 = Regex.Replace(result1, "<div>", string.Empty);
            var result3 = Regex.Replace(result2, "</div>", string.Empty);
            return result3;
        }

        internal static string LineBreaksToTags(string text) =>
            Regex.Replace(text, Environment.NewLine, LineBreakTag);

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
    }
}
