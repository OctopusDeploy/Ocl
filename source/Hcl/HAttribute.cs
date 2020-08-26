using System;
using System.Collections;
using System.Collections.Generic;

namespace Octopus.Hcl
{
    /// <summary>
    /// A HCL attribute, <code>MyAttribute = value</code>
    /// <seealso cref="https://github.com/hashicorp/hcl/blob/hcl2/hclsyntax/spec.md#attribute-definitions" />
    /// </summary>
    /// <remarks>
    /// An attribute definition assigns a value to a particular attribute name within a body. Each distinct attribute name may be defined no more than once within a single body.
    /// </remarks>
    public class HAttribute : IHElement
    {
        private string name;
        private object? value;

        public HAttribute(string name, object? value)
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
                    throw new ArgumentException("Attribute names cannot be blank");
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
                    throw new ArgumentException($"The type {value.GetType().FullName} is not a support value type HCL attribute");
                this.value = value;
            }
        }

        public static bool IsSupportedValueType(Type type)
            => type.IsPrimitive ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(HclStringLiteral) ||
                IsSupportedValueCollectionType(type);

        internal static bool IsSupportedValueCollectionType(Type type)
            => (type.IsArray && type.GetArrayRank() == 1 && IsSupportedValueType(type.GetElementType()!)) ||
                (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType && type.GenericTypeArguments.Length == 1 && IsSupportedValueType(type.GenericTypeArguments[0]));
    }
}