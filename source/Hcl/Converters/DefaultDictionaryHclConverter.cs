using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Octopus.Hcl.Converters
{
    public class DefaultDictionaryHclConverter : IHclConverter
    {
        public bool CanConvert(Type type)
            => typeof(IDictionary).IsAssignableFrom(type);

        public IEnumerable<IHElement> Convert(HclConversionContext context, string name, object value)
        {
            var dictionary = (IDictionary)value;
            var children = new List<IHElement>();

            foreach (var key in dictionary.Keys)
                if (key != null)
                    children.AddRange(context.ToElements(key.ToString() ?? "", dictionary[key]));

            return new[] { new HBlock(name, Array.Empty<string>(), children) };
        }
    }
}