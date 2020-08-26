using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Octopus.Hcl.Converters
{
    public class DefaultCollectionHclConverter : IHclConverter
    {
        public bool CanConvert(Type type)
            => typeof(IEnumerable).IsAssignableFrom(type);

        public IEnumerable<IHElement> Convert(HclConversionContext context, PropertyInfo property, object value)
        {
            var items = (IEnumerable)value;
            foreach (var item in items)
                if (item != null)
                    foreach (var element in context.ToElements(property, item))
                        yield return element;
        }
    }
}