using System;
using System.Collections.Generic;
using System.Reflection;
using Octopus.Ocl.Namers;

namespace Octopus.Ocl.Converters
{
    public class EnumAttributeOclConverter : IOclConverter
    {
        public bool CanConvert(Type type)
            => type.IsEnum;

        public OclDocument ToDocument(OclConversionContext context, object obj)
            => throw new NotSupportedException("This type does not support conversion to the OCL root document");

        public object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue)
        {
            if (element is OclAttribute attribute)
            {
                if (attribute.Value == null)
                    return null;

                if (attribute.Value is string str)
                    return Enum.Parse(type, str);

                if (attribute.Value is OclStringLiteral strLit)
                    return Enum.Parse(type, strLit.Value);

                throw new Exception("Enum values must be specified as a string");
            }

            throw new OclException("Can only convert attribute elements");
        }

        public IEnumerable<IOclElement> ToElements(OclConversionContext context, PropertyInfo? propertyInfo, object obj)
        {
            var isDefault = Activator.CreateInstance(obj.GetType()).Equals(obj);
            if (!isDefault)
                yield return new OclAttribute(context.Namer.GetName(propertyInfo!), obj.ToString());
        }
    }
}