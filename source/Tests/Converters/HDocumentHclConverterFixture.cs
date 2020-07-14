using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.Converters
{
    public class OclDocumentOclConverterFixture
    {
        [Test]
        public void IndexersAreIgnored()
        {
            var context = new OclConversionContext(new OclSerializerOptions());
            var data = new WithIndexer();
            var result = (OclBlock)new DefaultBlockOclConverter().Convert(context, "Test", data).Single();
            result.Should()
                .Be(
                    new OclBlock("Test")
                    {
                        new OclAttribute("MyProp", "MyValue")
                    }
                );
        }

        class WithIndexer
        {
            public string MyProp => "MyValue";
            public string this[int index] => throw new NotImplementedException();
        }
    }
}