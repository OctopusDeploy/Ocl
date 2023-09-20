using System;
using System.Collections.Generic;
using Octopus.Ocl.FunctionCalls;
using Octopus.Ocl.Namers;

namespace Octopus.Ocl
{
    /// <summary>
    /// This class is not threadsafe while it is being used to serialize/deserialize
    /// </summary>
    public class OclSerializerOptions
    {
        /// <summary>
        /// What character is used when indenting the OCL text.
        /// This is not settable as it is
        /// currently not used in some places in the static Parser 
        /// </summary>
        public char IndentChar { get; internal set; } = ' ';

        public int IndentDepth { get; set; } = 4;
        public string DefaultHeredocTag { get; set; } = "EOT";
        public List<IOclConverter> Converters { get; set; } = new List<IOclConverter>();
        public List<IFunctionCall> Functions { get; set; } = new List<IFunctionCall>();
        public IOclNamer Namer { get; set; } = new SnakeCaseOclNamer();
    }
}