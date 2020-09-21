using System;
using System.Linq;
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
            var result = (OclBlock)new DefaultBlockOclConverter().ToElements(context, "Test", data).Single();
            result.Should()
                .Be(
                    new OclBlock("Test")
                    {
                        new OclAttribute("my_prop", "MyValue")
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