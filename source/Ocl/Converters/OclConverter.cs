using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Octopus.Ocl.Converters
{
    public abstract class OclConverter : IOclConverter
    {
        /// <summary>
        /// Returns true if the converter can be used for the provided type
        /// </summary>
        /// <param name="type">The model type being converted</param>
        public abstract bool CanConvert(Type type);

        public virtual IEnumerable<IOclElement> ToElements(OclConversionContext context, PropertyInfo? propertyInfo, object obj)
        {
            var element = ConvertInternal(context, propertyInfo, obj);
            return element != null
                ? new[] { element }
                : Array.Empty<IOclElement>();
        }

        /// <summary>
        /// Converts the provided object to an OCL root document.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual OclDocument ToDocument(OclConversionContext context, object obj)
            => throw new NotSupportedException("This type does not support conversion to the OCL root document");

        public abstract object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue);

        protected abstract IOclElement ConvertInternal(OclConversionContext context, PropertyInfo? propertyInfo, object obj);

        protected virtual string GetName(OclConversionContext context, PropertyInfo? propertyInfo, object obj)
            => propertyInfo != null // If the object is the value of a property, then base the name on the property name 
                ? context.Namer.GetName(propertyInfo)
                : context.Namer.FormatName(obj.GetType().Name); // Otherwise base it on the type name

        protected virtual IEnumerable<IOclElement> GetElements(object obj, IEnumerable<PropertyInfo> properties, OclConversionContext context)
        {
            var elements = from p in properties
                from element in PropertyToElements(obj, context, p)
                orderby
                    element is OclBlock,
                    element.Name
                select element;
            return elements;
        }

        protected virtual IEnumerable<IOclElement> PropertyToElements(object obj, OclConversionContext context, PropertyInfo p)
            => context.ToElements(p, p.GetValue(obj));


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

                        propertyToSet.SetValue(target, CoerceValue(context, valueToSet, propertyToSet.PropertyType));
                    }
                }
            }

            return notFound;
        }

        object? CoerceValue(OclConversionContext context, object? sourceValue, Type targetType)
        {
            if (sourceValue is OclStringLiteral literal)
                sourceValue = literal.Value;
            
            if (sourceValue is OclFunctionCall functionCall)
            {
                var result = context.GetFunctionCallFor(functionCall.Name).ToValue(functionCall);
                return CoerceValue(context, result, targetType);
            }

            if (sourceValue == null)
                return null;

            if (targetType.IsInstanceOfType(sourceValue))
                return sourceValue;

            if (sourceValue is Dictionary<string, object?> dict)
            {
                if (targetType.IsAssignableFrom(typeof(Dictionary<string, string>)))
                    return dict.ToDictionary(kvp => kvp.Key, kvp => (string?)CoerceValue(context, kvp.Value, typeof(string)));

                throw new OclException($"Could not coerce dictionary to {targetType.Name}. Only Dictionary<string, string> and Dictionary<string, object?> are supported.");
            }

            if (targetType == typeof(string))
            {
                if (sourceValue.GetType().IsPrimitive)
                    return sourceValue.ToString();

                if (sourceValue is byte[] bytes)
                    return Encoding.UTF8.GetString(bytes);
            }
            
            object? FromArray<T>()
            {
                if (sourceValue is T[] array)
                {
                    if (targetType == typeof(List<T>))
                        return array.ToList();
                    if (targetType == typeof(HashSet<T>))
                        return array.ToHashSet();
                }

                return null;
            }

            return FromArray<string>() ?? FromArray<decimal>() ?? FromArray<int>() ?? throw new Exception($"Could not coerce value of type {sourceValue.GetType().Name} to {targetType.Name}");
        }

        /// <summary>
        /// Get the properties for the given type.
        /// TODO: The virtual accessor can probably be removed and replaced with a ShouldSerialize method that seems to be used.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
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