using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Hcl;
using Octopus.Hcl.Converters;

namespace Tests.Converters
{
    public class DefaultDictionaryHclConverterFixture
    {
        [Test]
        public void CanConvert()
            => new DefaultDictionaryHclConverter().CanConvert(typeof(Dictionary<string, int>))
                .Should()
                .BeTrue();


        [Test]
        public void ConvertPrimitiveDictionary()
        {
            var data = new Dictionary<string, int>()
            {
                { "Key1", 1 },
                { "Key2", 2 },
            };

            var context = new HclConversionContext(new HclSerializerOptions());

            var result = new DefaultDictionaryHclConverter().Convert(context, "MyCollection", data);

            result.Should()
                .BeEquivalentTo(new HBlock("MyCollection")
                {
                    new HAttribute("Key1", 1),
                    new HAttribute("Key2", 3),
                });
        }
    }
}