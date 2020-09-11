using System.Linq;
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

        [TestCase("ZZZ\n")]
        [TestCase("ZZZ")]
        [TestCase("\t\tZZZ\t\t\n")]
        [TestCase("ZZZ\nFoo\n")]
        public void End(string input)
            => HeredocParser.End("ZZZ")
                .Parse(input)
                .Should()
                .Be("ZZZ");

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

        [TestCase("<<-ZZZ\nZZZ", "", Description = "IndentedHeredoc - Empty")]
        [TestCase("<<-ZZZ\n \n \nZZZ", "\n", Description = "IndentedHeredoc - Whitespace")]
        [TestCase("<<-ZZZ\n  A\n  B\nZZZ", "A\nB", Description = "IndentedHeredoc - NoExtraIndent")]
        [TestCase("<<-ZZZ\n  A\n    B\n  ZZZ", "A\n  B", Description = "IndentedHeredoc - ExtraIndent")]
        [TestCase("<<-ZZZ\n  A ZZZ\n   ZZZ B\nZZZ", "A ZZZ\n ZZZ B", Description = "IndentedHeredoc - End Marker In Text")]
        [TestCase("<<-ZZZ\n  A\n    B\nZZZ", "A\n  B", Description = "IndentedHeredoc - End marker does not affect indent")]
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

        [TestCase("")]
        [TestCase("A")]
        [TestCase("", "", "")]
        [TestCase("A", "      B")]
        [TestCase("A", " ", "      B")]
        [TestCase("    A", "B")]
        public void UnindentNoChange(params string[] input)
        {
            var result = HeredocParser.Unindent(input, OclStringLiteralFormat.IndentedHeredoc);
            result.Should().BeEquivalentTo(input);
        }

        [TestCase("    A")]
        [TestCase("  ", "    ")]
        [TestCase("    A", "\t\t\t\tB")]
        [TestCase("    A", "        B")]
        [TestCase("    A", "        B")]
        [TestCase("    A", "  ", "                    ", "    B", Description = "Empty lines do not affect unindent")]
        public void UnindentBy4Chars(params string[] input)
        {
            var expected = input.Select(i => i.Length < 4 ? "" : i.Substring(4));
            var result = HeredocParser.Unindent(input, OclStringLiteralFormat.IndentedHeredoc);
            result.Should().BeEquivalentTo(expected);
        }
    }
}