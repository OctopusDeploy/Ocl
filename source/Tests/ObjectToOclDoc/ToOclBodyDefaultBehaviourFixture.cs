using System;
using System.Collections.Generic;
using NUnit.Framework;
using Octopus.Ocl;

namespace Tests.ObjectToOclDoc
{
    public class ToOclBodyDefaultBehaviourFixture
    {
        [Test]
        public void Null()
        {
            OclConvert.ToOclDocument(null, new OclSerializerOptions())
                .Should()
                .Be(new OclDocument());
        }

        [Test]
        public void SimpleObject()
        {
            OclConvert.ToOclDocument(new { MyProp = "MyValue" }, new OclSerializerOptions())
                .Should()
                .HaveChildrenExactly(
                    new OclAttribute("MyProp", "MyValue")
                );
        }

        [Test]
        public void ListOfComplexTypesProperty()
        {
            var data = new
            {
                Cars = new List<Car>() { new Car(), new Car() }
            };

            var result = OclConvert.ToOclDocument(data, new OclSerializerOptions());
            result.Should()
                .HaveChildrenExactly(
                    new OclBlock("car")
                    {
                        new OclAttribute("Doors", 2)
                    },
                    new OclBlock("car")
                    {
                        new OclAttribute("Doors", 2)
                    }
                );
        }

        class Car
        {
            public int Doors { get; } = 2;
        }
    }
}