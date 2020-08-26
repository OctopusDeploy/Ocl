using System;
using System.Collections.Generic;
using System.Linq;

namespace Octopus.Hcl
{
    public class HDocument : HBody
    {
        public HDocument()
        {
            
        }
        
        public HDocument(IEnumerable<IHElement> elements)
            : base(elements)
        {
        }
    }
}