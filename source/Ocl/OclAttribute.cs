using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Octopus.Ocl
{
    /// <summary>
    /// An attribute, <code>MyAttribute = value</code>
    /// <seealso cref="https://github.com/hashicorp/hcl/blob/hcl2/hclsyntax/spec.md#attribute-definitions" />
    /// </summary>
    /// <remarks>
    /// An attribute definition assigns a value to a particular attribute name within a body. Each distinct attribute name may be defined no more than once within a single body.
    /// </remarks>
    [DebuggerDisplay("{Name} = {Value}", Name = "OclAttribute")]
    public class OclAttribute : IOclElement
    {
        string name;
        object? value;

        public OclAttribute(string name, object? value)
        {
            this.name = Name = name; // Make the compiler happy
            Value = value;
        }

        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new OclException("Attribute names cannot be blank");
                name = value;
            }
        }

        /// <remarks>
        /// The attribute value is given as an expression, which is retained literally for later evaluation by the calling application.
        /// </remarks>
        public object? Value
        {
            get => value;
            set
            {
                if (value != null && !IsSupportedValueType(value.GetType()))
                    throw new OclException($"The type {value.GetType().FullName} is not a support value type OCL attribute value");
                this.value = value;
            }
        }

        public static bool IsSupportedValueType(Type type)
        {
            bool IsNullableSupportedValueType()
                => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsSupportedValueType(type.GetGenericArguments()[0]);

            return type.IsPrimitive ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(OclStringLiteral) ||
                IsObjectDictionary(type) ||
                IsStringDictionary(type) ||
                IsNullableSupportedValueType() ||
                IsSupportedValueCollectionType(type);
        }

        internal static bool IsObjectDictionary(Type type)
            => typeof(IEnumerable<KeyValuePair<string, object?>>).IsAssignableFrom(type);

        internal static bool IsStringDictionary(Type type)
            => typeof(IEnumerable<KeyValuePair<string, string>>).IsAssignableFrom(type);

        internal static bool IsSupportedValueCollectionType(Type type)
            => (type.IsArray && type.GetArrayRank() == 1 && IsSupportedValueType(type.GetElementType()!)) ||
                (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType && type.GenericTypeArguments.Length == 1 && IsSupportedValueType(type.GenericTypeArguments[0]));

        public override string ToString()
        {
            var sb = new StringBuilder();
            using var tw = new StringWriter(sb, CultureInfo.InvariantCulture);
            using var writer = new OclWriter(tw);
            writer.Write(this);
            return sb.ToString();
        }
    }
}