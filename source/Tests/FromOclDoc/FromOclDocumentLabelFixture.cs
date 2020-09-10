using System;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;

namespace Tests.FromOclDoc
{
    public class FromOclDocumentLabelFixture
    {
        [Test]
        public void LabelIsAssignedToProperty()
        {
            var document = new OclDocument()
            {
                new OclBlock("Item", "Label1Value")
            };

            OclConvert.FromOclDocument<Parent<SampleWithLabelAttributes>>(document, new OclSerializerOptions())
                .Item
                .Should()
                .BeEquivalentTo(new SampleWithLabelAttributes() { FirstLabel = "Label1Value" });
        }

        [Test]
        public void MultipleLabelsAreAssignedToProperties()
        {
            var document = new OclDocument()
            {
                new OclBlock("Item", new[] { "Label1Value", "Label2Value" })
            };

            OclConvert.FromOclDocument<Parent<SampleWithLabelAttributes>>(document, new OclSerializerOptions())
                .Item
                .Should()
                .BeEquivalentTo(new SampleWithLabelAttributes()
                {
                    FirstLabel = "Label1Value",
                    SecondLabel = "Label2Value"
                });
        }

        [Test]
        public void ErrorIsThrownIfNoLabelAttributeIsFound()
        {
            var document = new OclDocument()
            {
                new OclBlock("Item", "Label1Value")
            };

            Action action = () => OclConvert.FromOclDocument<Parent<SampleWithoutLabelAttribute>>(document, new OclSerializerOptions());
            action.Should()
                .Throw<OclException>()
                .WithMessage("*The block 'Item' defines 1 labels (Label1Value) but the type SampleWithoutLabelAttribute only has 0 label properties*");
        }

        class Parent<T> where T : class
        {
            public T? Item { get; set; }
        }

        class SampleWithLabelAttributes
        {
            [OclLabel]
            public string? FirstLabel { get; set; }

            [OclLabel]
            public string? SecondLabel { get; set; }
        }

        class SampleWithoutLabelAttribute
        {
            public string? ALabel { get; }
        }
    }
}