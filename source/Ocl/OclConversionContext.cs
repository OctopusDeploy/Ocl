using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Octopus.Ocl.Converters;
using Octopus.Ocl.FunctionCalls;
using Octopus.Ocl.Namers;

namespace Octopus.Ocl
{
    public class OclConversionContext
    {
        readonly IReadOnlyList<IOclConverter> converters;
        
        readonly IReadOnlyDictionary<string, IFunctionCall> functions;

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
                new Base64DecodeFunctionCall()
            }).ToDictionary(func => func.Name, func => func);

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
            if(!functions.TryGetValue(name, out var fnCall)) {
                throw new OclException($"Call to unknown function. "
                    + $"There is no function named \"{name}\"");
            }

            return fnCall;
        }

        internal IEnumerable<IOclElement> ToElements(PropertyInfo? propertyInfo, object? value)
        {
            if (value == null)
                return Array.Empty<IOclElement>();
            
            if (propertyInfo != null 
                && propertyInfo.GetCustomAttribute(typeof(OclFunctionAttribute)) is OclFunctionAttribute oclFunctionAttribute 
                && !string.IsNullOrEmpty(oclFunctionAttribute.Name))
            {
                return PropertyToOclFunction(value, propertyInfo, oclFunctionAttribute.Name);
            }

            return GetConverterFor(value.GetType())
                .ToElements(this, propertyInfo, value);
        }

        internal IEnumerable<IOclElement> PropertyToOclFunction(object? propertyValue, PropertyInfo propertyInfo, string oclFunctionName)
        {
            object? attributeValue = null;
         
            if (propertyValue != null)
            {
                var convertedValues = GetFunctionCallFor(oclFunctionName).ToOclFunctionCall(propertyValue);
                attributeValue = new OclFunctionCall(oclFunctionName, convertedValues);
            }

            return new IOclElement[] { new OclAttribute(Namer.GetName(propertyInfo), attributeValue) };


        }

        internal object? FromElement(Type type, IOclElement element, object? getCurrentValue)
            => GetConverterFor(type)
                .FromElement(this, type, element, getCurrentValue);
    }
}