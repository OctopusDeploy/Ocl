using System;
using System.Collections.Generic;
using System.Reflection;

namespace Octopus.Hcl
{
    public interface IHclConverter
    {
        bool CanConvert(Type type);
        IEnumerable<IHElement> Convert(HclConversionContext context, PropertyInfo name, object value);
    }
}