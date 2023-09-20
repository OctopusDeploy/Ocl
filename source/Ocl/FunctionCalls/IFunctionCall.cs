using System;
using System.Collections.Generic;

namespace Octopus.Ocl.FunctionCalls
{
    public interface IFunctionCall
    {
        string Name {get;}
        
        // Called during deserialization when converting from the OCL representation to a single property value.
        object? ToValue(IEnumerable<object?> arguments);
        
        // Called during serialization and allows for a single object value to be represented by the function call
        // as being defined with non or many arguments.
        IEnumerable<object?> ToOclFunctionCall(object propertyValue);
    }
}