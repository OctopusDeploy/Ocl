using System;
using System.Collections.Generic;

namespace Octopus.Ocl.Converters
{
    public class DefaultAttributeOclConverter : OclConverter
    {
        public override bool CanConvert(Type type)
            => OclAttribute.IsSupportedValueType(type);

        public override object? FromElement(OclConversionContext context, Type type, IOclElement element, Func<object?> getCurrentValue)
        {
            if (element is OclAttribute attribute)
                return attribute.Value;

            throw new OclException("Can only convert attribute elements");
        }

        protected override IOclElement ConvertInternal(OclConversionContext context, string name, object obj)
             => new OclAttribute(GetName(name, obj), obj);
    }
}