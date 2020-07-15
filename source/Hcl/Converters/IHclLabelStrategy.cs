using System;
using System.Collections.Generic;

namespace Octopus.Hcl.Converters
{
    public interface IHclLabelStrategy
    {
        IEnumerable<string> GetLabels(object obj);
    }
}