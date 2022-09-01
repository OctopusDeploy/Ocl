using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;

namespace Tests.ToString
{
    [Parallelizable(ParallelScope.All)]
    public class OclWriterFixture
    {
        static IEnumerable<TestCaseData> WriteAttributeDataSource()
        {
            TestCaseData CreateCase(string name, object? value, string expected) => new(value, expected) { TestName = "WriteAttribute value: " + name };

            yield return CreateCase("null", null, "null");
            yield return CreateCase("bool", false, "false");
            yield return CreateCase("char", '5', "\"5\"");
            yield return CreateCase("byte", (byte)5, "5");
            yield return CreateCase("short", (short)-5, "-5");
            yield return CreateCase("ushort", (ushort)5, "5");
            yield return CreateCase("uint", 5u, "5");
            yield return CreateCase("int", 5, "5");

            yield return CreateCase("negative", -5, "-5");
            yield return CreateCase("long", -4398257458902378590L, "-4398257458902378590");
            yield return CreateCase("ulong", 4398257458902437590uL, "4398257458902437590");
            yield return CreateCase("float", 3243242.43432f, "3243242.5");
            yield return CreateCase("double", 3243242.43432d, "3243242.43432");
            yield return CreateCase("double small exponential", 3243242e+3, "3243242000");
            yield return CreateCase("double large exponential", 3243242e+34, "3.243242E+40");
            yield return CreateCase("decimal", 3243242.43432m, "3243242.43432");
            yield return CreateCase("string", "MyValue", @"""MyValue""");
            yield return CreateCase("double quotes", @"a""b", @"""a\""b""");
            yield return CreateCase("string array", new[] { "B", "C" }, @"[""B"", ""C""]");
            yield return CreateCase("Int array", new[] { 4, 3, 4 }, @"[4, 3, 4]");
            yield return CreateCase("single line string slashes are escaped", new OclStringLiteral(@"a\c\r\nb\t", OclStringLiteralFormat.SingleLine), @"""a\\c\\r\\nb\\t""");
            yield return CreateCase("single line string tab is escaped", new OclStringLiteral("a\tb", OclStringLiteralFormat.SingleLine), @"""a\tb""");
            yield return CreateCase("single line string newlines are escaped", new OclStringLiteral("a\r\nb", OclStringLiteralFormat.SingleLine), @"""a\r\nb""");
            yield return CreateCase("single line string double quotes are escaped", new OclStringLiteral("a\"b", OclStringLiteralFormat.SingleLine), @"""a\""b""");
        }

        [TestCaseSource(nameof(WriteAttributeDataSource))]
        public void WriteAttribute(object? input, string expected)
            => Execute(w => w.Write(new OclAttribute("MyAttr", input)))
                .Should()
                .Be($"MyAttr = {expected}");

        [Test]
        public void WriteAttributeInvalidValueThrows()
        {
            Action action = () => Execute(w => w.Write(new OclAttribute("MyAttr", new Random())));
            action.Should()
                .Throw<OclException>()
                .WithMessage("*System.Random*");
        }

        [Test]
        public void WriteAttributeLeadingNumberInName()
            => Execute(w => w.Write(new OclAttribute("0MyAttr", 5)))
                .Should()
                .Be("_0MyAttr = 5");

        [Test]
        public void WriteAttributeSpecialCharactersInName()
            => Execute(w => w.Write(new OclAttribute("My0%&2_'\"-Attr", 5)))
                .Should()
                .Be("My0__2___-Attr = 5");

        [Test]
        public void WriteDictionaryAttributes()
        {
            var dict = new Dictionary<string, object?>
            {
                { "One.One", "1" },
                { "Two Two", 2 },
                { "Three\"Three", null }
            };

            var expected = @"properties = {
    One.One = ""1""
    ""Three\""Three"" = null
    ""Two Two"" = 2
}";

            Execute(w => w.Write(new OclAttribute("properties", dict)))
                .Trim()
                .Should()
                .Be(expected.ToUnixLineEndings());
        }

        [Test]
        public void Heredoc()
        {
            var literal = new OclStringLiteral(" a\n    b", OclStringLiteralFormat.Heredoc) { HeredocTag = "ZZZ" };
            var block = new OclBlock("MyBlock")
            {
                new OclAttribute("MyAttr", literal)
            };

            var expected = @"MyBlock {
    MyAttr = <<ZZZ
 a
    b
ZZZ
}";

