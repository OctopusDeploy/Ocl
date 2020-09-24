using System;
using System.Collections.Generic;
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

            OclConvert.ToOclDocument(new FakeType(), options)
                .Should()
                .HaveChildrenExactly(new OclAttribute("Fake", null));
        }

        class FakeType
        {
            public int Foo { get; set; } = 1;
        }

        class FakeConverter : IOclConverter
        {
            public bool CanConvert(Type type)
                => type == typeof(FakeType);

            public IEnumerable<IOclElement> ToElements(OclConversionContext context, string name, object value)
                => throw new NotImplementedException();

            public OclDocument ToDocument(OclConversionContext context, object obj)
                => new OclDocument(new[] { new OclAttribute("Fake", null) });

            public object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue)
                => throw new NotImplementedException();
        }
    }
}