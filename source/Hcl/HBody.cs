using System;
using System.Collections;
using System.Collections.Generic;

namespace Octopus.Hcl
{
    /// <summary>
    ///     <seealso cref="https://github.com/hashicorp/hcl/blob/hcl2/hclsyntax/spec.md#bodies" />
    /// </summary>
    /// <remarks>
    /// A body is a collection of associated attributes and blocks. The meaning of this association is defined by the calling application.
    /// </remarks>
    public class HBody : IEnumerable<IHElement>
    {
        private readonly List<IHElement> elements = new List<IHElement>();

        public IEnumerator<IHElement> GetEnumerator()
            => elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public void Add(IHElement element)
            => elements.Add(element);
    }
}