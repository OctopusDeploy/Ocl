using System;

namespace Octopus.Ocl.Converters
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OclDefaultEnumValueAttribute : Attribute
    {
        public OclDefaultEnumValueAttribute(object defaultValue)
        {
            if (!(defaultValue is Enum))
            {
                throw new ArgumentException("defaultValue must be an enum", nameof(defaultValue));
            }

            DefaultValue = defaultValue;
        }

        public object DefaultValue { get; }
    }
}