using System;

namespace Octopus.Hcl
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HclElementAttribute : Attribute
    {
        public int Ordinal { get; set; } = 1_000;
        public string? Name { get; set; }
    }
}