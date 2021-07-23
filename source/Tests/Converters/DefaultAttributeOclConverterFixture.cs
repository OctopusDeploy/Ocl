using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.Converters
{
    public class DefaultAttributeOclConverterFixture
    {
        [Test]
        public void NameCaseIsKept()
        {
            var context = new OclConversionContext(new OclSerializerOptions());
            var result = (OclAttribute)new DefaultAttributeOclConverter().ToElements(context, typeof(Dummy).GetProperty(nameof(Dummy.Test))!, "Value").Single();
            result.Name.Should().Be("test");
        }
        
        class Dummy
        {
            public string Test { get; } = "Value";
        }
    }
}