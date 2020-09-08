using System;

namespace Octopus.Ocl
{
    /// <summary>
    /// This class is thread safe
    /// </summary>
    public class OclSerializer
    {
        public OclSerializer(OclSerializerOptions? options = null)
        {
            Options = options ?? new OclSerializerOptions();
        }

        public OclSerializerOptions Options { get; }

        public string Serialize(OclDocument document)
        {
            return OclConvert.Serialize(document, Options);
        }
    }
}