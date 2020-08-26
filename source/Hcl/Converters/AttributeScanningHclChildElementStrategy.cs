using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Octopus.Hcl.Converters
{
    public class AttributeScanningHclChildElementStrategy : IHclChildElementStrategy
    {
        public virtual IEnumerable<IHElement> GetElements(object obj, HclConversionContext context)
            => GetElements(
                obj,
                from p in obj.GetType().GetProperties()
                where p.CanRead
                where p.GetCustomAttribute<HclIgnoreAttribute>() == null
                where p.GetCustomAttribute<HclLabelAttribute>() == null
                select p,
                context
            );

        protected virtual IEnumerable<IHElement> GetElements(object obj, IEnumerable<PropertyInfo> properties, HclConversionContext context)
        {
            var elements = from p in properties
                let attr = p.GetCustomAttribute<HclElementAttribute>() ?? new HclElementAttribute()
                from element in context.ToElements(attr.Name ?? p.Name, p.GetValue(obj))
                orderby attr.Ordinal, element is HAttribute, element.Name
                select element;
            return elements;
        }
    }
}