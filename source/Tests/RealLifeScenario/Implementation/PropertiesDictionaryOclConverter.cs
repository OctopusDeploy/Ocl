using System;
using System.Collections.Generic;
using System.Linq;
using Octopus.Ocl;
using Octopus.Server.Extensibility.HostServices.Model;
using Tests.RealLifeScenario.Entities;

namespace Tests.RealLifeScenario.Implementation
{
    public class PropertiesDictionaryOclConverter : IOclConverter
    {
        public bool CanConvert(Type type)
            => type == typeof(PropertiesDictionary);

        public IEnumerable<IOclElement> ToElements(OclConversionContext context, string name, object value)
        {
            var dict = (PropertiesDictionary)value;

            if (dict.None())
                yield break;

            if (dict.Values.Any(v => v.IsSensitive))
                throw new NotSupportedException("Sensitive property values are not supported");

            var stringDict = dict.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Value
            );

            yield return new OclAttribute("properties", stringDict);
        }

        public OclDocument ToDocument(OclConversionContext context, object obj)
            => throw new NotSupportedException();

        public object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue)
        {
            if (!(element is OclAttribute attribute))
                throw new OclException("The properties must be an attribute");

            var properties = currentValue == null ? new PropertiesDictionary() : (PropertiesDictionary)currentValue;

            if (!(attribute.Value is Dictionary<string, object?> source))
                throw new OclException("The properties attribute value must be a dictionary");

            foreach (var item in source)
            {
                var itemValue = item.Value is OclStringLiteral lit ? lit.Value : item.Value?.ToString();
                if (itemValue != null)
                    properties[item.Key] = new PropertyValue(itemValue);
            }

            return properties;
        }
    }
}