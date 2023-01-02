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
                }
            };

            var ocl = CreateSerializer().Serialize(car);
            ocl.Should()
                .BeEquivalentTo(@"name = ""Hatchback""

engine {
    temp_c = f2c(152.6)
}");

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

        [Test]
        public void UnknownFunctionThrows()
        {
            Action action = () =>
            {
                CreateSerializer()
                    .Deserialize<Car>(new OclDocument()
                    {
                        new OclAttribute("name", new OclFunctionCall("somefakefunction", new object?[] { 11, "zoom" }))
                    });
            };
            action.Should()
                .Throw<OclException>()
                .WithMessage("Call to unknown function. There is no function named \"somefakefunction\"");
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
                    var val = context.GetFunctionCallFor(FahrenheitToCelsiusFunction.FnName).ToOclFunctionCall(obj, propertyInfo);
                    return new IOclElement[] { new OclAttribute(context.Namer.GetName(propertyInfo!), val) };

                }

                return base.PropertyToElements(obj, context, propertyInfo);
            }
        }

        class FahrenheitToCelsiusFunction : IFunctionCall
        {
            public static string FnName = "f2c";
            public string Name { get; } = FnName;

            public object? ToValue(OclFunctionCall functionCall)
            {
                if (!functionCall.Arguments.Any())
                {
                    return null;
                }

                var val = functionCall.Arguments.First();
                if (val == null || !double.TryParse(val.ToString(), out var fahrenheit))
                {
                    throw new OclException("f2c function expecting a single double argument. Unable to parse value");
                }

                return (fahrenheit - 32) * 5 / 9;
            }

            public OclFunctionCall? ToOclFunctionCall(object obj, PropertyInfo propertyInfo)
            {
                var arg = propertyInfo.GetValue(obj);
                if (arg == null)
                {
                    return null;
                }

                if (!double.TryParse(arg.ToString(), out var celsius))
                {
                    throw new OclException("f2c function expecting a double argument. Unable to parse value");
                }

                var fahrenheit = (celsius * 9 / 5) + 32;
                return new OclFunctionCall(FnName, new object?[] { fahrenheit });
            }

            public OclFunctionCall? ToOclFunctionCall(object[] arguments) => new(FnName, arguments);
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