            Execute(w => w.Write(block))
                .Should()
                .Be(expected.ToUnixLineEndings());
        }

        [Test]
        public void HeredocIndented()
        {
            var literal = new OclStringLiteral(" a\n    b", OclStringLiteralFormat.IndentedHeredoc);
            var block = new OclBlock("MyBlock")
            {
                new OclAttribute("MyAttr", literal)
            };

            var expected = @"MyBlock {
    MyAttr = <<-EOT
             a
                b
        EOT
}";

            Execute(w => w.Write(block))
                .Should()
                .Be(expected.ToUnixLineEndings());
        }

        [Test]
        public void MultilineStringsUseHeredocAndTheHeredocIdentifierFromOptions()
        {
            var options = new OclSerializerOptions
            {
                DefaultHeredocTag = "YYY"
            };

            var expected = @"MyAttr = <<-YYY
        a
        b
    YYY";

            Execute(w => w.Write(new OclAttribute("MyAttr", "a\nb")), options)
                .Should()
                .Be(expected.ToUnixLineEndings());
        }

        [Test]
        public void WriteBlockEmpty()
            => Execute(w => w.Write(new OclBlock("MyBlock")))
                .Should()
                .Be("MyBlock {}");

        [Test]
        public void WriteBlockSpecialCharactersInName()
            => Execute(w => w.Write(new OclBlock("My0%&2_'\"-Block")))
                .Should()
                .Be("My0__2___-Block {}");

        [Test]
        public void WriteBlockSingleLabel()
        {
            var block = new OclBlock("MyBlock");
            block.Labels.Add("MyLabel");

            Execute(w => w.Write(block))
                .Should()
                .Be("MyBlock \"MyLabel\" {}");
        }

        [Test]
        public void WriteBlockMultipleLabel()
        {
            var block = new OclBlock("MyBlock");
            block.Labels.Add("MyLabel");
            block.Labels.Add("OtherLabel");
            block.Labels.Add("LastLabel");

            Execute(w => w.Write(block))
                .Should()
                .Be("MyBlock \"MyLabel\" \"OtherLabel\" \"LastLabel\" {}");
        }

        [Test]
        public void WriteBlockDoubleQuotesInLabel()
        {
            var block = new OclBlock("MyBlock");
            block.Labels.Add("My\"Label");

            Execute(w => w.Write(block))
                .Should()
                .Be("MyBlock \"My\\\"Label\" {}");
        }

        [Test]
        public void WriteBlockSingleChildBlock()
        {
            var block = new OclBlock("MyBlock")
            {
                new OclBlock("Child")
            };

            var expected = @"MyBlock {
    Child {}
}";

            Execute(w => w.Write(block))
                .Should()
                .Be(expected.ToUnixLineEndings());
        }

        [Test]
        public void WriteBlockSingleChildAttribute()
        {
            var block = new OclBlock("MyBlock")
            {
                new OclAttribute("Child", 5)
            };

            var expected = @"MyBlock {
    Child = 5
}";

            Execute(w => w.Write(block))
                .Should()
                .Be(expected.ToUnixLineEndings());
        }

        [Test]
        public void IndentOptionsAreUsed()
        {
            var options = new OclSerializerOptions
            {
                IndentChar = '+',
                IndentDepth = 5
            };

            var block = new OclBlock("MyBlock")
            {
                new OclAttribute("Child", 5)
            };

            var expected = @"MyBlock {
+++++Child = 5
}";

            Execute(w => w.Write(block), options)
                .Should()
                .Be(expected.ToUnixLineEndings());
        }

        [Test]
        public void WriteBlockMixedAttributesAndBlocks()
        {
            var block = new OclBlock("MyBlock")
            {
                new OclAttribute("First", 1),
                new OclAttribute("Second", 2),
                new OclBlock("Third")
                {
                    new OclAttribute("ThirdChild", 3)
                },
                new OclBlock("Fourth"),
                new OclAttribute("Last", 9)
            };

            var expected = @"MyBlock {
    First = 1
    Second = 2

    Third {
        ThirdChild = 3
    }

    Fourth {}

    Last = 9
}";

            Execute(w => w.Write(block))
                .Should()
                .Be(expected.ToUnixLineEndings());
        }

        [Test]
        public void CollectionAttributeFollowedByEndOfBlock()
        {
            var block = new OclBlock("OuterBlock")
            {
                new OclBlock("InnerBlock")
                {
                    new OclAttribute("MapAttribute",
                        new Dictionary<string, object>
                            { { "alpha", 1 }, { "bravo", 2 } })
                }
            };

            const string expected = @"OuterBlock {
    InnerBlock {
        MapAttribute = {
            alpha = 1
            bravo = 2
        }
    }
}";

            Execute(w => w.Write(block))
                .Should()
                .Be(expected.ToUnixLineEndings());
        }

        [Test]
        public void CollectionAttributeFollowedByAttribute()
        {
            var block = new OclBlock("OuterBlock")
            {
                new OclBlock("InnerBlock")
                {
                    new OclAttribute("MapAttribute",
                        new Dictionary<string, object>
                            { { "alpha", 1 }, { "bravo", 2 } }),
                    new OclAttribute("StringAttribute", "Value")
                }
            };

            const string expected = @"OuterBlock {
    InnerBlock {
        MapAttribute = {
            alpha = 1
            bravo = 2
        }
        StringAttribute = ""Value""
    }
}";

            Execute(w => w.Write(block))
                .Should()
                .Be(expected.ToUnixLineEndings());
        }

        string Execute(Action<OclWriter> when, OclSerializerOptions? options = null)
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (var writer = new OclWriter(sw, options))
            {
                sw.NewLine = "\n";
                when(writer);
            }

            return sb.ToString();
        }
    }
}