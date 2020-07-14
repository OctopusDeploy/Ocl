using System;
using System.Text;
using Octopus.Hcl.Converters;

namespace Octopus.Hcl
{
    public class HclConvert
    {
        public static string Serialize(HDocument document, HclSerializerOptions? options = null)
        {
            options ??= new HclSerializerOptions();
            var sb = new StringBuilder();
            // ReSharper disable once ConvertToUsingDeclaration
            using (var writer = new HclWriter(sb, options))
            {
                writer.Write(document);
            }

            return sb.ToString();
        }

        public static string Serialize(object? obj, HclSerializerOptions? options = null)
            => Serialize(ToHDocument(obj, options));

        public static HDocument ToHDocument(object? obj, HclSerializerOptions? options = null)
            => new HDocumentHclConverter().Convert(obj, new HclConversionContext(options ?? new HclSerializerOptions()));
    }
}