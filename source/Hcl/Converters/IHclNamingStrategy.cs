using System;
using System.Reflection;

namespace Octopus.Hcl.Converters
{
    public interface IHclNamingStrategy
    {
        string GetName(PropertyInfo property, object value);
    }
}