using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Hcl;
using Octopus.Hcl.Converters;

namespace Tests.Converters
{
    public class BlockHclConverterFixture
    {
        [Test]
        public void NameCaseIsKept()
        {
            var context = new HclConversionContext(new HclSerializerOptions());
            var data = new object();
            var result = (HBlock) new DefaultBlockHclConverter().Convert(context, "Test", data).Single();
            result.Name.Should().Be("Test");
        }

        [Test]
        public void AttributesComeBeforeBlocks()
        {
            var context = new HclConversionContext(new HclSerializerOptions());
            var data = new
            {
                MyBlock = new { BlockProp = "OtherValue" },
                MyProp = "MyValue"
            };
            var result = (HBlock) new DefaultBlockHclConverter().Convert(context, "Test", data).Single();
            result.First()
                .Should()
                .BeEquivalentTo(new HAttribute("MyProp", "MyValue"));
        }

        [Test]
        public void IndexersAreIgnored()
        {
            var context = new HclConversionContext(new HclSerializerOptions());
            var data = new WithIndexer();
            var result = (HBlock) new DefaultBlockHclConverter().Convert(context, "Test", data).Single();
            result.Should().BeEquivalentTo(new[] { new HAttribute("MyProp", "MyValue") });
        }

        class WithIndexer
        {
            public string MyProp => "MyValue";
            public string this[int index] => throw new NotImplementedException();
        }
    }
}