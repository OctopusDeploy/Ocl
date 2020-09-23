using System;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Parsing;

namespace Tests.Parsing
{
    public class ParserFixture
    {
        [Test]
        public void Empty()
            => OclParser.Execute("")
                .Should()
                .Be(new OclDocument());

        [Test]
        public void Blank()
            => OclParser.Execute(" \t \r\n \t ")
                .Should()
                .Be(new OclDocument());

        [Test]
        public void InvalidDocumentReportsCorrectLineNumber()
        {
            var (_, error) = OclParser.TryExecute(@"
                    Foo = ""Bar""

                    Foo = ""Bar
            ");

            error.Should().Be("Parsing failure: unexpected '\n'; expected \" (Line 4, Column 31); recently consumed: Foo = \"Bar");
        }
    }
}