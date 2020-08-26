using FluentAssertions;
using NUnit.Framework;
using Octopus.Hcl;
using Octopus.Hcl.Converters;

namespace Tests
{
    public class HDocumentFactoryFixture
    {

        [Test]
        public void Null()
        {
            HclConvert.ToHDocument(null, new HclSerializerOptions())
                .Should()
                .NotBeNull()
                .And
                .BeEmpty();
        }

        [Test]
        public void SimpleObject()
        {
            HclConvert.ToHDocument(new { MyProp = "MyValue" }, new HclSerializerOptions())
                .Should()
                .BeEquivalentTo(
                    new HAttribute("MyProp", "MyValue")
                );
        }


        [Test]
        public void LabelsAreIncludedAsAttributes()
        {
            HclConvert.ToHDocument(new SampleWithLabel(), new HclSerializerOptions())
                .Should()
                .BeEquivalentTo(
                    new HAttribute("ALabel", "The Label")
                );
        }


        class SampleWithLabel
        {
            [HclLabel]
            public string ALabel { get; } = "The Label";
        }
    }
}