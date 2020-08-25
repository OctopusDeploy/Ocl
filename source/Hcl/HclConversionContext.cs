using System;
using System.Collections.Generic;
using System.Linq;
using Octopus.Hcl.Converters;

namespace Octopus.Hcl
{
    public class HclConversionContext
    {
        private readonly IHclConverter[] converters;

        internal HclConversionContext(HclSerializerOptions options)
        {
            converters = options
                .Converters
                .Concat(
                    new IHclConverter[]
                    {
                        new DefaultAttributeHclConverter(),
                        new DefaultDictionaryHclConverter(),
                        new DefaultCollectionHclConverter(),
                        new DefaultBlockHclConverter(),
                    })
                .ToArray();
        }


        public IEnumerable<IHElement> ToElements(string name, object? value)
        {
            if (value == null)
                return new[] { new HAttribute(name, null) };

            foreach (var converter in converters)
                if (converter.CanConvert(value.GetType()))
                    return converter.Convert(this, name, value);

            throw new Exception("Could not find a converter for " + value.GetType().FullName);
        }
    }
}