using System;
using System.Collections.Generic;
using System.Linq;

namespace Octopus.Ocl.FunctionCalls
{
    public class Base64DecodeFunctionCall : IFunctionCall
    {
        public string Name => "base64decode";

        public object? ToValue(IEnumerable<object?> arguments)
        {
            var val = arguments.FirstOrDefault();
            if (val == null)
            {
                return null;
            }

            if (val is not string valString)
            {
                throw new OclException("f2c function expecting a single double argument. Unable to parse value");
            }
            return Convert.FromBase64String(valString);
        }

        public IEnumerable<object?> ToOclFunctionCall(object propertyValue)
        {
            if (propertyValue is Byte[] bytes)
            {
                var fahrenheit = Convert.ToBase64String(bytes);
                return new object?[] { fahrenheit };
            }

            throw new InvalidOperationException($"The {Name} OCL function currently only supports byte arrays");
        }
    }
}