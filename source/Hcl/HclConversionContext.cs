using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                        new DefaultCollectionHclConverter(),
                        new BlockHclConverter(),
                    })
                .ToArray();
        }
        
         
        public IEnumerable<IHElement> ToElements(PropertyInfo property, object? value)
        {
            if (value == null)
                yield return new[] { new HAttribute(name, null) };

            foreach (var converter in converters)
                if (converter.CanConvert(value.GetType()))
                    return converter.Convert(this, property, value);

            throw new Exception("Could not find a converter for " + value.GetType().FullName);
        }
    }
}