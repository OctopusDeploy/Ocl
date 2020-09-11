using System;
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

        [Test]
        public void Int()
            => OclParser.Execute(@"Foo = 1")
                .Should()
                .HaveChildrenExactly(new OclAttribute("Foo", 1));

        [Test]
        public void Decimal()
            => OclParser.Execute(@"Foo = 1.5")
                .Should()
                .HaveChildrenExactly(new OclAttribute("Foo", 1.5m));

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
    }
}