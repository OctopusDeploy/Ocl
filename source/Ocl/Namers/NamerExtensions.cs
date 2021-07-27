using System.Reflection;
using Octopus.Ocl.Converters;

namespace Octopus.Ocl.Namers
{
    static class NamerExtensions
    {
        internal static string GetOclNameForProperty(this IOclNamer namer, PropertyInfo propertyInfo)
        {
            // The OclAttribute attribute can be applied to properties to control the OCL attribute name
            return propertyInfo.GetCustomAttribute(typeof(OclNameAttribute)) is OclNameAttribute oclAttribute && !string.IsNullOrEmpty(oclAttribute.Name)
                ? oclAttribute.Name
                : namer.FormatName(propertyInfo.Name);
        }
    }
}