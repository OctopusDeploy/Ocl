using System;
using System.Linq;
using Sprache;

namespace Octopus.Ocl.Parsing
{
    internal static class OclParser
    {
        private static readonly Parser<Char> Comma = Parse.Char(',');
        private static readonly Parser<Char> ArrayOpen = Parse.Char('[');
        private static readonly Parser<Char> ArrayClose = Parse.Char(']');

        private static readonly Parser<Char> BlockOpen = Parse.Char('{');
        private static readonly Parser<Char> BlockClose = Parse.Char('}');

        public static Parser<T> SameLineToken<T>(this Parser<T> parser)
            => from leading in ParserCommon.WhitespaceExceptNewline.Many()
                from item in parser
                from trailing in ParserCommon.WhitespaceExceptNewline.Many()
                select item;

        private static readonly Parser<string> Name =
            from name in Parse.Char(c => c == '_' || char.IsLetterOrDigit(c), "letter, digit, _")
                .AtLeastOnce()
                .Text()
            select name;


        private static readonly Parser<int> IntegerLiteral =
            from number in Parse.Number
            select int.Parse(number);

        private static readonly Parser<decimal> DecimalLiteral =
            from number in Parse.DecimalInvariant
            select decimal.Parse(number);

        private static readonly Parser<string[]> QuotedStringArrayLiteral =
            from open in ArrayOpen.Token()
            from values in QuotedStringParser.QuotedStringLiteral.DelimitedBy(Comma.Token())
            from close in ArrayClose.Token()
            select values.ToArray();

        private static readonly Parser<string[]> EmptyArrayLiteral =
            from open in ArrayOpen.Token()
            from close in ArrayClose.Token()
            select new string[0];

        private static readonly Parser<int[]> IntArrayLiteral =
            from open in ArrayOpen.Token()
            from values in IntegerLiteral.DelimitedBy(Comma.Token())
            from close in ArrayClose.Token()
            select values.ToArray();

        private static readonly Parser<decimal[]> DecimalArrayLiteral =
            from open in ArrayOpen.Token()
            from values in DecimalLiteral.DelimitedBy(Comma.Token())
            from close in ArrayClose.Token()
            select values.ToArray();

        private static readonly Parser<object> Literal =
            QuotedStringParser.QuotedStringLiteral
                .Or<object>(HeredocParser.Literal)
                .Or(DecimalLiteral.Select(d => (object)d))
                .Or(IntegerLiteral.Select(d => (object)d))
                .Or(EmptyArrayLiteral)
                .Or(QuotedStringArrayLiteral)
                .Or(DecimalArrayLiteral)
                .Or(IntArrayLiteral);

        private static readonly Parser<OclAttribute> Attribute =
            from name in Name.SameLineToken()
            from _ in Parse.Char('=')
            from value in Literal.SameLineToken()
            select new OclAttribute(name, value);

        private static readonly Parser<IOclElement[]> EmptyBlockBody =
            from open in BlockOpen.SameLineToken()
            from close in BlockClose.Token()
            select Array.Empty<IOclElement>();

        private static readonly Parser<IOclElement[]> BlockBody =
            from open in BlockOpen.SameLineToken()
            from openNewline in ParserCommon.NewLine
            from children in Block.Token().Or<IOclElement>(Attribute.Token()).AtLeastOnce()
            from close in BlockClose.Token()
            select children.ToArray();

        private static readonly Parser<OclBlock> Block =
            from name in Name.SameLineToken()
            from labels in QuotedStringParser.QuotedStringLiteral.SameLineToken().Many()
            from children in EmptyBlockBody.Or(BlockBody).Token().Once()
            select new OclBlock(name, labels.ToArray(), children.Single());

        static readonly Parser<OclDocument> Document =
            from child in Block.Or<IOclElement>(Attribute).Many().Token().End()
            select new OclDocument(child);

        internal static OclDocument Execute(string input)
        {
            var (document, error) = TryExecute(input);

            if (error != null)
                throw new OclException(error);

            return document ?? throw new OclException("Null document returned"); // Shouldn't happen
        }

        internal static (OclDocument? document, string? error) TryExecute(string input)
        {
            var result = Document.TryParse(input);
            if (!result.WasSuccessful)
                return (null, result.ToString());

            if (!result.Remainder.AtEnd)
                throw new OclException("Parser should have expected end of input and we never get here");

            return (result.Value, null);
        }
    }
}