using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Octopus.Ocl.Converters;
using Octopus.Ocl.FunctionCalls;
using Octopus.Ocl.Namers;

namespace Octopus.Ocl
{
    /*public class FileFunction: IFunctionCall
    {
        public static string FnName = "file";
        public string Name { get; } = FnName;
        
        
        public object? ToValue(OclFunctionCall functionCall)
            => throw new NotImplementedException();

        public OclFunctionCall? ToOclFunctionCall(object obj, PropertyInfo propertyInfo)
            => throw new NotImplementedException();

        public OclFunctionCall? ToOclFunctionCall(object[] arguments)
            => throw new NotImplementedException();
    }*/
    
    public class OclConversionContext
    {
        readonly IReadOnlyList<IOclConverter> converters;
        
        readonly IReadOnlyList<IFunctionCall> functions;

        public OclConversionContext(OclSerializerOptions options)
        {
            converters = options
                .Converters
                .Concat(
                    new IOclConverter[]
                    {
                        new EnumAttributeOclConverter(),
                        new DefaultAttributeOclConverter(),
                        new DefaultCollectionOclConverter(),
                        new DefaultBlockOclConverter()
                    })
                .ToArray();

            functions = options.Functions.Concat(new IFunctionCall[]
            {
                //TODO: Add some built-in functions
            }).ToArray();

            Namer = options.Namer;
        }

        internal IOclNamer Namer { get; }

        public IOclConverter GetConverterFor(Type type)
        {
            foreach (var converter in converters)
                if (converter.CanConvert(type))
                    return converter;

            throw new Exception("Could not find a converter for " + type.FullName);
        }

        public IFunctionCall GetFunctionCallFor(string name)
        {
            var fnCall = functions.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (fnCall == null)
            {
                throw new OclException($"Call to unknown function. "
                    + $"There is no function named \"{name}\"");
            }

            return fnCall;
        }

        internal IEnumerable<IOclElement> ToElements(PropertyInfo? propertyInfo, object? value)
        {
            if (value == null)
                return new IOclElement[0];

            return GetConverterFor(value.GetType())
                .ToElements(this, propertyInfo, value);
        }

        internal object? FromElement(Type type, IOclElement element, object? getCurrentValue)
            => GetConverterFor(type)
                .FromElement(this, type, element, getCurrentValue);
    }
}