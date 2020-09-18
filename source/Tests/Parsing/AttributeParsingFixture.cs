using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Parsing;

namespace Tests.Parsing
{
    public class AttributeParsingFixture
    {
        [Test]
        public void String()
            => OclParser.Execute(@"Foo = ""Bar""")
                .Should()
                .HaveChildrenExactly(new OclAttribute("Foo", "Bar"));

        [Test]
        public void WithExtraWhitespace()
            => OclParser.Execute(" \t   Foo    = \t  \"Bar\"  \t  ")
                .Should()
                .HaveChildrenExactly(new OclAttribute("Foo", "Bar"));

        [TestCase("1", 1)]
        [TestCase("-1", -1)]
        public void Int(string input, int expected)
        {
            var result = OclParser.Execute(@"Foo = " + input);
            result
                .Should()
                .HaveChildrenExactly(new OclAttribute("Foo", expected));

            result.OfType<OclAttribute>().First().Value.Should().BeOfType(typeof(int));
        }

        [TestCase("1.5", 1.5)]
        [TestCase("-1.5", -1.5)]
        public void Decimal(string input, decimal expected)
            => OclParser.Execute(@"Foo = " + input)
                .Should()
                .HaveChildrenExactly(new OclAttribute("Foo", expected));

        [Test]
        public void Null()
            => OclParser.Execute("Foo = null")
                .Should()
                .HaveChildrenExactly(new OclAttribute("Foo", null));

        [Test]
        public void True()
            => OclParser.Execute("Foo = true")
                .Should()
                .HaveChildrenExactly(new OclAttribute("Foo", true));

        [Test]
        public void False()
            => OclParser.Execute("Foo = false")
                .Should()
                .HaveChildrenExactly(new OclAttribute("Foo", false));

        [TestCase("Foo=[]")]
        [TestCase("Foo = []")]
        [TestCase("Foo = [ ] ")]
        [TestCase("Foo=[    ]")]
        public void EmptyArray(string input)
            => OclParser.Execute(input)
                .Should()
                .HaveChildrenExactly(new OclAttribute("Foo", new string[0]));

        [TestCase(@"Foo=[""A""]")]
        [TestCase(@"Foo=  [    ""A""           ]")]
        public void SingleStringArray(string input)
            => OclParser.Execute(input)
                .Should()
                .HaveChildrenExactly(new OclAttribute("Foo", new[] { "A" }));

        [TestCase(@"Foo=[""A"",""B""]")]
        [TestCase(@"Foo = [ ""A"",""B"" ] ")]
        [TestCase(@"Foo =[""A"", ""B""]")]
        [TestCase(@"Foo=  [    ""A""           ,        ""B""        ]")]
        public void StringArray(string input)
            => OclParser.Execute(input)
                .Should()
                .HaveChildrenExactly(new OclAttribute("Foo", new[] { "A", "B" }));

        [TestCase(@"Foo=[1]")]
        [TestCase(@"Foo=  [    1           ]")]
        public void SingleIntArray(string input)
            => OclParser.Execute(input)
                .Should()
                .HaveChildrenExactly(new OclAttribute("Foo", new[] { 1 }));

        [TestCase(@"Foo=[1,2]")]
        [TestCase(@"Foo = [ 1,2 ] ")]
        [TestCase(@"Foo =[1, 2]")]
        [TestCase(@"Foo=  [    1           ,        2        ]")]
        public void IntArray(string input)
            => OclParser.Execute(input)
                .Should()
                .HaveChildrenExactly(new OclAttribute("Foo", new[] { 1, 2 }));

        [TestCase(@"Foo=[""A""""B""]", TestName = "Array without separator")]
        [TestCase(@"Foo=[1 2]", TestName = "Array without separator")]
        [TestCase(@"Foo=[1,,2]", TestName = "Array missing element separator")]
        public void Invalid(string input)
        {
            var (document, error) = OclParser.TryExecute(input);
            error.Should().NotBeEmpty();
            document.Should().BeNull();
        }

        [Test]
        public void MultipleInRoot()
            => OclParser.Execute(@"
                One = 1
                Two = 2
                ")
                .Should()
                .HaveChildrenExactly(
                    new OclAttribute("One", 1),
                    new OclAttribute("Two", 2)
                );

        [Test]
        public void MultipleInBlock()
            => OclParser.Execute(@"
                MyBlock {
                    One = 1
                    Two = 2
                }
                ")
                .Should()
                .HaveChildrenExactly(
                    new OclBlock("MyBlock")
                    {
                        new OclAttribute("One", 1),
                        new OclAttribute("Two", 2)
                    }
                );
    }
}