using System.Reflection;
using Octopus.Ocl.Converters;

namespace Octopus.Ocl.Namers
{
    static class NamerExtensions
    {
        internal static string GetOclAttributeName(this IOclNamer namer, PropertyInfo propertyInfo)
        {
            // The OclAttribute attribute can be applied to properties to control the OCL attribute name
            return propertyInfo.GetCustomAttribute(typeof(OclAttributeAttribute)) is OclAttributeAttribute oclAttribute && !string.IsNullOrEmpty(oclAttribute.AttributeName)
                ? oclAttribute.AttributeName
                : namer.FormatName(propertyInfo.Name);
        }
    }
}