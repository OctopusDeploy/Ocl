using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Octopus.Ocl.Converters;

namespace Octopus.Ocl.Namers
{
    public abstract class BaseNamer : IOclNamer
    {
        public abstract string FormatName(string name);

        public string GetName(PropertyInfo propertyInfo)
            // The OclAttribute attribute can be applied to properties to control the OCL attribute name
            => propertyInfo.GetCustomAttribute(typeof(OclNameAttribute)) is OclNameAttribute oclAttribute && !string.IsNullOrEmpty(oclAttribute.Name)
                ? oclAttribute.Name
                : FormatName(propertyInfo.Name);

        public PropertyInfo? GetProperty(string name, IReadOnlyCollection<PropertyInfo> properties)
        {
            var matches = properties
                .Where(p => p.GetCustomAttribute(typeof(OclNameAttribute)) is OclNameAttribute oclAttribute
                    && !string.IsNullOrEmpty(oclAttribute.Name) && oclAttribute.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (matches.Length == 1)
                return matches[0];
            
            if (matches.Length > 1)
                throw new OclException($"Multiple properties match the name '{name}': {string.Join(", ", matches.Select(m => m.Name))}");

            return GetPropertyInternal(name, properties);
        }

        protected abstract PropertyInfo? GetPropertyInternal(string name, IReadOnlyCollection<PropertyInfo> properties);
    }
}