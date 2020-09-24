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

        public override OclDocument ToDocument(OclConversionContext context, object obj)
        {
            var properties = GetProperties(obj.GetType());
            var children = GetElements(obj, properties, context);
            return new OclDocument(children);
        }

        public override object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue)
        {
            var target = CreateInstance(type, element);

            if (!(element is OclBody body))
                throw new OclException("Cannot convert attribute element");

            if (body is OclBlock block && block.Labels.Any())
                SetLabels(type, block, target);

            SetProperties(context, type, body, target);

            return target;
        }

        protected virtual object CreateInstance(Type type, IOclElement oclElement)
            => Activator.CreateInstance(type)
                ?? throw new OclException("Could not create instance of " + type.Name);

        protected virtual void SetLabels(Type type, OclBlock block, object target)
        {
            var labelProperties = GetLabelProperties(type).ToArray();

            if (block.Labels.Count > labelProperties.Length)
                throw new OclException($"The block '{block.Name}' defines {block.Labels.Count} labels ({string.Join(", ", block.Labels)}) but the type {type.Name} only has {labelProperties.Length} label properties");

            for (var x = 0; x < block.Labels.Count; x++)
                labelProperties[x].SetValue(target, block.Labels[x]);
        }

        protected virtual void SetProperties(OclConversionContext context, Type type, IEnumerable<IOclElement> elements, object target)
        {
            var properties = GetProperties(type).Except(GetLabelProperties(type)).ToArray();

            var notFound = SetProperties(context, elements, target, properties);

            if (notFound.Any())
                throw new OclException($"The propert{(notFound.Count > 1 ? "ies" : "y")} '{string.Join("', '", notFound.Select(a => a.Name))}' {(notFound.Count > 1 ? "were" : "was")} not found on '{type.Name}'");
        }

        protected override IOclElement ConvertInternal(OclConversionContext context, string name, object obj)
            => new OclBlock(
                GetName(context, name, obj),
                GetLabels(obj),
                GetElements(obj, context)
            );

        protected virtual IEnumerable<string> GetLabels(object obj)
        {
            var labels = from p in GetLabelProperties(obj.GetType())
                let labelObj = p.GetValue(obj) ?? throw new OclException($"Labels cannot be null ({p.DeclaringType?.FullName}.{p.Name})")
                let label = labelObj as string ?? throw new Exception($"Labels must be strings ({p.DeclaringType?.FullName}.{p.Name})")
                select label;
            return labels;
        }

        protected virtual IEnumerable<PropertyInfo> GetLabelProperties(Type type)
            => Array.Empty<PropertyInfo>();

        protected virtual IEnumerable<IOclElement> GetElements(object obj, OclConversionContext context)
        {
            var properties = GetProperties(obj.GetType())
                .Except(GetLabelProperties(obj.GetType()))
                .ToArray();
            return GetElements(obj, properties, context);
        }
    }
}