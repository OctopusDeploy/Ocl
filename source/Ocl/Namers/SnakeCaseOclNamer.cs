using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Octopus.Ocl.Namers
{
    public class SnakeCaseOclNamer : BaseNamer
    {
        public override string FormatName(string name)
            => Regex.Replace(name, "([a-z])([A-Z])", "$1_$2")
                .Replace(" ", "_")
                .ToLower();

        protected override PropertyInfo? GetPropertyInternal(string name, IReadOnlyCollection<PropertyInfo> properties)
        {
            var nameWithoutUnderscores = name.Replace("_", "");
            var matches = properties.Where(p => p.Name.Equals(nameWithoutUnderscores, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matches.Length > 1)
                throw new OclException($"Multiple properties match the name '{name}': {string.Join(", ", matches.Select(m => m.Name))}");

            return matches.FirstOrDefault();
        }
    }
}