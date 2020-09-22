using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;

namespace Tests.FromOclDoc
{
    public class DeserializeDefaultBehaviourFixture
    {
        [Test]
        public void Empty()
        {
            OclConvert.Deserialize<Car>(new OclDocument(), new OclSerializerOptions())
                .Should()
                .BeEquivalentTo(new Car());
        }

        [Test]
        public void IntAttribute()
        {
            var document = new OclDocument()
            {
                new OclAttribute("Doors", 4)
            };

            OclConvert.Deserialize<Car>(document, new OclSerializerOptions())
                .Should()
                .BeEquivalentTo(new Car() { Doors = 4 });
        }

        [Test]
        public void IntNullAttribute()
        {
            var document = new OclDocument()
            {
                new OclAttribute("Doors", null)
            };

            OclConvert.Deserialize<Car>(document, new OclSerializerOptions())
                .Should()
                .BeEquivalentTo(new Car() { Doors = null });
        }

        [Test]
        public void StringAttribute()
        {
            var document = new OclDocument()
            {
                new OclAttribute("Name", "Mystery Machine")
            };

            OclConvert.Deserialize<Car>(document, new OclSerializerOptions())
                .Should()
                .BeEquivalentTo(new Car() { Name = "Mystery Machine" });
        }

        [Test]
        public void EnumAttribute()
        {
            var document = new OclDocument()
            {
                new OclAttribute("Type", "Suv")
            };

            OclConvert.Deserialize<Car>(document, new OclSerializerOptions())
                .Should()
                .BeEquivalentTo(new Car() { Type = CarType.Suv });
        }

        [Test]
        public void CaseIsIgnoredAttribute()
        {
            var document = new OclDocument()
            {
                new OclAttribute("nAMe", "Mystery Machine")
            };

            OclConvert.Deserialize<Car>(document, new OclSerializerOptions())
                .Should()
                .BeEquivalentTo(new Car() { Name = "Mystery Machine" });
        }

        [Test]
        public void Block()
        {
            var document = new OclDocument()
            {
                new OclBlock("Driver")
                {
                    new OclAttribute("Name", "Bob")
                }
            };

            OclConvert.Deserialize<Car>(document, new OclSerializerOptions())
                .Should()
                .BeEquivalentTo(new Car()
                {
                    Driver = new Person() { Name = "Bob" }
                });
        }

        [Test]
        public void CollectionSingleItem()
        {
            var document = new OclDocument()
            {
                new OclBlock("Passengers")
                {
                    new OclAttribute("Name", "Bob")
                }
            };

            OclConvert.Deserialize<Car>(document, new OclSerializerOptions())
                .Should()
                .BeEquivalentTo(new Car()
                {
                    Passengers = new List<Person>
                    {
                        new Person() { Name = "Bob" }
                    }
                });
        }

        [Test]
        public void CollectionMultipleItems()
        {
            var document = new OclDocument()
            {
                new OclBlock("Passengers")
                {
                    new OclAttribute("Name", "Bob")
                },
                new OclBlock("Passengers")
                {
                    new OclAttribute("Name", "George")
                }
            };

            OclConvert.Deserialize<Car>(document, new OclSerializerOptions())
                .Should()
                .BeEquivalentTo(new Car()
                {
                    Passengers = new List<Person>
                    {
                        new Person() { Name = "Bob" },
                        new Person() { Name = "George" }
                    }
                });
        }

        [Test]
        public void ExceptionIsThrownIfPropertyDoesNotExist()
        {
            var document = new OclDocument()
            {
                new OclAttribute("Wings", 1)
            };

            Action action = () => OclConvert.Deserialize<Car>(document, new OclSerializerOptions());
            action.Should()
                .Throw<OclException>()
                .WithMessage("*The property 'Wings' was not found on 'Car'*");
        }

        [Test]
        public void ExceptionIsThrownIfPropertyCantBeSet()
        {
            var document = new OclDocument()
            {
                new OclAttribute("ReadOnly", 1)
            };

            Action action = () => OclConvert.Deserialize<Car>(document, new OclSerializerOptions());
            action.Should()
                .Throw<OclException>()
                .WithMessage("*The property 'ReadOnly' on 'Car' does not have a setter*");
        }

        [Test]
        public void ReadOnlyPropertiesWorkIfTheReferenceMatches()
        {
            var document = new OclDocument()
            {
                new OclBlock("ReadOnlyPassengers")
                {
                    new OclAttribute("Name", "Bob")
                }
            };

            var expected = new Car();
            expected.ReadOnlyPassengers.Add(new Person() { Name = "Bob" });

            OclConvert.Deserialize<Car>(document, new OclSerializerOptions())
                .Should()
                .BeEquivalentTo(expected);
        }

        class Car
        {
            public string Name { get; set; } = "";
            public int? Doors { get; set; }
            public int ReadOnly { get; } = 1;
            public Person? Driver { get; set; }
            public List<Person>? Passengers { get; set; }
            public CarType Type { get; set; }
            public List<Person> ReadOnlyPassengers { get; } = new List<Person>();
        }

        enum CarType
        {
            Hatchback,
            Suv
        }

        class Person
        {
            public string Name { get; set; } = "";
        }
    }
}