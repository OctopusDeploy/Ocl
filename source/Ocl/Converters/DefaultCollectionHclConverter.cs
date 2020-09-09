using System;
using System.Collections;
using System.Collections.Generic;

namespace Octopus.Ocl.Converters
{
    public class DefaultCollectionOclConverter : IOclConverter
    {
        public bool CanConvert(Type type)
            => typeof(IEnumerable).IsAssignableFrom(type);

        public IEnumerable<IOclElement> ToOclElements(OclConversionContext context, string name, object value)
        {
            var items = (IEnumerable)value;
            foreach (var item in items)
                if (item != null)
                    foreach (var element in context.ToElements(name, item))
                        yield return element;
        }
    }
}