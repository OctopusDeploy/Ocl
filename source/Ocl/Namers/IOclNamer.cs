using System;
using System.Collections.Generic;
using System.Reflection;

namespace Octopus.Ocl.Namers
{
    public interface IOclNamer
    {
        /// <summary>
        /// Formats a name to something OCL friendly
        /// </summary>
        /// <param name="name">The name of the element</param>
        /// <returns>The name to be written out to the OCL document</returns>
        string FormatName(string name);

        /// <summary>
        /// Gets the OCL name for the given property 
        /// </summary>
        string GetName(PropertyInfo propertyInfo);

        /// <summary>
        /// Get the corresponding property for the given name
        /// </summary>
        /// <param name="name">The OCL representation of the name</param>
        /// <param name="properties">The available properties</param>
        /// <returns>The property that is the best match for the name, otherwise null if no match is found</returns>
        PropertyInfo? GetProperty(string name, IReadOnlyCollection<PropertyInfo> properties);
    }
}