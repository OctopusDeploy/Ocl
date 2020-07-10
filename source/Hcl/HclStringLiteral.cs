using System;

namespace Octopus.Hcl
{
    public enum HclStringLiteralFormat
    {
        SingleLine = 0,
        Heredoc = 1,
        IndentedHeredoc = 2
    }

    public class HclStringLiteral
    {
        public HclStringLiteral(string value, HclStringLiteralFormat format)
        {
            Value = value;
            Format = format;
        }

        public string Value { get; }
        public HclStringLiteralFormat Format { get; }
        public string HeredocIdentifier { get; set; } = "EOT";

        public static HclStringLiteral Create(string value)
            => new HclStringLiteral(value, value.Contains('\n') ? HclStringLiteralFormat.IndentedHeredoc : HclStringLiteralFormat.SingleLine);
    }
}