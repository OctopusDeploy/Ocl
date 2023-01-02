using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Octopus.Ocl
{
    [DebuggerDisplay("{Name}({Arguments})", Name = "OclFunctionCall")]
    public class OclFunctionCall : IOclElement
    {
        string name;
        IEnumerable<object?> arguments;

        public OclFunctionCall(string name, IEnumerable<object?> arguments)
        {
            this.name = Name = name; // Make the compiler happy
            this.arguments = arguments;
        }

        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new OclException("FunctionCalls must have an identifier name");
                name = value;
            }
        }

        /// <remarks>
        /// The attribute value is given as an expression, which is retained literally for later evaluation by the calling application.
        /// </remarks>
        public IEnumerable<object?> Arguments
        {
            get => arguments;
            set
            {
                var invalidArg = arguments.Where(a => a != null && !OclAttribute.IsSupportedValueType(a.GetType()))
                    .Select(t => t?.GetType().FullName).Distinct().ToArray();
                if(invalidArg.Any())
                {
                    var msg = (invalidArg.Length == 1) ?
                        $"The type {invalidArg} is not a supported value type for an OCL function call argument" :
                        $"The types {string.Join(',', invalidArg)} are not a supported value types for an OCL function call argument";
                    throw new OclException(msg);
                }

                this.arguments = value;
            }
        }
    }
}