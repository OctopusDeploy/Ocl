using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.ToOclDoc
{
    public class ToOclDocNamingFixture
    {
        [Test]
        public void TheDefaultNameIsThePropertyName()
        {
            var obj = new { Sample = "My Value" };
            new OclSerializer().ToOclDocument(obj)
                .Should()
                .Be(
                    new OclDocument
                    {
                        new OclAttribute("sample", "My Value")
                    }
                );
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

            new OclSerializer(options).ToOclDocument(obj)
                .Should()
                .Be(
                    new OclDocument
                    {
                        new OclBlock("the_name")
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

            protected override string GetName(OclConversionContext context, PropertyInfo? propertyInfo, object obj)
                => context.Namer.FormatName(((dynamic)obj).Name);

            protected override IEnumerable<IOclElement> GetElements(object obj, OclConversionContext context)
                => new IOclElement[0];
        }
    }
}