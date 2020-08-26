using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Octopus.Hcl.Converters
{
    public interface IHclNamingStrategy
    {
        string GetName(PropertyInfo property, object obj);
    }
    
    public class BlockHclConverter : HclConverter
    {
        public override bool CanConvert(Type type)
            => !HAttribute.IsSupportedValueType(type);
        
        protected override IHElement ConvertInternal(HclConversionContext context, string name, object obj)
            => new HBlock(
                GetName(name, obj), 
                GetLabels(obj), 
                GetElements(obj, context)
            );
        
        protected virtual IEnumerable<string> GetLabels(object obj)
        {
            var labels = from p in obj.GetType().GetProperties()
                where p.CanRead
                let attr = p.GetCustomAttribute<HclLabelAttribute>()
                where attr != null
                orderby attr.Ordinal
                let labelObj = p.GetValue(obj) ?? throw new HclException($"Labels cannot be null ({p.DeclaringType?.FullName}.{p.Name})")
                let label = labelObj as string ?? throw new Exception($"Labels must be strings ({p.DeclaringType?.FullName}.{p.Name})")
                select label;
            return labels;
        }
        
        protected virtual IEnumerable<IHElement> GetElements(object obj, HclConversionContext context)
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