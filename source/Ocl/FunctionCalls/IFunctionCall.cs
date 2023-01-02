using System;
using System.Reflection;

namespace Octopus.Ocl.FunctionCalls
{
    public interface IFunctionCall
    {
        string Name {get;}
        object? ToValue(OclFunctionCall functionCall);
        OclFunctionCall? ToOclFunctionCall(object obj, PropertyInfo propertyInfo);

        OclFunctionCall? ToOclFunctionCall(object[] arguments);
    }
}