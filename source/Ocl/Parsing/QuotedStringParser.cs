using System;
using Sprache;

namespace Octopus.Ocl.Parsing
{
    public class QuotedStringParser
    {
        static readonly Parser<char> EscapedChar =
            from slash in Parse.Char('\\')
            from ch in Parse.AnyChar
            select ConvertEscapedChar(ch);

        static char ConvertEscapedChar(in char ch)
        {
            switch (ch)
            {
                case 'r':
                    return '\r';
                case 'n':
                    return '\n';
                case 't':
                    return '\t';
                case '\\':
                    return '\\';
                case '"':
                    return '"';
                default:
                    throw new OclException(@"Unrecognised character escape: \" + ch);
            }
        }

        static readonly Parser<char> AnythingOtherThanEolOrQuote
            = Parse.CharExcept(new[] { '"', '\r', '\n' });

        public static readonly Parser<string> QuotedStringLiteral =
            from startQuote in Parse.Char('"')
            from str in EscapedChar.XOr(AnythingOtherThanEolOrQuote).Many().Text()
            from endQuote in Parse.Char('"')
            select str;
    }
}