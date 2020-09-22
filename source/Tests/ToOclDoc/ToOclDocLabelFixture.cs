using System;
using System.Collections.Generic;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.ToOclDoc
{
    public class ToOclDocLabelFixture
    {
        [Test]
        public void LabelAttributeOnRootDocumentIsIgnoredAndPropertyIsIncludedAsAChild()
        {
            OclConvert.ToOclDocument(new SampleWithLabelAttribute())
                .Should()
                .HaveChildrenExactly(
                    new OclAttribute("my_label", "The Label")
                );
        }

        [Test]
        public void ByDefaultLabelsAreIdentifiedViaAttributeAndNotIncludedAsAChild()
        {
            var obj = new { Sample = new SampleWithLabelAttribute() };
            OclConvert.ToOclDocument(obj)
                .Should()
                .HaveChildrenExactly(
                    new OclBlock("sample", "The Label")
                );
        }

        [Test]
        public void LabelsAreIncludedAsAttributesInTheRootDocument()
        {
            OclConvert.ToOclDocument(new SampleWithLabelAttribute())
                .Should()
                .HaveChildrenExactly(
                    new OclAttribute("my_label", "The Label")
                );
        }

        [Test]
        public void LabelsCanBeDefinedViaConverter()
        {
            var options = new OclSerializerOptions();
            options.Converters.Add(new LowercasingNameLabelConverter());
            var obj = new
            {
                Sample = new SampleWithANameProperty
                {
                    Name = "The Name"
                }
            };

            OclConvert.ToOclDocument(obj, options)
                .Should()
                .HaveChildrenExactly(
                    new OclBlock("sample", "the name")
                );
        }

        class SampleWithLabelAttribute
        {
            [OclLabel]
            public string MyLabel { get; } = "The Label";
        }

        class SampleWithANameProperty
        {
            public string? Name { get; set; }
        }

        class LowercasingNameLabelConverter : DefaultBlockOclConverter
        {
            public override bool CanConvert(Type type)
                => type == typeof(SampleWithANameProperty);

            protected override IEnumerable<string> GetLabels(object obj)
                => new[] { ((SampleWithANameProperty)obj).Name?.ToLower() ?? "" };

            protected override IEnumerable<IOclElement> GetElements(object obj, OclConversionContext context)
                => new IOclElement[0];
        }
    }
}