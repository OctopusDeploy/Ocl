using System;
using System.Collections.Generic;

namespace Octopus.Hcl.Converters
{
    public class DefaultAttributeHclConverter : HclConverter
    {
        public override bool CanConvert(Type type)
            => HAttribute.IsSupportedValueType(type);

        protected override IHElement ConvertInternal(HclConversionContext context, string name, object obj)
             => new HAttribute(GetName(name, obj), obj);
    }
}