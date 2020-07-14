using System;
using System.Collections.Generic;

namespace Octopus.Hcl
{
    public interface IHclConverter
    {
        bool CanConvert(Type type);
        IEnumerable<IHElement> Convert(HclConversionContext context, string name, object value);
    }
}