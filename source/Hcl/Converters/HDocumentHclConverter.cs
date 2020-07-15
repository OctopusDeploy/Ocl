using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Octopus.Hcl.Converters
{
    internal class HDocumentHclConverter
    {
        public HDocument Convert(object? obj, HclConversionContext context)
            => obj == null
                ? new HDocument()
                : new HDocument(
                    GetElements(
                        obj,
                        from p in obj.GetType().GetProperties()
                        where p.CanRead
                        where p.GetCustomAttribute<HclIgnoreAttribute>() == null
                        select p,
                        context
                    )
                );
    }
}