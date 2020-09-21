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

        public virtual OclDocument ToDocument(OclConversionContext context, object obj)
            => throw new NotSupportedException("This type does not support conversion to the OCL root document");

        public abstract object? FromElement(OclConversionContext context, Type type, IOclElement element, Func<object?> getCurrentValue);

        protected abstract IOclElement ConvertInternal(OclConversionContext context, string name, object obj);

        protected virtual string GetName(string name, object obj)
            => name;

        protected virtual IEnumerable<IOclElement> GetElements(object obj, IEnumerable<PropertyInfo> properties, OclConversionContext context)
        {
            var elements = from p in properties
                let attr = p.GetCustomAttribute<OclElementAttribute>() ?? new OclElementAttribute()
                from element in context.ToElements(attr.Name ?? p.Name, p.GetValue(obj))
                orderby
                    attr.Ordinal,
                    element is OclBlock,
                    element.Name
                select element;
            return elements;
        }

        protected virtual IReadOnlyList<IOclElement> SetProperties(
            OclConversionContext context,
            OclBody body,
            object target,
            IEnumerable<PropertyInfo> properties)
        {
            var propertyLookup = properties.ToDictionary(p => p.Name);
            var notFound = new List<IOclElement>();
            foreach (var child in body)
            {
                var name = child.Name?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    throw new OclException("Encountered invalid child: " + child.GetType());

                if (propertyLookup.TryGetValue(name, out var property))
                {
                    var valueToSet = context.FromElement(property.PropertyType, child, () => property.GetValue(target));
                    property.SetValue(target, CoerceValue(valueToSet, property.PropertyType));
                }
                else
                {
                    notFound.Add(child);
                }
            }

            return notFound;
        }

        object? CoerceValue(object? valueToSet, Type type)
        {
            if (valueToSet == null)
                return null;

            if (type.IsInstanceOfType(valueToSet))
                return valueToSet;

            object? FromArray<T>()
            {
                if (valueToSet is T[] array)
                {
                    if (type == typeof(List<T>))
                        return array.ToList();
                    if (type == typeof(HashSet<T>))
                        return array.ToHashSet();
                }

                return null;
            }

            return FromArray<string>() ?? FromArray<decimal>() ?? FromArray<int>() ?? throw new Exception($"Could not coerce value of type {valueToSet.GetType().Name} to {type.Name}");
        }

        protected virtual IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            var defaultProperties = type.GetDefaultMembers().OfType<PropertyInfo>();
            var properties = from p in type.GetProperties()
                where p.CanRead
                where p.GetCustomAttribute<OclIgnoreAttribute>() == null
                select p;

            return properties.Except(defaultProperties);
        }
    }
}