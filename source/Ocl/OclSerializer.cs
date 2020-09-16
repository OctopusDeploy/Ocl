using System;

namespace Octopus.Ocl
{
    public interface IOclSerializer
    {
        OclSerializerOptions Options { get; }
        string Serialize(OclDocument document);
        string Serialize(object obj);
        T Deserialize<T>(string document) where T : notnull;
        T Deserialize<T>(OclDocument document) where T : notnull;
    }

    /// <summary>
    /// This class is thread safe
    /// </summary>
    public class OclSerializer : IOclSerializer
    {
        public OclSerializer(OclSerializerOptions? options = null)
            => Options = options ?? new OclSerializerOptions();

        public OclSerializerOptions Options { get; }

        public string Serialize(OclDocument document)
            => OclConvert.Serialize(document, Options);

        public string Serialize(object obj)
            => OclConvert.Serialize(obj, Options);

        public T Deserialize<T>(string document) where T : notnull
            => OclConvert.Deserialize<T>(document, Options);

        public T Deserialize<T>(OclDocument document) where T : notnull
            => OclConvert.Deserialize<T>(document, Options);
    }
}