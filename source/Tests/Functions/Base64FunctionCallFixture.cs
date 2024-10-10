using System;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.Functions
{
    public class Base64FunctionCallFixture
    {
        [Test]
        public void TwoWayFunctionIsReversible()
        {
            var obj = new TestObject()
            {
                WithFunctionAttribute = new byte[]{72, 101, 108, 108, 111},
                WithoutFunctionAttribute = new byte[]{72, 101, 108, 108, 111}
            };

            var ocl = CreateSerializer().Serialize(obj);
            ocl = @"with_function_attribute = base64decode(""SGVsbG8="")
            without_function_attribute = [72, 101, 108, 108, 111]";

            CreateSerializer().Deserialize<TestObject>(ocl)
                .Should()
                .BeEquivalentTo(obj);
        }
        
        OclSerializer CreateSerializer()
        {
            return new OclSerializer(new OclSerializerOptions());
        }

        class TestObject
        {
            [OclFunction("base64decode")]
            public byte[] WithFunctionAttribute { get; set; } = Array.Empty<byte>();
            
            public byte[] WithoutFunctionAttribute { get; set; } = Array.Empty<byte>();
        }
    }
}