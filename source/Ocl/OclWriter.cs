using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Octopus.Ocl
{
    /// <summary>
    /// This class is not threadsafe
    /// </summary>
    public class OclWriter : IDisposable
    {
        readonly TextWriter writer;
        int currentIndent;
        bool isFirstLine = true;
        bool lastWrittenWasBlock;

        public OclWriter(StringBuilder sb, OclSerializerOptions? options = null)
            : this(new StringWriter(sb), options)
        {
        }

        public OclWriter(TextWriter writer, OclSerializerOptions? options = null)
        {
            this.writer = writer;
            Options = options ?? new OclSerializerOptions();
        }

        public OclSerializerOptions Options { get; }

        public void Write(IOclElement element)
        {
            switch (element)
            {
                case OclAttribute attribute:
                    Write(attribute);
                    return;
                case OclBlock block:
                    Write(block);
                    return;
                case OclDocument document:
                    foreach (var item in document)
                        Write(item);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(element.GetType().Name);
            }
        }

        public void Write(OclAttribute attribute)
        {
            WriteNextLine();
            WriteIndent();
            WriteIdentifier(attribute.Name);
            writer.Write(" = ");
            WriteValue(attribute.Value);
        }

        public void Write(OclBlock block)
        {
            if (!isFirstLine && !lastWrittenWasBlock)
                writer.WriteLine();
            WriteNextLine();
            WriteIndent();
            WriteIdentifier(block.Name);

            foreach (var label in block.Labels)
            {
                writer.Write(' ');
                WriteSingleLineStringLiteral(label);
            }

            writer.Write(" {");

            currentIndent++;
            foreach (var child in block)
                Write(child);
            currentIndent--;

            writer.WriteLine();

            WriteIndent();
            writer.Write("}");

            lastWrittenWasBlock = true;
        }

        void WriteNextLine()
        {
            if (!isFirstLine)
                writer.WriteLine();
            if (lastWrittenWasBlock)
                writer.WriteLine();
            isFirstLine = false;
            lastWrittenWasBlock = false;
        }

        void WriteIndent(int extraIndent = 0)
        {
            var indent = (currentIndent + extraIndent) * Options.IndentDepth;
            for (var x = 0; x < indent; x++)
                writer.Write(Options.IndentChar);
        }

        void WriteIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new OclException("Identifier cannot be blank");

            if (char.IsDigit(identifier[0]))
                writer.Write('_');

            foreach (var c in identifier)
                if (
                    char.IsLetterOrDigit(c) ||
                    c == '_' ||
                    c == '-'
                )
                    writer.Write(c);
                else
                    writer.Write('_');
        }

        void WriteValue(object? value)
        {
            if (value == null)
            {
                writer.Write("null");
                return;
            }

            switch (value)
            {
                case bool b:
                    writer.Write(b ? "true" : "false");
                    return;
                case byte b:
                    writer.Write(b);
                    return;
                case ushort s:
                    writer.Write(s);
                    return;
                case short s:
                    writer.Write(s);
                    return;
                case uint i:
                    writer.Write(i);
                    return;
                case int i:
                    writer.Write(i);
                    return;
                case ulong l:
                    writer.Write(l);
                    return;
                case long l:
                    writer.Write(l);
                    return;
                case float f:
                    writer.Write(f);
                    return;
                case double d:
                    writer.Write(d);
                    return;
                case decimal d:
                    writer.Write(d);
                    return;
                case char c:
                    WriteSingleLineStringLiteral(c.ToString());
                    return;
                case string s:
                    var literal = OclStringLiteral.Create(s);
                    literal.HeredocTag = Options.DefaultHeredocTag;
                    WriteValue(literal);
                    return;
                case OclStringLiteral s:
                    WriteValue(s);
                    return;
            }

            if (OclAttribute.IsSupportedValueCollectionType(value.GetType()))
            {
                var enumerable = (IEnumerable)value;
                writer.Write('[');
                var isFirst = true;
                foreach (var item in enumerable)
                {
                    if (!isFirst)
                        writer.Write(", ");
                    isFirst = false;
                    WriteValue(item);
                }

                writer.Write(']');
                return;
            }

            throw new InvalidOperationException($"The type {value.GetType().FullName} is not a valid attribute value and can not be serialized");
        }

        void WriteValue(OclStringLiteral literal)
        {
            if (literal.Format == OclStringLiteralFormat.SingleLine)
            {
                WriteSingleLineStringLiteral(literal.Value);
                return;
            }

            var isIndented = literal.Format == OclStringLiteralFormat.IndentedHeredoc;

            writer.Write("<<");
            if (isIndented)
                writer.Write("-");
            writer.WriteLine(literal.HeredocTag);

            var lines = literal.Value.Split('\n');
            for(var x = 0; x < lines.Length; x++)
            {
                if(x > 0)
                    writer.Write('\n');

                if (isIndented)
                    WriteIndent(2);
                writer.Write(lines[x]);
            }
            writer.WriteLine();

            if (isIndented)
                WriteIndent(1);
            writer.Write(literal.HeredocTag);
        }

        void WriteSingleLineStringLiteral(string s)
        {
            writer.Write('"');
            s = s.Replace(@"\", @"\\")
                .Replace("\r", @"\r")
                .Replace("\n", @"\n")
                .Replace("\t", @"\t")
                .Replace("\"", @"\""");
            writer.Write(s);
            writer.Write('"');
        }

        public void Dispose()
        {
            writer.Dispose();
        }
    }
}