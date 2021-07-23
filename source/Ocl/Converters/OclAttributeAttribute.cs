using System;

namespace Octopus.Ocl.Converters
{
    [AttributeUsage(AttributeTargets.Property,  AllowMultiple = false)]
    public sealed class OclAttributeAttribute : Attribute
    {
        public OclAttributeAttribute(string attributeName)
        {
            AttributeName = attributeName;
        } 
        
        /// <summary>
        /// The name of the OCL attribute.
        /// See https://github.com/OctopusDeploy/Ocl#ebnf
        /// </summary>
        public string AttributeName { get; set; }
    }
}