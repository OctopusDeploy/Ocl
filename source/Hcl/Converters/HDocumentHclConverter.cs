using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Octopus.Hcl.Converters
{
    internal class HDocumentHclConverter : DefaultBlockHclConverter
    {
        public HDocument Convert(object? obj, HclConversionContext context)
        {
            if (obj == null)
                return new HDocument();

            var defaultProperties = obj.GetType().GetDefaultMembers().OfType<PropertyInfo>();

            var properties = from p in obj.GetType().GetProperties()
                where p.CanRead
                where p.GetCustomAttribute<HclIgnoreAttribute>() == null
                select p;

            return new HDocument(
                GetElements(obj, properties.Except(defaultProperties), context)
            );
        }
    }
}