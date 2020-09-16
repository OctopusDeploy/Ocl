using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace Octopus.Ocl.Parsing
{
    /// <summary><![CDATA[
    /// Parses HereDoc e.g.
    ///
    /// <<EOF
    /// Some Text
    /// Goes here
    /// EOF
    ///
    /// or
    ///
    /// <<-EOF
    ///   Some Text
    ///   Goes here
    /// EOF
    /// ]]></summary>
    class HeredocParser
    {
        /// <summary>
        /// Parses the first line of heredoc, including the newline
        /// </summary>
        /// <remarks><![CDATA[
        /// <<EOF
        ///
        /// or
        ///
        /// <<-EOF
        /// ]]></remarks>
        internal static readonly Parser<(string tag, OclStringLiteralFormat format)> Start =
            from open in Parse.Char('<').Repeat(2)
            from indentMarker in Parse.Char('-').Optional()
            from tag in Parse.Char(c => !char.IsWhiteSpace(c), "Not whitespace").Many().Text()
            from whitespace in ParserCommon.WhitespaceExceptNewline.Many()
            from newline in ParserCommon.NewLine
            select (
                tag,
                indentMarker.IsDefined ? OclStringLiteralFormat.IndentedHeredoc : OclStringLiteralFormat.Heredoc
            );

        static readonly Parser<string> Line =
            from line in Parse.CharExcept('\n').Many().Text()
            from newline in ParserCommon.NewLine
            select line;

        public static readonly Parser<OclStringLiteral> Literal =
            from start in Start
            let endParser = End(start.tag)
            from lines in Line.Except(endParser).Many()
            from end in endParser
            let trimmed = TrimCarriageReturn(lines)
            let unindented = Unindent(trimmed.lines, start.format)
            let joined = string.Join(trimmed.lineSeparator, unindented)
            select new OclStringLiteral(joined, start.format)
            {
                HeredocTag = start.tag
            };

        internal static Parser<string> End(string tag)
        {
            var endLine = from leadingWhitespace in ParserCommon.WhitespaceExceptNewline.Many()
                from endtag in Parse.String(tag).Text()
                from trailingWhitespace in ParserCommon.WhitespaceExceptNewline.Many()
                select endtag;

            var terminateWithNewline = from endtag in endLine
                from newline in ParserCommon.NewLine
                select endtag;

            var terminateWithEof = endLine.End();

            return terminateWithNewline.Or(terminateWithEof);
        }

        static (string lineSeparator, IReadOnlyList<string> lines) TrimCarriageReturn(IEnumerable<string> lines)
        {
            var lineArr = lines.ToArray();
            var hasCarriageReturn = lineArr.Any(l => l.EndsWith('\r'));
            var trimmed = lineArr.Select(l => l.TrimEnd('\r')).ToArray();
            var lineSeparator = hasCarriageReturn ? "\r\n" : "\n";
            return (lineSeparator, trimmed);
        }

        public static IEnumerable<string> Unindent(IReadOnlyList<string> lines, OclStringLiteralFormat format)
        {
            if (format != OclStringLiteralFormat.IndentedHeredoc)
                return lines;

            if (lines.Count == 0)
                return lines;

            int CountLeadingWhitespace(string input)
            {
                for (var x = 0; x < input.Length; x++)
                    if (!char.IsWhiteSpace(input[x]))
                        return x;
                return int.MaxValue;
            }

            var unindentBy = lines
                .Min(CountLeadingWhitespace);

            return lines.Select(l => l.Length <= unindentBy ? "" : l.Substring(unindentBy));
        }
    }
}