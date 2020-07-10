using System;

namespace Octopus.Hcl
{
    /// <summary>
    /// A HCL attribute, <code>MyAttribute = value</code>
    /// <seealso cref="https://github.com/hashicorp/hcl/blob/hcl2/hclsyntax/spec.md#attribute-definitions" />
    /// </summary>
    /// <remarks>
    /// An attribute definition assigns a value to a particular attribute name within a body. Each distinct attribute name may be defined no more than once within a single body.
    /// </remarks>
    public class HAttribute : IHElement
    {
        private string name;

        public HAttribute(string name, object? value)
        {
            this.name = Name = name; // Make the compiler happy
            Value = value;
        }

        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Attribute names cannot be blank");
                name = value;
            }
        }

        /// <remarks>
        /// The attribute value is given as an expression, which is retained literally for later evaluation by the calling application.
        /// </remarks>
        public object? Value { get; set; }
    }
}