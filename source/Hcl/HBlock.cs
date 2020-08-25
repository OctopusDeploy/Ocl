using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
    [DebuggerDisplay("{Name} {DebuggerDisplayLabels}", Name = "HBlock")]
    public class HBlock : HBody, IHElement
    {
        private string name;

        public HBlock(string name)
            : this(name, Array.Empty<string>())
        {
        }

        public HBlock(string name, string label)
            : this(name, new[] { label }, Array.Empty<IHElement>())
        {
        }

        public HBlock(string name, IEnumerable<string> labels)
            : this(name, labels, Array.Empty<IHElement>())
        {
        }

        public HBlock(string name, IEnumerable<string> labels, IEnumerable<IHElement> elements)
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

        private string DebuggerDisplayLabels => string.Join(" ", Labels.Select(l => @$"""{l}"""));
    }
}