using System;
using System.Collections.Generic;

namespace Octopus.Ocl
{
    public interface IOclConverter
    {
        bool CanConvert(Type type);
        IEnumerable<IOclElement> ToOclElements(OclConversionContext context, string name, object value);
    }
}