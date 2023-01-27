using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Parsing;
using Sprache;

namespace Tests.Parsing
{
    public class HeredocParsingFixture
    {
        [TestCase("<<ZZZ\n", "ZZZ", OclStringLiteralFormat.Heredoc)]
        [TestCase("<<ZZZ\r\n", "ZZZ", OclStringLiteralFormat.Heredoc)]
        [TestCase("<<-ZZZ\n", "ZZZ", OclStringLiteralFormat.IndentedHeredoc)]
        [TestCase("<<Z\n", "Z", OclStringLiteralFormat.Heredoc)]
        [TestCase("<<Z \t  \n", "Z", OclStringLiteralFormat.Heredoc)]
        [TestCase("<<!!!!\n", "!!!!", OclStringLiteralFormat.Heredoc)]
        public void Start(string input, string tag, OclStringLiteralFormat format)
            => HeredocParser.Start
                .Parse(input)
                .Should()
                .Be((tag, format));

        [TestCase("<<ZZZ")]
        [TestCase("<<Z Z\n")]
        [TestCase("<< ZZ\n")]
        [TestCase("<<- ZZ\n")]
        [TestCase("<<- Z\nZ\n")]
        public void StartInvalid(string input)
            => HeredocParser.Start
                .TryParse(input)
                .WasSuccessful
                .Should()
                .BeFalse();

        [TestCase("ZZZ", "ZZZ", Reason = "Exact Match")]
        [TestCase("ZZZ\n", "ZZZ", Reason = "Trailing line breaks removed")]
        [TestCase("\t\tZZZ\t\t\n", "\t\tZZZ", Reason = "Leading whitespace returned for indent count")]
        [TestCase("ZZZ\nFoo\n", "ZZZ", Reason = "Proceeding nodes ignored")]
        public void End(string input, string expected)
            => HeredocParser.End("ZZZ")
                .Parse(input)
                .Should()
                .Be(expected);

        [TestCase("ZZZ A\n")]
        [TestCase("A ZZZ\n")]
        public void EndInvalid(string input)
            => HeredocParser.End("ZZZ")
                .TryParse(input)
                .WasSuccessful
                .Should()
                .BeFalse();

        [TestCase("<<ZZZ\nZZZ", "", Description = "HereDoc - Empty")]
        [TestCase("<<ZZZ\n \n \nZZZ", " \n ", Description = "HereDoc - Whitespace")]
        [TestCase("<<ZZZ\n  A\n  B\nZZZ", "  A\n  B", Description = "HereDoc - Indented")]
        [TestCase("<<ZZZ\nA ZZZ\nZZZ B\nZZZ", "A ZZZ\nZZZ B", Description = "HereDoc - End Marker In Text")]
        public void Heredoc(string input, string expected)
            => OclParser.Execute("Foo = " + input.ToUnixLineEndings())
                .Should()
                .HaveChildrenExactly(
                    new OclAttribute("Foo",
                        new OclStringLiteral(expected, OclStringLiteralFormat.Heredoc)
                        {
                            HeredocTag = "ZZZ"
                        }
                    )
                );

        [TestCase("<<-ZZZ\nZZZ", "", Reason = "IndentedHeredoc - Empty")]
        [TestCase("<<-ZZZ\n \n \nZZZ", " \n ", Reason = "IndentedHeredoc - Whitespace Preserved")]
        [TestCase("<<-ZZZ\n \n \n ZZZ", "\n", Reason = "IndentedHeredoc - Whitespace Removed")]
        [TestCase("<<-ZZZ\n  A\n   B\n  ZZZ", "A\n B", Reason = "IndentedHeredoc - Offset By End Indent")]
        [TestCase("<<-ZZZ\n  A ZZZ\nZZZ B\nZZZ", "  A ZZZ\nZZZ B", Reason = "IndentedHeredoc - End Marker In Text")]
        [TestCase("<<-ZZZ\n  A\nB\n     ZZZ", "  A\nB", Reason = "IndentedHeredoc - Min Across All Lines")]
        public void IndentedHeredoc(string input, string expected)
            => OclParser.Execute("Foo = " + input.ToUnixLineEndings())
                .Should()
                .HaveChildrenExactly(
                    new OclAttribute("Foo",
                        new OclStringLiteral(expected, OclStringLiteralFormat.IndentedHeredoc)
                        {
                            HeredocTag = "ZZZ"
                        }
                    )
                );

        [TestCase("\r\n")]
        [TestCase("\n")]
        public void LineEndingsArePreserved(string lineEnding)
        {
            var documentLineEnding = lineEnding == "\n" ? "\r\n" : "\n"; // The opposite

            var expected = $"{lineEnding}A{lineEnding}B{lineEnding}";

            var attribute = new OclAttribute("value",
                new OclStringLiteral(expected, OclStringLiteralFormat.Heredoc)
                {
                    HeredocTag = "ZZZ"
                }
            );

            var sb = new StringBuilder();
            using (var tw = new StringWriter(sb) { NewLine = documentLineEnding })
            using (var writer = new OclWriter(tw))
            {
                writer.Write(attribute);
            }

            OclParser.Execute(sb.ToString())
                .Should()
                .HaveChildrenExactly(attribute);
        }

        [TestCase("")]
        [TestCase("A")]
        [TestCase("", "", "")]
        [TestCase("A", "      B")]
        [TestCase("A", " ", "      B")]
        [TestCase("    A", "B")]
        public void UnindentNoChange(params string[] input)
        {
            var result = HeredocParser.Unindent(input, "", OclStringLiteralFormat.IndentedHeredoc);
            result.Should().BeEquivalentTo(input);
        }

        [TestCase("    A")]
        [TestCase("  ", "    ")]
        [TestCase("    A", "\t\t\t\tB")]
        [TestCase("    A", "        B")]
        [TestCase("    A", "        B")]
        [TestCase("    A",
            "  ",
            "                    ",
            "    B",
            Description = "Empty lines do not affect unindent")]
        public void UnindentBy4Chars(params string[] input)
        {
            var expected = input.Select(i => i.Length < 4 ? "" : i.Substring(4));
            var result = HeredocParser.Unindent(input, "", OclStringLiteralFormat.IndentedHeredoc);
            result.Should().BeEquivalentTo(expected);
        }
    }
}