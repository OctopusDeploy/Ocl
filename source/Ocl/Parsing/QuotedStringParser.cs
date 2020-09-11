using System;
using Sprache;

namespace Octopus.Ocl.Parsing
{
    public class QuotedStringParser
    {
        public static readonly Parser<string> QuotedStringLiteral =
            from startQuote in Parse.Char('"')
            from str in Parse.CharExcept(new[] { '"', '\r', '\n' }).Many().Text()
            from endQuote in Parse.Char('"')
            select str;
    }
}