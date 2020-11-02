using System;
using System.Collections.Generic;
using System.Linq;
using Octopus.Ocl;
using Octopus.Server.Extensibility.HostServices.Model;
using Tests.RealLifeScenario.Entities;

namespace Tests.RealLifeScenario.ConverterStrategy.Implementation
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

            var attributes =
                from kvp in dict
                let key = kvp.Key.Replace(".", "_")
                where kvp.Value.HasValue
                select new OclAttribute(key, kvp.Value.Value);

            yield return new OclBlock("properties", Array.Empty<string>(), attributes);
        }

        public OclDocument ToDocument(OclConversionContext context, object obj)
            => throw new NotSupportedException();

        public object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue)
        {
            if (!(element is OclBlock block))
                throw new OclException("The properties must be a block");

            var dict = currentValue == null ? new PropertiesDictionary() : (PropertiesDictionary)currentValue;

            foreach (var child in block)
            {
                if (!(child is OclAttribute attr))
                    throw new OclException("The properties block may only contain attributes");

                var key = attr.Name.Replace("_", ".");

                if (dict.ContainsKey(key))
                    throw new OclException($"The properties {attr.Name} has been specified more than once");

                dict[key] = attr.Value is OclStringLiteral lit
                    ? new PropertyValue(lit.Value)
                    : new PropertyValue(attr.Value?.ToString() ?? "");
            }

            return dict;
        }
    }
}