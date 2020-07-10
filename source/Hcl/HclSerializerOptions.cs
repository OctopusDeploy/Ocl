using System;

namespace Octopus.Hcl
{
    /// <summary>
    /// This class is not threadsafe while it is being used to serialize/deserialize
    /// </summary>
    public class HclSerializerOptions
    {
        public char IndentChar { get; set; } = ' ';
        public int IndentDepth { get; set; } = 4;
        public string DefaultHeredocIdentifier { get; set; } = "EOT";
    }
}