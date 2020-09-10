using System;
using System.Collections.Generic;
using System.Linq;

namespace Octopus.Ocl
{
    /// <summary>
    /// A block, <code>MyThing "ALabel" { ... }</code>
    /// <seealso cref="https://github.com/hashicorp/hcl/blob/hcl2/hclsyntax/spec.md#blocks" />
    /// </summary>
    /// <remarks>
    /// A block creates a child body that is annotated with a block type and zero or more block labels. Blocks create a
    /// structural hierarchy which can be interpreted by the calling application.
    /// </remarks>
    public class OclBlock : OclBody, IOclElement
    {
        private string name;

        public OclBlock(string name)
            : this(name, Array.Empty<string>())
        {
        }

        public OclBlock(string name, string label)
            : this(name, new[] { label }, Array.Empty<IOclElement>())
        {
        }

        public OclBlock(string name, IEnumerable<string> labels)
            : this(name, labels, Array.Empty<IOclElement>())
        {
        }

        public OclBlock(string name, IEnumerable<string> labels, IEnumerable<IOclElement> elements)
            : base(elements)
        {
            this.name = Name = name; // Make the compiler happy
            Labels = labels.ToList();
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
        public List<string> Labels { get; }
    }
}