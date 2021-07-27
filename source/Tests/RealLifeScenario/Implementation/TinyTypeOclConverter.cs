using System;
using System.Collections.Generic;
using System.Reflection;
using Octopus.Ocl;
using Octopus.TinyTypes;

namespace Tests.RealLifeScenario.Implementation
{
    public class TinyTypeOclConverter : IOclConverter
    {
        public bool CanConvert(Type type)
            => type.IsTinyType();

        public IEnumerable<IOclElement> ToElements(OclConversionContext context, PropertyInfo? propertyInfo, object value)
            => new[]
            {
                new OclAttribute(context.Namer.FormatName(propertyInfo!.Name), value.ToString())
            };

        public OclDocument ToDocument(OclConversionContext context, object obj)
            => throw new NotSupportedException();

        public object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue)
        {
            if (!(element is OclAttribute attrib))
                throw new OclException($"The {element.Name} element must be an attribute");

            if (attrib.Value is string str)
                return Activator.CreateInstance(type, str);

            throw new Exception($"The {element.Name} attribute must have a string value");
        }
    }
}