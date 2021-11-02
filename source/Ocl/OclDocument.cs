using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

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

        public string Name => "";

        public override string ToString()
        {
            var sb = new StringBuilder();
            using var tw = new StringWriter(sb, CultureInfo.InvariantCulture);
            using var writer = new OclWriter(tw);
            writer.Write(this);
            return sb.ToString();
        }
    }
}