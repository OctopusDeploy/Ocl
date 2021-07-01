using System;
using System.Text;
using Octopus.Ocl.Parsing;

namespace Octopus.Ocl
{
    public interface IOclSerializer
    {
        string Serialize(OclDocument document);
        string Serialize(object obj);
        OclDocument ToOclDocument(object? obj);
        T Deserialize<T>(string str) where T : notnull;
        object Deserialize(string ocl, Type type);
        T Deserialize<T>(OclDocument document) where T : notnull;
        object Deserialize(OclDocument document, Type type);
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

        public OclDocument ToOclDocument(object? obj)
        {
            if (obj == null)
                return new OclDocument();

            var context = new OclConversionContext(options);

            var converter = context.GetConverterFor(obj.GetType());
            return converter.ToDocument(context, obj);
        }

        public T Deserialize<T>(string ocl) where T : notnull
        {
            return (T)Deserialize(ocl, typeof(T));
        }

        public object Deserialize(string ocl, Type type)
        {
            var document = OclParser.Execute(ocl);
            return type == typeof(OclDocument)
                ? document
                : Deserialize(document, type);
        }

        public T Deserialize<T>(OclDocument document) where T : notnull
        {
            return (T)Deserialize(document, typeof(T));
        }
        
        public object Deserialize(OclDocument document, Type type)
        {
            var context = new OclConversionContext(options);
            var result = context.FromElement(type, document, null);
            if (result == null)
                throw new OclException("Document conversion resulted in null, which is not valid");
            return result;
        }
    }
}