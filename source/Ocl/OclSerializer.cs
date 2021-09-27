using System;
using System.Text;
using Octopus.Ocl.Parsing;

namespace Octopus.Ocl
{
    public interface IOclSerializer
    {
        string Serialize(OclDocument document);
        string Serialize(object obj);

        OclDocument Deserialize(string str);
        T Deserialize<T>(string str) where T : notnull;
        T Deserialize<T>(OclDocument document) where T : notnull;

        OclDocument ToOclDocument(object? obj);
    }

    /// <summary>
    /// This class is thread safe
    /// </summary>
    public class OclSerializer : IOclSerializer
    {
        readonly OclSerializerOptions options;

        public OclSerializer(OclSerializerOptions? options = null)
            => this.options = options ?? new OclSerializerOptions();

        public string Serialize(OclDocument document)
        {
            var sb = new StringBuilder();
            // ReSharper disable once ConvertToUsingDeclaration
            using (var writer = new OclWriter(sb, options))
            {
                writer.Write(document);
            }

            return sb.ToString();
        }

        public string Serialize(object obj)
        {
            var document = obj as OclDocument ?? ToOclDocument(obj);
            var sb = new StringBuilder();
            // ReSharper disable once ConvertToUsingDeclaration
            using (var writer = new OclWriter(sb, options))
            {
                writer.Write(document);
            }

            return sb.ToString();
        }

        public OclDocument Deserialize(string ocl)
        {
            var document = OclParser.Execute(ocl);
            return document;
        }

        public T Deserialize<T>(string ocl) where T : notnull
        {
            var document = Deserialize(ocl);
            return typeof(T) == typeof(OclDocument)
                ? (T)(object)document
                : Deserialize<T>(document);
        }

        public T Deserialize<T>(OclDocument document) where T : notnull
        {
            var context = new OclConversionContext(options);
            var result = context.FromElement(typeof(T), document, null);
            if (result == null)
                throw new OclException("Document conversion resulted in null, which is not valid");
            return (T)result;
        }

        public OclDocument ToOclDocument(object? obj)
        {
            if (obj == null)
                return new OclDocument();

            var context = new OclConversionContext(options);

            var converter = context.GetConverterFor(obj.GetType());
            return converter.ToDocument(context, obj);
        }
    }
}