using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Octopus.Ocl.Converters
{
    public abstract class OclConverter : IOclConverter
    {
        public abstract bool CanConvert(Type type);

        public virtual IEnumerable<IOclElement> ToElements(OclConversionContext context, string name, object obj)
            => new[] { ConvertInternal(context, name, obj) };

        public virtual OclDocument ToDocument(OclConversionContext context, object obj)
            => throw new NotSupportedException("This type does not support conversion to the OCL root document");

        public abstract object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue);

        protected abstract IOclElement ConvertInternal(OclConversionContext context, string name, object obj);

        protected virtual string GetName(OclConversionContext context, string name, object obj)
            => context.Namer.FormatName(name);

        protected virtual IEnumerable<IOclElement> GetElements(object obj, IEnumerable<PropertyInfo> properties, OclConversionContext context)
        {
            var elements = from p in properties
                from element in context.ToElements(p.Name, p.GetValue(obj))
                orderby
                    element is OclBlock,
                    element.Name
                select element;
            return elements;
        }

        protected virtual IReadOnlyList<IOclElement> SetProperties(
            OclConversionContext context,
            IEnumerable<IOclElement> elements,
            object target,
            IReadOnlyList<PropertyInfo> properties)
        {
            var notFound = new List<IOclElement>();
            foreach (var element in elements)
            {
                var name = element.Name?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    throw new OclException("Encountered invalid child: " + element.GetType());

                var propertyToSet = context.Namer.GetProperty(name, properties);
                if (propertyToSet == null)
                {
                    notFound.Add(element);
                }
                else
                {
                    var currentValue = propertyToSet.GetValue(target);
                    var valueToSet = context.FromElement(propertyToSet.PropertyType, element, currentValue);

                    if (currentValue != valueToSet)
                    {
                        if (!propertyToSet.CanWrite)
                            throw new OclException($"The property '{propertyToSet.Name}' on '{target.GetType().Name}' does not have a setter");

                        propertyToSet.SetValue(target, CoerceValue(valueToSet, propertyToSet.PropertyType));
                    }
                }
            }

            return notFound;
        }

        object? CoerceValue(object? valueToSet, Type type)
        {
            if (valueToSet is OclStringLiteral literal)
                valueToSet = literal.Value;

            if (valueToSet == null)
                return null;

            if (type.IsInstanceOfType(valueToSet))
                return valueToSet;

            if (valueToSet is Dictionary<string, object?> dict)
            {
                if (type.IsAssignableFrom(typeof(Dictionary<string, string>)))
                    return dict.ToDictionary(kvp => kvp.Key, kvp => (string?)CoerceValue(kvp.Value, typeof(string)));

                throw new OclException($"Could not coerce dictionary to {type.Name}. Only Dictionary<string, string> and Dictionary<string, object?> are supported.");
            }

            if (type == typeof(string) && valueToSet.GetType().IsPrimitive)
                return valueToSet.ToString();

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
                select p;

            return properties.Except(defaultProperties);
        }
    }
}