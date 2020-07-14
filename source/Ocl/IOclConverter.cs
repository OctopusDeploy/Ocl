using System;
using System.Collections.Generic;

namespace Octopus.Ocl
{
    public interface IOclConverter
    {
        bool CanConvert(Type type);
        IEnumerable<IOclElement> Convert(OclConversionContext context, string name, object value);
    }
}