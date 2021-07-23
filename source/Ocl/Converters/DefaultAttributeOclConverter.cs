using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Octopus.Ocl.Namers;

namespace Octopus.Ocl.Converters
{
    public class DefaultAttributeOclConverter : IOclConverter
    {
        public bool CanConvert(Type type)
            => OclAttribute.IsSupportedValueType(type);

        public OclDocument ToDocument(OclConversionContext context, object obj)
            => throw new NotSupportedException("This type does not support conversion to the OCL root document");

        public object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue)
        {
            if (element is OclAttribute attribute)
                return attribute.Value;

            throw new OclException("Can only convert attribute elements");
        }

        public IEnumerable<IOclElement> ToElements(OclConversionContext context, PropertyInfo? propertyInfo, object obj)
        {
            var type = obj.GetType();

            if (!type.IsValueType && type == null)
                yield break;

            if (type.IsValueType)
                if (obj.Equals(false) || obj.Equals(0) || obj.Equals(0m) || obj.Equals(0f) || obj.Equals('\0'))
                    yield break;

            if (!(obj is string) && obj is IEnumerable enumerable)
                if (!enumerable.GetEnumerator().MoveNext())
                    yield break;

            yield return new OclAttribute(context.Namer.GetOclAttributeName(propertyInfo!), obj);
        }
    }
}