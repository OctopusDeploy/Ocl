using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Octopus.Ocl.Converters;
using Octopus.Ocl.Namers;

namespace Octopus.Ocl
{
    public class OclConversionContext
    {
        readonly IReadOnlyList<IOclConverter> converters;

        public OclConversionContext(OclSerializerOptions options)
        {
            converters = options
                .Converters
                .Concat(
                    new IOclConverter[]
                    {
                        new EnumAttributeOclConverter(),
                        new DefaultAttributeOclConverter(),
                        new DefaultCollectionOclConverter(),
                        new DefaultBlockOclConverter()
                    })
                .ToArray();
            Namer = options.Namer;
        }

        internal IOclNamer Namer { get; }

        public IOclConverter GetConverterFor(Type type)
        {
            foreach (var converter in converters)
                if (converter.CanConvert(type))
                    return converter;

            throw new Exception("Could not find a converter for " + type.FullName);
        }

        internal IEnumerable<IOclElement> ToElements(PropertyInfo? propertyInfo, object? value)
        {
            if (value == null)
                return new IOclElement[0];

            return GetConverterFor(value.GetType())
                .ToElements(this, propertyInfo, value);
        }

        internal object? FromElement(Type type, IOclElement element, object? getCurrentValue)
            => GetConverterFor(type)
                .FromElement(this, type, element, getCurrentValue);
    }
}