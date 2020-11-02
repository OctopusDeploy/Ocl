using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Octopus.Data.Model;
using Octopus.Ocl;
using Octopus.TinyTypes;

namespace Tests.RealLifeScenario.Implementation
{
    public class TinyTypeReferenceCollectionOclConverter : IOclConverter
    {
        public bool CanConvert(Type type)
            => GetUnderlyingTypeOfReferenceCollection(type)?.IsTinyType() ?? false;

        static Type? GetUnderlyingTypeOfReferenceCollection(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ReferenceCollection<>))
                return type.GenericTypeArguments[0];

            return type.BaseType == null ? null : GetUnderlyingTypeOfReferenceCollection(type.BaseType);
        }

        public IEnumerable<IOclElement> ToElements(OclConversionContext context, string name, object value)
        {
            var values = ((IEnumerable) value)
                .Cast<object>()
                .Select(v => v.ToString())
                .ToArray();

            if (values.Length == 0)
                return Array.Empty<IOclElement>();

            return new[]
            {
                new OclAttribute(context.Namer.FormatName(name), values)
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

            dynamic collection = currentValue ?? Activator.CreateInstance(type, new object?[] { null })!;
            var tinyTypeType = GetUnderlyingTypeOfReferenceCollection(collection.GetType());

            foreach (var value in valuesToAdd)
                collection.Add(Activator.CreateInstance(tinyTypeType, value));

            return (object) collection;
        }
    }
}