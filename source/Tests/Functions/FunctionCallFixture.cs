using System;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;

namespace Tests.Functions
{
    public class FunctionCallFixture
    {
        [Test]
        public void UnknownFunctionThrows()
        {
            Action action = () =>
            {
                CreateSerializer()
                    .Deserialize<TestObject>(new OclDocument()
                    {
                        new OclAttribute("name", new OclFunctionCall("somefakefunction", new object?[] { 11, "zoom" }))
                    });
            };
            action.Should()
                .Throw<OclException>()
                .WithMessage("Call to unknown function. There is no function named \"somefakefunction\"");
        }

        class TestObject
        {
        }
        OclSerializer CreateSerializer()
        {
            return new OclSerializer(new OclSerializerOptions());
        }
    }
}