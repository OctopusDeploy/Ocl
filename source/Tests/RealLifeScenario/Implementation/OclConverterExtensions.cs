using System;
using System.Collections.Generic;
using System.Linq;
using Octopus.Ocl;

namespace Tests.RealLifeScenario.Implementation
{
    public static class OclConverterExtensions
    {
        public static IEnumerable<IOclElement> OrderByThenAlpha(this IEnumerable<IOclElement> elements, params string[] names)
            => from e in elements
                let index = Array.FindIndex(names, n => n.Equals(e.Name, StringComparison.OrdinalIgnoreCase))
                orderby
                    e is OclAttribute,
                    index == -1,
                    index,
                    e.Name
                select e;
    }
}