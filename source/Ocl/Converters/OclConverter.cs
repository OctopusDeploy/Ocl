using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Octopus.Ocl.Converters
{
    public abstract class OclConverter : IOclConverter
    {
        public abstract bool CanConvert(Type type);

        public IEnumerable<IOclElement> ToElements(OclConversionContext context, string name, object obj)
            => new[] { ConvertInternal(context, name, obj) };

        protected abstract IOclElement ConvertInternal(OclConversionContext context, string name, object obj);

        protected virtual string GetName(OclConversionContext context, string name, object obj)
            => context.Namer.FormatName(name);

        protected virtual IEnumerable<IOclElement> GetElements(object obj, IEnumerable<PropertyInfo> properties, OclConversionContext context)
        {
            var elements = from p in properties
                let attr = p.GetCustomAttribute<OclElementAttribute>() ?? new OclElementAttribute()
                from element in context.ToElements(attr.Name ?? p.Name, p.GetValue(obj))
                orderby attr.Ordinal, element is OclBlock, element.Name
                select element;
            return elements;
        }
    }
}