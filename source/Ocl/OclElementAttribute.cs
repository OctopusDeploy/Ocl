using System;

namespace Octopus.Ocl
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OclElementAttribute : Attribute
    {
        public int Ordinal { get; set; } = 1_000;
        public string? Name { get; set; }
    }
}