using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Octopus.Ocl.Converters
{
    internal class OclDocumentOclConverter : DefaultBlockOclConverter
    {
        public OclDocument Convert(object? obj, OclConversionContext context)
        {
            if (obj == null)
                return new OclDocument();

            var defaultProperties = obj.GetType().GetDefaultMembers().OfType<PropertyInfo>();

            var properties = from p in obj.GetType().GetProperties()
                where p.CanRead
                where p.GetCustomAttribute<OclIgnoreAttribute>() == null
                select p;

            return new OclDocument(
                GetElements(obj, properties.Except(defaultProperties), context)
            );
        }
    }
}