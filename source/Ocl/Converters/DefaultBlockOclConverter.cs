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

        public override object? FromElement(OclConversionContext context, Type type, IOclElement element, Func<object?> getCurrentValue)
        {
            var target = Activator.CreateInstance(type);
            if (target == null)
                throw new OclException("Could not create instance of " + type.Name);

            if (!(element is OclBody body))
                throw new OclException("Cannot convert attribute element");

            if (body is OclBlock block && block.Labels.Any())
                SetLabels(type, block, target);

            SetProperties(context, type, body, target);

            return target;
        }

        protected virtual void SetLabels(Type type, OclBlock block, object target)
        {
            var labelProperties = GetLabelProperties(type, true).ToArray();

            if (block.Labels.Count > labelProperties.Length)
                throw new OclException($"The block '{block.Name}' defines {block.Labels.Count} labels ({string.Join(", ", block.Labels)}) but the type {type.Name} only has {labelProperties.Length} label properties");

            for (var x = 0; x < block.Labels.Count; x++)
                labelProperties[x].SetValue(target, block.Labels[x]);
        }

        void SetProperties(OclConversionContext context, Type type, OclBody body, object target)
        {
            var properties = GetNonLabelProperties(type, true).ToArray();

            var notFound = SetProperties(context, body, target, properties);

            if (notFound.Any())
                throw new OclException($"The propert{(notFound.Count > 1 ? "ies" : "y")} '{string.Join("', '", notFound.Select(a => a.Name))}' {(notFound.Count > 1 ? "were" : "was")} not found on '{type.Name}'");
        }

        protected override IOclElement ConvertInternal(OclConversionContext context, string name, object obj)
            => new OclBlock(
                GetName(name, obj),
                GetLabels(obj),
                GetElements(obj, context)
            );

        protected virtual IEnumerable<string> GetLabels(object obj)
        {
            var labels = from p in GetLabelProperties(obj.GetType(), false)
                let labelObj = p.GetValue(obj) ?? throw new OclException($"Labels cannot be null ({p.DeclaringType?.FullName}.{p.Name})")
                let label = labelObj as string ?? throw new Exception($"Labels must be strings ({p.DeclaringType?.FullName}.{p.Name})")
                select label;
            return labels;
        }

        static IEnumerable<PropertyInfo> GetLabelProperties(Type type, bool forWriting)
            => from p in type.GetProperties()
                where p.CanRead
                where !forWriting || p.CanWrite
                let attr = p.GetCustomAttribute<OclLabelAttribute>()
                where attr != null
                orderby attr.Ordinal
                select p;

        protected virtual IEnumerable<IOclElement> GetElements(object obj, OclConversionContext context)
            => GetElements(obj, GetNonLabelProperties(obj.GetType(), false), context);
    }
}