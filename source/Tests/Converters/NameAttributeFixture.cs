using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.Converters
{
    public class NameAttributeFixture
    {
        [Test]
        public void NameIsUsedForAttribute()
        {
            var context = new OclConversionContext(new OclSerializerOptions());
            var result = (OclAttribute)new DefaultAttributeOclConverter().ToElements(context, typeof(DummyWithAttribute).GetProperty(nameof(DummyWithAttribute.AttributeProperty))!, "whatever").Single();
            result.Name.Should().Be("NewName");
        }

        [Test]
        public void NameIsUsedForBlock()
        {
            var context = new OclConversionContext(new OclSerializerOptions());
            var result = (OclBlock)new DefaultBlockOclConverter().ToElements(context, typeof(DummyWithBlock).GetProperty(nameof(DummyWithBlock.BlockProperty))!, new DummyWithAttribute()).Single();
            result.Name.Should().Be("AnotherName");
        }

        class DummyWithAttribute
        {
            [OclName("NewName")]
            public string AttributeProperty { get; } = "Value";
        }

        class DummyWithBlock
        {
            [OclName("AnotherName")]
            public DummyWithAttribute BlockProperty { get; } = new DummyWithAttribute();
        }
    }
}