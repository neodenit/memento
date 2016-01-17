﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Core
{
    public static class ConverterPatterns
    {
        public const string ClozePattern = @"{{(\w+)::((?:(?!}}).)+?)(::((?:(?!}}).)+?))?}}";
        public const string LabelPattern = @"\w+";
        public const string RawDelimeter = "\t";
        public const string LineBreakTag = "<br />";
        public const string AltLineBreakTag = "<div></div>";
        public const string DelimeterTag = "<hr />";
        public const string Mask = "...";

        public static string EmptyLine { get; } = Environment.NewLine + Environment.NewLine;
        public static string Delimeter { get; } = EmptyLine + Settings.Default.CommentDelimeter + EmptyLine;
    }
}
