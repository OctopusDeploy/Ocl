using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace Octopus.Ocl.Parsing
{
    static class OclParser
    {
        static readonly Parser<char> Comma = Parse.Char(',');
        static readonly Parser<char> ArrayOpen = Parse.Char('[');
        static readonly Parser<char> ArrayClose = Parse.Char(']');

        static readonly Parser<char> BlockOpen = Parse.Char('{');
        static readonly Parser<char> BlockClose = Parse.Char('}');

        static readonly Parser<string> Identifier =
            from name in Parse.Char(c => c == '_' || char.IsLetterOrDigit(c), "letter, digit, _")
                .AtLeastOnce()
                .Text()
            select name;

        static readonly Parser<object?> NullLiteral =
            from _ in Parse.String("null")
            select (object?)null;

        static readonly Parser<bool> TrueLiteral =
            from _ in Parse.String("true")
            select true;

        static readonly Parser<bool> FalseLiteral =
            from _ in Parse.String("false")
            select false;

        static readonly Parser<int> IntegerLiteral =
            from neg in Parse.Char('-').Optional()
            from number in Parse.Number
            let result = int.Parse(number)
            select neg.IsDefined ? 0 - result : result;

        static readonly Parser<decimal> DecimalLiteral =
            from neg in Parse.Char('-').Optional()
            from whole in Parse.Digit.AtLeastOnce().Text()
            from dot in Parse.Char('.')
            from fraction in Parse.Digit.AtLeastOnce().Text()
            let result = decimal.Parse(whole + "." + fraction)
            select neg.IsDefined ? 0 - result : result;

        static readonly Parser<string[]> QuotedStringArrayLiteral =
            from open in ArrayOpen.Token()
            from values in QuotedStringParser.QuotedStringLiteral.DelimitedBy(Comma.Token())
            from close in ArrayClose.Token()
            select values.ToArray();

        static readonly Parser<string[]> EmptyArrayLiteral =
            from open in ArrayOpen.Token()
            from close in ArrayClose.Token()
            select new string[0];

        static readonly Parser<int[]> IntArrayLiteral =
            from open in ArrayOpen.Token()
            from values in IntegerLiteral.DelimitedBy(Comma.Token())
            from close in ArrayClose.Token()
            select values.ToArray();

        static readonly Parser<decimal[]> DecimalArrayLiteral =
            from open in ArrayOpen.Token()
            from values in DecimalLiteral.DelimitedBy(Comma.Token())
            from close in ArrayClose.Token()
            select values.ToArray();

        static readonly Parser<object> Tuple =
            EmptyArrayLiteral
                .Or(QuotedStringArrayLiteral)
                .Or<object>(DecimalArrayLiteral)
                .Or(IntArrayLiteral);

        static readonly Parser<object> NumberLiteral =
            DecimalLiteral.Select(d => (object)d)
                .Or(IntegerLiteral.Select(d => (object)d));

        static readonly Parser<object> FunctionCall =
            from identifier in Identifier.Token()
            from funcOpen in Parse.Char('(')
            from values in Literal.DelimitedBy(Comma.Token()).Optional()
            from funcClose in Parse.Char(')')
            select new OclFunctionCall(identifier.ToString(), values.GetOrDefault() ?? Array.Empty<object?>());
        
        static readonly Parser<object?> Literal =
            NullLiteral
                .XOr(TrueLiteral.Select(d => (object)d))
                .XOr(FalseLiteral.Select(d => (object)d))
                .XOr(QuotedStringParser.QuotedStringLiteral)
                .XOr(HeredocParser.Literal)
                .XOr(NumberLiteral)
                .XOr(Tuple);

        static readonly Parser<string> UnquotedDictionaryKey =
            Parse.CharExcept(c => char.IsWhiteSpace(c) || c == '"', "Not whitespace or quotes")
                .Many()
                .Text();

        static readonly Parser<KeyValuePair<string, object?>> ObjectElem =
            from key in UnquotedDictionaryKey.Or(QuotedStringParser.QuotedStringLiteral).SameLineToken()
            from _ in Parse.Char('=')
            from value in ExprTerm
            select new KeyValuePair<string, object?>(key, value);

        /// <summary>
        /// See https://github.com/hashicorp/hcl/blob/main/hclsyntax/spec.md#collection-values
        /// </summary>
        static readonly Parser<Dictionary<string, object?>> @Object =
            from open in BlockOpen.Token()
            from entries in ObjectElem.Token().Many()
            from close in BlockClose.Token()
            select new Dictionary<string, object?>(entries);


        /// <summary>
        /// See https://github.com/hashicorp/hcl/blob/hcl2/hclsyntax/spec.md#attribute-definitions
        /// Currently only support <see cref="ExprTerm"/> values
        /// </summary>
        static readonly Parser<OclAttribute> Attribute =
            from name in Identifier.SameLineToken()
            from _ in Parse.Char('=')
            from value in ExprTerm
            select new OclAttribute(name, value);
        
        /// <summary>
        /// See https://github.com/hashicorp/hcl/blob/main/hclsyntax/spec.md#expression-terms
        /// </summary>
        static readonly Parser<object?> ExprTerm =
            Object.Or(FunctionCall).Or(Literal).SameLineToken();

        static readonly Parser<IOclElement[]> EmptyBlockBody =
            from open in BlockOpen.SameLineToken()
            from close in BlockClose.Token()
            select Array.Empty<IOclElement>();

        static readonly Parser<IOclElement[]> BlockBody =
            from open in BlockOpen.SameLineToken()
            from openNewline in ParserCommon.NewLine
            from children in Block.Token().Or<IOclElement>(Attribute.Token()).AtLeastOnce()
            from close in BlockClose.Token()
            select children.ToArray();

        static readonly Parser<OclBlock> Block =
            from name in Identifier.SameLineToken()
            from labels in QuotedStringParser.QuotedStringLiteral.SameLineToken().Many()
            from children in EmptyBlockBody.Or(BlockBody).Token().Once()
            select new OclBlock(name, labels.ToArray(), children.Single());

        static readonly Parser<OclDocument> Document =
            from child in Block.Or<IOclElement>(Attribute).Token().XMany().End()
            select new OclDocument(child);

        public static Parser<T> SameLineToken<T>(this Parser<T> parser)
            => from leading in ParserCommon.WhitespaceExceptNewline.Many()
                from item in parser
                from trailing in ParserCommon.WhitespaceExceptNewline.Many()
                select item;

        internal static OclDocument Execute(string input)
        {
            var (document, error) = TryExecute(input);

            if (error != null)
                throw new OclException(error);

            return document ?? throw new OclException("Null document returned"); // Shouldn't happen
        }

        internal static (OclDocument? document, string? error) TryExecute(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (new OclDocument(), null);

            var result = Document.TryParse(input);
            if (!result.WasSuccessful)
                return (null, result.ToString());

            if (!result.Remainder.AtEnd)
                throw new OclException("Parser should have expected end of input and we never get here");

            return (result.Value, null);
        }
    }
}