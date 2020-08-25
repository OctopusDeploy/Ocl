using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Hcl;

namespace Tests.ObjectToHDoc
{
    public class ToHDocDefaultBehaviourFixture
    {
        [Test]
        public void Null()
        {
            HclConvert.ToHDocument(null, new HclSerializerOptions())
                .Should()
                .NotBeNull()
                .And
                .BeEmpty();
        }

        [Test]
        public void SimpleObject()
        {
            HclConvert.ToHDocument(new { MyProp = "MyValue" }, new HclSerializerOptions())
                .Should()
                .BeEquivalentTo(
                    new HDocument()
                    {
                        new HAttribute("MyProp", "MyValue")
                    },
                    config => config.IncludingAllRuntimeProperties()
                );
        }

        [Test]
        public void ListOfComplexTypesProperty()
        {
            var data = new
            {
                Cars = new List<Car>() { new Car(), new Car() }
            };

            var result = HclConvert.ToHDocument(data, new HclSerializerOptions());
            result.Should()
                .BeEquivalentTo(new[]
                {
                    new HBlock("car")
                    {
                        new HAttribute("Doors", 2)
                    },
                    new HBlock("car")
                    {
                        new HAttribute("Doors", 2)
                    }
                });
        }

        class Car
        {
            public int Doors { get; } = 2;
        }
    }
}