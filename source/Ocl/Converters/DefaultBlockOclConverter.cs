using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Octopus.Ocl.Converters
{
    public class DefaultBlockOclConverter : OclConverter
    {
        public override bool CanConvert(Type type)
            => !OclAttribute.IsSupportedValueType(type);

        protected override IOclElement ConvertInternal(OclConversionContext context, string name, object obj)
            => new OclBlock(
                GetName(name, obj),
                GetLabels(obj),
                GetElements(obj, context)
            );

        protected virtual IEnumerable<string> GetLabels(object obj)
        {
            var labels = from p in obj.GetType().GetProperties()
                where p.CanRead
                let attr = p.GetCustomAttribute<OclLabelAttribute>()
                where attr != null
                orderby attr.Ordinal
                let labelObj = p.GetValue(obj) ?? throw new OclException($"Labels cannot be null ({p.DeclaringType?.FullName}.{p.Name})")
                let label = labelObj as string ?? throw new Exception($"Labels must be strings ({p.DeclaringType?.FullName}.{p.Name})")
                select label;
            return labels;
        }

        protected virtual IEnumerable<IOclElement> GetElements(object obj, OclConversionContext context)
        {
            var defaultProperties = obj.GetType().GetDefaultMembers().OfType<PropertyInfo>();
            var properties = from p in obj.GetType().GetProperties()
                where p.CanRead
                where p.GetCustomAttribute<OclIgnoreAttribute>() == null
                where p.GetCustomAttribute<OclLabelAttribute>() == null
                select p;

            return GetElements(obj, properties.Except(defaultProperties), context);
        }
    }
}