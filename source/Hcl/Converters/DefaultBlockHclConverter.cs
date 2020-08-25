using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Octopus.Hcl.Converters
{
    public class DefaultBlockHclConverter : HclConverter
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
        {
            var defaultProperties = obj.GetType().GetDefaultMembers().OfType<PropertyInfo>();
            var properties = from p in obj.GetType().GetProperties()
                where p.CanRead
                where p.GetCustomAttribute<HclIgnoreAttribute>() == null
                where p.GetCustomAttribute<HclLabelAttribute>() == null
                select p;

            return GetElements(obj, properties.Except(defaultProperties), context);
        }

    }
}