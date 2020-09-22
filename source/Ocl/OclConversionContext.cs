using System;
using System.Collections.Generic;
using System.Linq;
using Octopus.Ocl.Converters;
using Octopus.Ocl.Namers;

namespace Octopus.Ocl
{
    public class OclConversionContext
    {
        readonly IOclConverter[] converters;

        internal OclConversionContext(OclSerializerOptions options)
        {
            converters = options
                .Converters
                .Concat(
                    new IOclConverter[]
                    {
                        new DefaultAttributeOclConverter(),
                        new DefaultCollectionOclConverter(),
                        new DefaultBlockOclConverter()
                    })
                .ToArray();
            Namer = options.Namer;
        }

        public IOclNamer Namer { get; }

        public IEnumerable<IOclElement> ToElements(string name, object? value)
        {
            if (value == null)
                return new IOclElement[0];

            foreach (var converter in converters)
                if (converter.CanConvert(value.GetType()))
                    return converter.ToElements(this, name, value);

            throw new Exception("Could not find a converter for " + value.GetType().FullName);
        }
    }
}