using System;
using System.Collections.Generic;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.ObjectToOclDoc
{
    public class ToOclDocNamingFixture
    {
        [Test]
        public void TheDefaultNameIsThePropertyName()
        {
            var obj = new { Sample = "My Value" };
            OclConvert.ToOclDocument(obj, new OclSerializerOptions())
                .Should()
                .Be(
                    new OclDocument()
                    {
                        new OclAttribute("Sample", "My Value")
                    }
                );
        }

        [Test]
        public void TheNameFromTheOclElementAttributeIsUsed()
        {
            var obj = new SampleWithElementNameAttribute();
            OclConvert.ToOclDocument(obj, new OclSerializerOptions())
                .Should()
                .Be(
                    new OclDocument()
                    {
                        new OclAttribute("Custom Name", "The Label")
                    }
                );
        }

        class SampleWithElementNameAttribute
        {
            [OclElement(Name = "Custom Name")]
            public string ALabel { get; } = "The Label";
        }

        [Test]
        public void BlockNamesCanBeDerivedFromTheBlocksTypeOrData()
        {
            var options = new OclSerializerOptions();
            options.Converters.Add(new DefaultBlockNameFromNamePropertyConverter());
            var obj = new
            {
                Sample = new SampleWithANameProperty
                {
                    Name = "The Name"
                }
            };

            OclConvert.ToOclDocument(obj, options)
                .Should()
                .Be(
                    new OclDocument()
                    {
                        new OclBlock("The Name")
                    }
                );
        }

        class SampleWithANameProperty
        {
            public string? Name { get; set; }
        }

        class DefaultBlockNameFromNamePropertyConverter : DefaultBlockOclConverter
        {
            public override bool CanConvert(Type type)
                => type == typeof(SampleWithANameProperty);

            protected override string GetName(string name, object obj)
                => ((dynamic)obj).Name;

            protected override IEnumerable<IOclElement> GetElements(object obj, OclConversionContext context)
                => new IOclElement[0];
        }

    }
}