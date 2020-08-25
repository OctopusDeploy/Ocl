using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Hcl;
using Octopus.Hcl.Converters;

namespace Tests.Converters
{
    public class DefaultAttributeHclConverterFixture
    {
        [Test]
        public void NameCaseIsKept()
        {
            var context = new HclConversionContext(new HclSerializerOptions());
            var result = (HAttribute) new DefaultAttributeHclConverter().Convert(context, "Test", "Value").Single();
            result.Name.Should().Be("Test");
        }
    }
}