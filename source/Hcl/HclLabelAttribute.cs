using System;

namespace Octopus.Hcl
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HclLabelAttribute : Attribute
    {
        public object Ordinal { get; set; } = 1_000;
    }
}