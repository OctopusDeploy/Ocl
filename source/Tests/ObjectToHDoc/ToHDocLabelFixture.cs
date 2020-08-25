using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Hcl;
using Octopus.Hcl.Converters;

namespace Tests.ObjectToHDoc
{
    public class ToHDocLabelFixture
    {
        [Test]
        public void LabelAttributeOnRootDocumentIsIgnoredAndPropertyIsIncludedAsAChild()
        {
            HclConvert.ToHDocument(new SampleWithLabelAttribute(), new HclSerializerOptions())
                .Should()
                .BeEquivalentTo(
                    new HDocument()
                    {
                        new HAttribute("ALabel", "The Label")
                    },
                    config => config.IncludingAllRuntimeProperties()
                );
        }

        [Test]
        public void ByDefaultLabelsAreIdentifiedViaAttributeAndNotIncludedAsAChild()
        {
            var obj = new { Sample = new SampleWithLabelAttribute() };
            HclConvert.ToHDocument(obj, new HclSerializerOptions())
                .Should()
                .BeEquivalentTo(
                    new HDocument()
                    {
                        new HBlock("Sample", "The Label")
                    },
                    config => config.IncludingAllRuntimeProperties()
                );
        }

        class SampleWithLabelAttribute
        {
            [HclLabel]
            public string ALabel { get; } = "The Label";
        }

        [Test]
        public void LabelsCanBeDefinedViaConverter()
        {
            var options = new HclSerializerOptions();
            options.Converters.Add(new LowercasingNameLabelConverter());
            var obj = new
            {
                Sample = new SampleWithANameProperty
                {
                    Name = "The Name"
                }
            };

            HclConvert.ToHDocument(obj, options)
                .Should()
                .BeEquivalentTo(
                    new HDocument()
                    {
                        new HBlock("Sample", "the name")
                    },
                    config => config.IncludingAllRuntimeProperties()
                );
        }

        class SampleWithANameProperty
        {
            public string? Name { get; set; }
        }

        class LowercasingNameLabelConverter : DefaultBlockHclConverter
        {
            public override bool CanConvert(Type type)
                => type == typeof(SampleWithANameProperty);

            protected override IEnumerable<string> GetLabels(object obj)
                => new[] { ((SampleWithANameProperty)obj).Name?.ToLower() ?? "" };

            protected override IEnumerable<IHElement> GetElements(object obj, HclConversionContext context)
                => new IHElement[0];
        }
    }
}