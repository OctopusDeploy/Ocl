using System;
using System.Collections.Generic;

namespace Octopus.Ocl.FunctionCalls
{
    public interface IFunctionCall
    {
        string Name {get;}
        object? ToValue(IEnumerable<object?> arguments);
        IEnumerable<object?> ToOclFunctionCall(object propertyValue);
    }
}