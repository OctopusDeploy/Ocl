using System;

namespace Octopus.Ocl
{
    public enum OclStringLiteralFormat
    {
        SingleLine = 0,
        Heredoc = 1,
        IndentedHeredoc = 2
    }

    public class OclStringLiteral
    {
        public OclStringLiteral(string value, OclStringLiteralFormat format)
        {
            Value = value;
            Format = format;
        }

        public string Value { get; }
        public OclStringLiteralFormat Format { get; }
        public string HeredocIdentifier { get; set; } = "EOT";

        public static OclStringLiteral Create(string value)
            => new OclStringLiteral(value, value.Contains('\n') ? OclStringLiteralFormat.IndentedHeredoc : OclStringLiteralFormat.SingleLine);
    }
}