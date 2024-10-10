using System;

namespace Octopus.Ocl.Converters
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class OclFunctionAttribute : Attribute
    {
        public OclFunctionAttribute(string name)
            => Name = name;

        /// <summary>
        /// The name of the FunctionCall operation
        /// </summary>
        public string Name { get; }
    }
}