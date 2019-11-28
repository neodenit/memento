using System;
using Memento.Common;

namespace Memento.Core.Converter
{
    public static class ConverterPatterns
    {
        public const string ClozePattern = @"{{(\w+)::((?:(?!}}).)+?)(::((?:(?!}}).)+?))?}}";
        public const string LabelPattern = @"\w+";
        public const string RawDelimiter = "\t";
        public const string LineBreakTag = "<br />";
        public const string DelimiterTag = "<hr />";
        public const string Mask = "...";
        public const string Tab = "\t";
        public const string TabReplacement = "    ";

        public static string EmptyLine { get; } = Environment.NewLine + Environment.NewLine;
    }
}
