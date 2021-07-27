using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.Converters
{
    public class BlockOclConverterFixture
    {
        [Test]
        public void NameCaseIsKept()
        {
            var context = new OclConversionContext(new OclSerializerOptions());
            var data = new object();
            var result = (OclBlock)new DefaultBlockOclConverter().ToElements(context, typeof(WithIndexer).GetProperty(nameof(WithIndexer.MyProp))!, data).Single();
            result.Name.Should().Be("my_prop");
        }

        [Test]
        public void AttributesComeBeforeBlocks()
        {
            var context = new OclConversionContext(new OclSerializerOptions());
            var data = new
            {
                MyBlock = new { BlockProp = "OtherValue" },
                MyProp = "MyValue"
            };
            var result = (OclBlock)new DefaultBlockOclConverter().ToElements(context, typeof(WithIndexer).GetProperty(nameof(WithIndexer.MyProp)), data).Single();
            result.First()
                .Should()
                .BeEquivalentTo(new OclAttribute("my_prop", "MyValue"));
        }

        [Test]
        public void IndexersAreIgnored()
        {
            var context = new OclConversionContext(new OclSerializerOptions());
            var data =
                new Dummy
                {
                    Foo = new WithIndexer()
                };
            var result = (OclBlock)new DefaultBlockOclConverter().ToElements(context, typeof(Dummy).GetProperty(nameof(Dummy.Foo)), data.Foo).Single();
            result.Should()
                .Be(
                    new OclBlock("foo")
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

        class Dummy
        {
            public object? Foo { get; set; }
        }
    }
}