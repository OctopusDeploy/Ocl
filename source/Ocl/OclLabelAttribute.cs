using System;

namespace Octopus.Ocl
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OclLabelAttribute : Attribute
    {
        public object Ordinal { get; set; } = 1_000;
    }
}