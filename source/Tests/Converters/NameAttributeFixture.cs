using System;
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
        public void SerializingAttribute()
        {
            var context = new OclConversionContext(new OclSerializerOptions());
            var result = (OclAttribute)new DefaultAttributeOclConverter().ToElements(context, typeof(DummyWithAttribute).GetProperty(nameof(DummyWithAttribute.AttributeProperty))!, "whatever").Single();
            result.Name.Should().Be("NewName");
        }

        [Test]
        public void SerializingBlock()
        {
            var context = new OclConversionContext(new OclSerializerOptions());
            var result = (OclBlock)new DefaultBlockOclConverter().ToElements(context, typeof(DummyWithBlock).GetProperty(nameof(DummyWithBlock.BlockProperty))!, new DummyWithAttribute()).Single();
            result.Name.Should().Be("AnotherName");
        }

        [Test]
        public void Deserializing()
        {
            var context = new OclConversionContext(new OclSerializerOptions());
            var result = (DummyWithBlock?)new DefaultBlockOclConverter().FromElement(context,
                typeof(DummyWithBlock),
                new OclBlock("whatever",
                    Array.Empty<string>(),
                    new[] { new OclBlock("AnotherName", Array.Empty<string>(), new[] { new OclAttribute("NewName", "Boo") }) }),
                null);

            result?.BlockProperty.AttributeProperty.Should().Be("Boo");
        }

        class DummyWithAttribute
        {
            [OclName("NewName")]
            public string AttributeProperty { get; set; } = "Value";
        }

        class DummyWithBlock
        {
            [OclName("AnotherName")]
            public DummyWithAttribute BlockProperty { get; set; } = new();
        }
    }
}