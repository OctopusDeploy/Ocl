using System;

namespace Octopus.Hcl
{
    /// <summary>
    /// This class is thread safe
    /// </summary>
    public class HclSerializer
    {
        public HclSerializer(HclSerializerOptions? options = null)
        {
            Options = options ?? new HclSerializerOptions();
        }

        public HclSerializerOptions Options { get; }

        public string Serialize(HDocument document)
            => HclConvert.Serialize(document, Options);
        
        public string Serialize(object obj)
            => HclConvert.Serialize(obj, Options);
        
    }
}