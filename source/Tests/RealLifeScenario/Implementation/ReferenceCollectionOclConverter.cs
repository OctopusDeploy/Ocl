using System;
using System.Collections.Generic;
using System.Linq;
using Octopus.Data.Model;
using Octopus.Ocl;

namespace Tests.RealLifeScenario.Implementation
{
    public class ReferenceCollectionOclConverter : IOclConverter
    {
        public bool CanConvert(Type type)
            => type == typeof(ReferenceCollection);

        public IEnumerable<IOclElement> ToElements(OclConversionContext context, string name, object value)
        {
            var collection = (ReferenceCollection)value;

            if (collection.Count == 0)
                return Array.Empty<IOclElement>();

            return new[]
            {
                new OclAttribute(context.Namer.FormatName(name), collection.ToArray())
            };
        }

        public OclDocument ToDocument(OclConversionContext context, object obj)
            => throw new NotSupportedException();

        public object FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue)
        {
            if (!(element is OclAttribute attrib))
                throw new OclException($"The {element.Name} element must be an attribute");

            if (!(attrib.Value is string[] valuesToAdd))
                throw new Exception($"The {element.Name} attribute must have a string array value");

            var collection = currentValue == null ? new ReferenceCollection() : (ReferenceCollection)currentValue;

            collection.ReplaceAll(valuesToAdd);

            return collection;
        }
    }
}