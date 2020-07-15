using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Hcl;

namespace Tests
{
    [Parallelizable(ParallelScope.All)]
    public class HclWriterFixture
    {
        private static IEnumerable<TestCaseData> WriteAttributeDataSource()
        {
            TestCaseData CreateCase(string name, object? value, string expected)
                => new TestCaseData(value, expected) { TestName = "WriteAttribute value: " + name };

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
            yield return CreateCase("single line string slashes are escaped", new HclStringLiteral(@"a\c\r\nb\t", HclStringLiteralFormat.SingleLine), @"""a\\c\\r\\nb\\t""");
            yield return CreateCase("single line string tab is escaped", new HclStringLiteral("a\tb", HclStringLiteralFormat.SingleLine), @"""a\tb""");
            yield return CreateCase("single line string newlines are escaped", new HclStringLiteral("a\r\nb", HclStringLiteralFormat.SingleLine), @"""a\r\nb""");
            yield return CreateCase("single line string double quotes are escaped", new HclStringLiteral("a\"b", HclStringLiteralFormat.SingleLine), @"""a\""b""");
        }

        [TestCaseSource(nameof(WriteAttributeDataSource))]
        public void WriteAttribute(object? input, string expected)
            => Execute(w => w.Write(new HAttribute("MyAttr", input)))
                .Should()
                .Be($"MyAttr = {expected}");

        
        [Test]
        public void WriteAttributeInvalidValueThrows()
        {
            Action action = () => Execute(w => w.Write(new HAttribute("MyAttr", new Random())));
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("*System.Random*");
        }
        
        [Test]
        public void WriteAttributeLeadingNumberInName()
            => Execute(w => w.Write(new HAttribute("0MyAttr", 5)))
                .Should()
                .Be("_0MyAttr = 5");

        [Test]
        public void WriteAttributeSpecialCharactersInName()
            => Execute(w => w.Write(new HAttribute("My0%&2_'\"-Attr", 5)))
                .Should()
                .Be("My0__2___-Attr = 5");
    
        [Test]
        public void Heredoc()
        {
            var literal = new HclStringLiteral(" a\n    b", HclStringLiteralFormat.Heredoc) { HeredocIdentifier = "ZZZ" };
            var block = new HBlock("MyBlock")
            {
                new HAttribute("MyAttr", literal)
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
            var literal = new HclStringLiteral(" a\n    b", HclStringLiteralFormat.IndentedHeredoc);
            var block = new HBlock("MyBlock")
            {
                new HAttribute("MyAttr", literal)
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
            var options = new HclSerializerOptions
            {
                DefaultHeredocIdentifier = "YYY"
            };

            var expected = @"MyAttr = <<-YYY
a
b
YYY";

            Execute(w => w.Write(new HAttribute("MyAttr", "a\nb")), options)
                .Should()
                .Be(expected.ToUnixLineEndings());
        }

        [Test]
        public void WriteBlockEmpty()
            => Execute(w => w.Write(new HBlock("MyBlock")))
                .Should()
                .Be("MyBlock {\n}");

        [Test]
        public void WriteBlockSpecialCharactersInName()
            => Execute(w => w.Write(new HBlock("My0%&2_'\"-Block")))
                .Should()
                .Be("My0__2___-Block {\n}");

        [Test]
        public void WriteBlockSingleLabel()
        {
            var block = new HBlock("MyBlock");
            block.Labels.Add("MyLabel");

            Execute(w => w.Write(block))
                .Should()
                .Be("MyBlock \"MyLabel\" {\n}");
        }

        [Test]
        public void WriteBlockMultipleLabel()
        {
            var block = new HBlock("MyBlock");
            block.Labels.Add("MyLabel");
            block.Labels.Add("OtherLabel");
            block.Labels.Add("LastLabel");

            Execute(w => w.Write(block))
                .Should()
                .Be("MyBlock \"MyLabel\" \"OtherLabel\" \"LastLabel\" {\n}");
        }

        [Test]
        public void WriteBlockDoubleQuotesInLabel()
        {
            var block = new HBlock("MyBlock");
            block.Labels.Add("My\"Label");

            Execute(w => w.Write(block))
                .Should()
                .Be("MyBlock \"My\\\"Label\" {\n}");
        }

        [Test]
        public void WriteBlockSingleChildBlock()
        {
            var block = new HBlock("MyBlock")
            {
                new HBlock("Child")
            };

            var expected = @"MyBlock {

    Child {
    }
}";

            Execute(w => w.Write(block))
                .Should()
                .Be(expected.ToUnixLineEndings());
        }

        [Test]
        public void WriteBlockSingleChildAttribute()
        {
            var block = new HBlock("MyBlock")
            {
                new HAttribute("Child", 5)
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
            var options = new HclSerializerOptions
            {
                IndentChar = '+',
                IndentDepth = 5
            };

            var block = new HBlock("MyBlock")
            {
                new HAttribute("Child", 5)
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
            var block = new HBlock("MyBlock")
            {
                new HAttribute("First", 1),
                new HAttribute("Second", 2),
                new HBlock("Third")
                {
                    new HAttribute("ThirdChild", 3)
                },
                new HBlock("Fourth"),
                new HAttribute("Last", 9)
            };

            var expected = @"MyBlock {
    First = 1
    Second = 2

    Third {
        ThirdChild = 3
    }

    Fourth {
    }

    Last = 9
}";

            Execute(w => w.Write(block))
                .Should()
                .Be(expected.ToUnixLineEndings());
        }


        private string Execute(Action<HclWriter> when, HclSerializerOptions? options = null)
        {
            var sb = new StringBuilder();
            using(var sw = new StringWriter(sb))
            using (var writer = new HclWriter(sw, options))
            {
                sw.NewLine = "\n";
                when(writer);
            }

            return sb.ToString();
        }
    }
}