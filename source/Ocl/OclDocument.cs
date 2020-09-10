using System;
using System.Collections.Generic;

namespace Octopus.Ocl
{
    public class OclDocument : OclBody, IOclElement
    {
        public OclDocument()
        {

        }

        public OclDocument(IEnumerable<IOclElement> elements)
            : base(elements)
        {
        }
    }
}