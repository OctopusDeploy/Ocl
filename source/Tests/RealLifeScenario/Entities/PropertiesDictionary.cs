using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.RealLifeScenario.Entities
{
    public class PropertiesDictionary : Dictionary<string, PropertyValue>
    {
        public PropertiesDictionary() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public PropertiesDictionary(IDictionary<string, PropertyValue> values)
            : base(values, StringComparer.OrdinalIgnoreCase)
        {
        }

        public PropertiesDictionary(params IDictionary<string, PropertyValue>[] propertiesDictionaries)
            : base(Merge(propertiesDictionaries))
        {
        }

        static IDictionary<T, U> Merge<T, U>(IDictionary<T, U>[] dictionaries)
            where T : notnull
        {
            return dictionaries.SelectMany(dict => dict)
                .ToLookup(pair => pair.Key, pair => pair.Value)
                .ToDictionary(group => group.Key, group => group.First());
        }
    }
}