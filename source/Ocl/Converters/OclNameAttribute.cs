using System;

namespace Octopus.Ocl.Converters
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class OclNameAttribute : Attribute
    {
        public OclNameAttribute(string name)
            => Name = name;

        /// <summary>
        /// The name of the OCL attribute or block.
        /// See https://github.com/OctopusDeploy/Ocl#ebnf
        /// </summary>
        public string Name { get; set; }
    }
}