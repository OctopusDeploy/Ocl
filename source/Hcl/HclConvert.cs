using System;
using System.Text;

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
    }
}