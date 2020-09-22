using System;

namespace Octopus.Ocl.Converters
{
    public class EnumAttributeOclConverter : OclConverter
    {
        public override bool CanConvert(Type type)
            => type.IsEnum;

        public override object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue)
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

        protected override IOclElement ConvertInternal(OclConversionContext context, string name, object obj)
            => new OclAttribute(GetName(context, name, obj), obj.ToString());
    }
}