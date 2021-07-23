using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.Converters
{
    public class CustomOclAttributeFixture
    {
        
        [Test]
        public void AttributeNameIsUsed()
        {
            var context = new OclConversionContext(new OclSerializerOptions());
            var result = (OclAttribute)new DefaultAttributeOclConverter().ToElements(context, typeof(Dummy).GetProperty(nameof(Dummy.PropWithAttribute))!, "whatever").Single();
            result.Name.Should().Be("NewName");
        }

        class Dummy
        {
            [OclAttribute("NewName")]
            public string PropWithAttribute { get; } = "Value";
        }
    }
}