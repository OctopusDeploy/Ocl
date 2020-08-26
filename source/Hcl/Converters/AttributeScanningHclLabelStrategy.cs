using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Octopus.Hcl.Converters
{
    public class AttributeScanningHclLabelStrategy : IHclLabelStrategy
    {
        public IEnumerable<string> GetLabels(object obj)
        {
            var labels = from p in obj.GetType().GetProperties()
                where p.CanRead
                let attr = p.GetCustomAttribute<HclLabelAttribute>()
                where attr != null
                orderby attr.Ordinal
                let labelObj = p.GetValue(obj) ?? throw new HclException($"Labels cannot be null ({p.DeclaringType?.FullName}.{p.Name})")
                let label = labelObj as string ?? throw new Exception($"Labels must be strings ({p.DeclaringType?.FullName}.{p.Name})")
                select label;
            return labels;
        }
    }
}