using System;
using System.Collections.Generic;

namespace Octopus.Ocl
{
    public interface IOclConverter
    {
        bool CanConvert(Type type);
        IEnumerable<IOclElement> ToElements(OclConversionContext context, string name, object value);
        OclDocument ToDocument(OclConversionContext context, object obj);
        object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue);
    }
}