using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Octopus.Hcl.Converters
{
    public abstract class HclConverter : IHclConverter
    {
        public abstract bool CanConvert(Type type);

        public IEnumerable<IHElement> Convert(HclConversionContext context, string name, object obj)
            => new[] { ConvertInternal(context, name, obj) };

        protected abstract IHElement ConvertInternal(HclConversionContext context, string name, object obj);

        protected virtual string GetName(string name, object obj)
            => name;
    }
}