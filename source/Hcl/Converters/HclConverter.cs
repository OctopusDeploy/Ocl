using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Octopus.Hcl.Converters
{
    public abstract class HclConverter : IHclConverter
    {
        public abstract bool CanConvert(Type type);

        public IEnumerable<IHElement> Convert(HclConversionContext context, string name, object obj)
            => new[] { ConvertInternal(context, name, obj) };

        protected abstract IHElement ConvertInternal(HclConversionContext context, string name, object obj);

        protected virtual string GetName(string name, object obj)
            => name;

        protected virtual IEnumerable<IHElement> GetElements(object obj, IEnumerable<PropertyInfo> properties, HclConversionContext context)
        {
            var elements = from p in properties
                let attr = p.GetCustomAttribute<HclElementAttribute>() ?? new HclElementAttribute()
                from element in context.ToElements(attr.Name ?? p.Name, p.GetValue(obj))
                orderby attr.Ordinal, element is HBlock, element.Name
                select element;
            return elements;
        }
    }
}