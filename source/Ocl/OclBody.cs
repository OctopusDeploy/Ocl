using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Octopus.Ocl
{
    /// <summary>
    ///     <seealso cref="https://github.com/hashicorp/hcl/blob/hcl2/hclsyntax/spec.md#bodies" />
    /// </summary>
    /// <remarks>
    /// A body is a collection of associated attributes and blocks. The meaning of this association is defined by the calling application.
    /// </remarks>
    public class OclBody : IEnumerable<IOclElement>
    {
        readonly List<IOclElement> elements;

        public OclBody()
            => elements = new List<IOclElement>();

        public OclBody(IEnumerable<IOclElement> elements)
            => this.elements = elements.ToList();

        public IEnumerator<IOclElement> GetEnumerator()
            => elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public void Add(IOclElement element)
            => elements.Add(element);

        public void InsertRange(int index, IEnumerable<IOclElement> collection)
            => elements.InsertRange(index, collection);
    }
}