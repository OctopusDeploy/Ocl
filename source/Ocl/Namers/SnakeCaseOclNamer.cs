using System;
using System.Text.RegularExpressions;

namespace Octopus.Ocl.Namers
{
    public class SnakeCaseOclNamer : IOclNamer
    {
        public string FormatName(string name)
            => Regex.Replace(name, "([a-z])([A-Z])", "$1_$2")
                .Replace(" ", "_")
                .ToLower();
    }
}