using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Hcl;
using Octopus.Hcl.Converters;

namespace Tests.ObjectToHDoc
{
    public class ToHDocNamingFixture
    {
        [Test]
        public void TheDefaultNameIsThePropertyName()
        {
            var obj = new { Sample = "My Value" };
            HclConvert.ToHDocument(obj, new HclSerializerOptions())
                .Should()
                .BeEquivalentTo(
                    new HDocument()
                    {
                        new HAttribute("Sample", "My Value")
                    },
                    config => config.IncludingAllRuntimeProperties()
                );
        }

        [Test]
        public void TheNameFromTheHclElementAttributeIsUsed()
        {
            var obj = new SampleWithElementNameAttribute();
            HclConvert.ToHDocument(obj, new HclSerializerOptions())
                .Should()
                .BeEquivalentTo(
                    new HDocument()
                    {
                        new HAttribute("Custom Name", "The Label")
                    },
                    config => config.IncludingAllRuntimeProperties()
                );
        }

        class SampleWithElementNameAttribute
        {
            [HclElement(Name = "Custom Name")]
            public string ALabel { get; } = "The Label";
        }

        [Test]
        public void BlockNamesCanBeDerivedFromTheBlocksTypeOrData()
        {
            var options = new HclSerializerOptions();
            options.Converters.Add(new DefaultBlockNameFromNamePropertyConverter());
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
                        new HBlock("The Name")
                    },
                    config => config.IncludingAllRuntimeProperties()
                );
        }

        class SampleWithANameProperty
        {
            public string? Name { get; set; }
        }

        class DefaultBlockNameFromNamePropertyConverter : DefaultBlockHclConverter
        {
            public override bool CanConvert(Type type)
                => type == typeof(SampleWithANameProperty);

            protected override string GetName(string name, object obj)
                => ((dynamic)obj).Name;

            protected override IEnumerable<IHElement> GetElements(object obj, HclConversionContext context)
                => new IHElement[0];
        }

    }
}