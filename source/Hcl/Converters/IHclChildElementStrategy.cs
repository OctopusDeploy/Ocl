using System;
using System.Collections.Generic;

namespace Octopus.Hcl.Converters
{
    public interface IHclChildElementStrategy
    {
        IEnumerable<IHElement> GetElements(object obj, HclConversionContext context);
    }
}