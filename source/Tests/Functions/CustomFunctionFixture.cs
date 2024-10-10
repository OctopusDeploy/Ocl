using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Converters;
using Octopus.Ocl.FunctionCalls;

namespace Tests.Functions
{
    public class CustomFunctionFixture
    {
        [Test]
        public void TwoWayFunctionIsReversible()
        {
            var car = new Car()
            {
                Name = "Hatchback",
                Engine = new Engine()
                {
                    TempC = 67
                },
            };

            var ocl = CreateSerializer().Serialize(car);
            ocl = @"name = ""Hatchback""

engine {
    temp_c = f2c(152.6)
}";

            CreateSerializer().Deserialize<Car>(ocl)
                .Should()
                .BeEquivalentTo(car);
        }

        [Test]
        public void StringLookingLikeFunctionRemainsString()
        {
            var car = new Car()
            {
                Name = "f2c(152.6)"
            };

            var ocl = CreateSerializer().Serialize(car);
            ocl.Should()
                .BeEquivalentTo(@"name = ""f2c(152.6)""");

            CreateSerializer().Deserialize<Car>(ocl)
                .Should()
                .BeEquivalentTo(car);
        }

        
        OclSerializer CreateSerializer()
        {
            return new OclSerializer(new OclSerializerOptions()
            {
                Converters = new List<IOclConverter>()
                {
                    new EngineConverter()
                },
                Functions = new List<IFunctionCall>()
                {
                    new FahrenheitToCelsiusFunction()
                }
            });
        }

        class EngineConverter : DefaultBlockOclConverter
        {
            public override bool CanConvert(Type type)
                => type == typeof(Engine);

            protected override IEnumerable<IOclElement> PropertyToElements(object obj, OclConversionContext context, PropertyInfo propertyInfo)
            {
                if (propertyInfo.Name == nameof(Engine.TempC))
                {
                    return context.PropertyToOclFunction(propertyInfo.GetValue(obj), propertyInfo, FahrenheitToCelsiusFunction.FnName);
                }

                return base.PropertyToElements(obj, context, propertyInfo);
            }
        }

        class FahrenheitToCelsiusFunction : IFunctionCall
        {
            public static readonly string FnName = "f2c";
            public string Name => FnName;

            public object? ToValue(IEnumerable<object?> arguments)
            {
                var val = arguments.FirstOrDefault();
                if (val == null)
                {
                    return null;
                }
                
                if (val == null || !double.TryParse(val.ToString(), out var fahrenheit))
                {
                    throw new OclException("f2c function expecting a single double argument. Unable to parse value");
                }

                return (fahrenheit - 32) * 5 / 9;
            }

            public IEnumerable<object?> ToOclFunctionCall(object propertyValue)
            {
                if (!double.TryParse(propertyValue.ToString(), out var celsius))
                {
                    throw new OclException("f2c function expecting a double argument. Unable to parse value");
                }

                var fahrenheit = (celsius * 9 / 5) + 32;
                return new object?[] { fahrenheit };
            }
        }

        class Car
        {
            public string? Name { get; set; }
            public byte[]? Image { get; set; }
            public Engine? Engine { get; set; }
        }

        class Engine
        {
            public double TempC { get; set; }
        }
    }
}