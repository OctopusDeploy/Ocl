using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;

namespace Tests.ToOclDoc
{
    public class ToOclDocConverterFixture
    {
        [Test]
        public void ConverterIsUsedForTheRootDocument()
        {
            var options = new OclSerializerOptions();
            options.Converters.Add(new FakeConverter());

            new OclSerializer(options).ToOclDocument(new FakeType())
                .Should()
                .HaveChildrenExactly(new OclAttribute("Fake", null));
        }

        [Test]
        public void SerializingCollectionOfObjectsUsesClassNameAsBlockName()
        {
            var subject = new[]
            {
                new FakeType(),
                new FakeType { Foo = 2 }
            };

            new OclSerializer().ToOclDocument(subject)
                .Should()
                .HaveChildrenExactly(
                    new OclBlock("fake_type", Array.Empty<string>(), new[] { new OclAttribute("foo", 1) }),
                    new OclBlock("fake_type", Array.Empty<string>(), new[] { new OclAttribute("foo", 2) })
                );
        }

        [Test]
        public void DeserializingCollectionOfObjectsWithClassNamesAsBlockNames()
        {
            var result = new OclSerializer().Deserialize<IEnumerable<FakeType>>(
                new OclDocument(
                    new[]
                    {
                        new OclBlock("fake_type", Array.Empty<string>(), new[] { new OclAttribute("foo", 1) }),
                        new OclBlock("fake_type", Array.Empty<string>(), new[] { new OclAttribute("foo", 2) })
                    }));

            result.Count().Should().Be(2);
        }

        class FakeType
        {
            public int Foo { get; set; } = 1;
        }

        class FakeConverter : IOclConverter
        {
            public bool CanConvert(Type type)
                => type == typeof(FakeType);

            public IEnumerable<IOclElement> ToElements(OclConversionContext context, PropertyInfo? propertyInfo, object value)
                => throw new NotImplementedException();

            public OclDocument ToDocument(OclConversionContext context, object obj)
                => new OclDocument(new[] { new OclAttribute("Fake", null) });

            public object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue)
                => throw new NotImplementedException();
        }
    }
}