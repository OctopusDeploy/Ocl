using System;
using System.Collections.Generic;

namespace Octopus.Hcl
{
    /// <summary>
    /// A HCL block, <code>MyThing "ALabel" { ... }</code>
    /// <seealso cref="https://github.com/hashicorp/hcl/blob/hcl2/hclsyntax/spec.md#blocks" />
    /// </summary>
    /// <remarks>
    /// A block creates a child body that is annotated with a block type and zero or more block labels. Blocks create a
    /// structural hierarchy which can be interpreted by the calling application.
    /// </remarks>
    public class HBlock : HBody, IHElement
    {
        private string name;

        public HBlock(string name)
        {
            this.name = Name = name; // Make the compiler happy
        }

        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Block names cannot be blank");
                name = value;
            }
        }

        /// <remarks>
        /// Block labels can either be quoted literal strings or naked identifiers.
        /// </remarks>
        public List<string> Labels { get; } = new List<string>();
    }
}