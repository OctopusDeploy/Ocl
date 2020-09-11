using System;
using System.Text;
using Octopus.Ocl.Converters;
using Octopus.Ocl.Parsing;

namespace Octopus.Ocl
{
    public class OclConvert
    {
        public static string Serialize(OclDocument document, OclSerializerOptions? options = null)
        {
            options ??= new OclSerializerOptions();
            var sb = new StringBuilder();
            // ReSharper disable once ConvertToUsingDeclaration
            using (var writer = new OclWriter(sb, options))
            {
                writer.Write(document);
            }

            return sb.ToString();
        }

        public static string Serialize(object? obj, OclSerializerOptions? options = null)
        {
            var document = obj as OclDocument ?? ToOclDocument(obj, options);
            return Serialize(document);
        }

        public static OclDocument ToOclDocument(object? obj, OclSerializerOptions? options = null)
            => new OclDocumentOclConverter().Convert(obj, new OclConversionContext(options ?? new OclSerializerOptions()));

        public static T Deserialize<T>(string ocl, OclSerializerOptions? options = null)
            where T : notnull
        {
            var document = OclParser.Execute(ocl);
            return typeof(T) == typeof(OclDocument)
                ? (T)(object)document
                : Deserialize<T>(document, options);
        }

        public static T Deserialize<T>(OclDocument document, OclSerializerOptions? options = null)
            where T : notnull
        {
            var context = new OclConversionContext(options ?? new OclSerializerOptions());
            var result = context.FromElement(typeof(T), document, () => null);
            if (result == null)
                throw new OclException("Document conversion resulted in null, which is not valid");
            return (T)result;
        }
    }
}