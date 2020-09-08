using System;
using System.Text;

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
    }
}