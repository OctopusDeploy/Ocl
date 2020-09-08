using System;
using Sprache;

namespace Octopus.Ocl.Parsing
{
    public class ParserCommon
    {
        public static readonly Parser<Char> NewLine = Parse.Char(c => c == '\n', "newline");
        public static readonly Parser<Char> WhitespaceExceptNewline = Parse.Char(c => char.IsWhiteSpace(c) && c != '\n', "Whitespace except newline");
    }
}