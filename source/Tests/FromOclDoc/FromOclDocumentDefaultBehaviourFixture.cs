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
            var document = new OclDocument();
            var context = new OclConversionContext(new OclSerializerOptions());
            var result = context.FromElement(typeof(Car), document, null);
            if (result == null)
                throw new OclException("Document conversion resulted in null, which is not valid");
            ((Car)result)
                .Should()
                .BeEquivalentTo(new Car());
        }

        [Test]
        public void IntAttribute()
        {
            var document = new OclDocument
            {
                new OclAttribute("Doors", 4)
            };

            var context = new OclConversionContext(new OclSerializerOptions());
            var result = context.FromElement(typeof(Car), document, null);
            if (result == null)
                throw new OclException("Document conversion resulted in null, which is not valid");
            ((Car)result)
                .Should()
                .BeEquivalentTo(new Car
                    { Doors = 4 });
        }

        [Test]
        public void IntAttributeToString()
        {
            var document = new OclDocument
            {
                new OclAttribute("Name", 4)
            };

            new OclSerializer().Deserialize<Car>(document)
                .Should()
                .BeEquivalentTo(new Car
                    { Name = "4" });
        }

        [Test]
        public void IntNullAttribute()
        {
            var document = new OclDocument
            {
                new OclAttribute("Doors", null)
            };

            new OclSerializer().Deserialize<Car>(document)
                .Should()
                .BeEquivalentTo(new Car
                    { Doors = null });
        }

        [Test]
        public void StringAttribute()
        {
            var document = new OclDocument
            {
                new OclAttribute("Name", "Mystery Machine")
            };

            var context = new OclConversionContext(new OclSerializerOptions() ?? new OclSerializerOptions());
            var result = context.FromElement(typeof(Car), document, null);
            if (result == null)
                throw new OclException("Document conversion resulted in null, which is not valid");
            ((Car)result)
                .Should()
                .BeEquivalentTo(new Car
                    { Name = "Mystery Machine" });
        }

        [Test]
        public void EnumAttribute()
        {
            var document = new OclDocument
            {
                new OclAttribute("Type", "Suv")
            };

            new OclSerializer().Deserialize<Car>(document)
                .Should()
                .BeEquivalentTo(new Car
                    { Type = CarType.Suv });
        }

        [Test]
        public void DictionaryOfStrings()
        {
            var dictionary = new Dictionary<string, object?>
            {
                { "One", "1" },
                { "Two", 2 }
            };

            var document = new OclDocument
            {
                new OclAttribute("StringDictionary", dictionary)
            };

            new OclSerializer().Deserialize<Car>(document)
                .Should()
                .BeEquivalentTo(new Car
                {
                    StringDictionary = new Dictionary<string, string>
                    {
                        { "One", "1" },
                        { "Two", "2" }
                    }
                });
        }

        [Test]
        public void DictionaryOfObjects()
        {
            var dictionary = new Dictionary<string, object?>
            {
                { "One", "1" },
                { "Two", "2" }
            };

            var document = new OclDocument
            {
                new OclAttribute("ObjectDictionary", dictionary)
            };

            new OclSerializer().Deserialize<Car>(document)
                .Should()
                .BeEquivalentTo(new Car
                {
                    ObjectDictionary = dictionary
                });
        }

        [Test]
        public void CaseIsIgnoredAttribute()
        {
            var document = new OclDocument
            {
                new OclAttribute("nAMe", "Mystery Machine")
            };

            var context = new OclConversionContext(new OclSerializerOptions() ?? new OclSerializerOptions());
            var result = context.FromElement(typeof(Car), document, null);
            if (result == null)
                throw new OclException("Document conversion resulted in null, which is not valid");
            ((Car)result)
                .Should()
                .BeEquivalentTo(new Car
                    { Name = "Mystery Machine" });
        }

        [Test]
        public void Block()
        {
            var document = new OclDocument
            {
                new OclBlock("Driver")
                {
                    new OclAttribute("Name", "Bob")
                }
            };

            var context = new OclConversionContext(new OclSerializerOptions() ?? new OclSerializerOptions());
            var result = context.FromElement(typeof(Car), document, null);
            if (result == null)
                throw new OclException("Document conversion resulted in null, which is not valid");
            ((Car)result)
                .Should()
                .BeEquivalentTo(new Car
                {
                    Driver = new Person
                        { Name = "Bob" }
                });
        }

        [Test]
        public void CollectionSingleItem()
        {
            var document = new OclDocument
            {
                new OclBlock("Passengers")
                {
                    new OclAttribute("Name", "Bob")
                }
            };

            var context = new OclConversionContext(new OclSerializerOptions() ?? new OclSerializerOptions());
            var result = context.FromElement(typeof(Car), document, null);
            if (result == null)
                throw new OclException("Document conversion resulted in null, which is not valid");
            ((Car)result)
                .Should()
                .BeEquivalentTo(new Car
                {
                    Passengers = new List<Person>
                    {
                        new()
                            { Name = "Bob" }
                    }
                });
        }

        [Test]
        public void CollectionMultipleItems()
        {
            var document = new OclDocument
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

            var context = new OclConversionContext(new OclSerializerOptions() ?? new OclSerializerOptions());
            var result = context.FromElement(typeof(Car), document, null);
            if (result == null)
                throw new OclException("Document conversion resulted in null, which is not valid");
            ((Car)result)
                .Should()
                .BeEquivalentTo(new Car
                {
                    Passengers = new List<Person>
                    {
                        new()
                            { Name = "Bob" },
                        new()
                            { Name = "George" }
                    }
                });
        }

        [Test]
        public void ExceptionIsThrownIfPropertyDoesNotExist()
        {
            var document = new OclDocument
            {
                new OclAttribute("Wings", 1)
            };

            var action = () =>
            {
                var context = new OclConversionContext(new OclSerializerOptions() ?? new OclSerializerOptions());
                var result = context.FromElement(typeof(Car), document, null);
                if (result == null)
                    throw new OclException("Document conversion resulted in null, which is not valid");
                var temp = (Car)result;
            };
            action.Should()
                .Throw<OclException>()
                .WithMessage("*The property 'Wings' was not found on 'Car'*");
        }

        [Test]
        public void ExceptionIsThrownIfPropertyCantBeSet()
        {
            var document = new OclDocument
            {
                new OclAttribute("ReadOnly", 1)
            };

            var action = () =>
            {
                var context = new OclConversionContext(new OclSerializerOptions() ?? new OclSerializerOptions());
                var result = context.FromElement(typeof(Car), document, null);
                if (result == null)
                    throw new OclException("Document conversion resulted in null, which is not valid");
                var temp = (Car)result;
            };
            action.Should()
                .Throw<OclException>()
                .WithMessage("*The property 'ReadOnly' on 'Car' does not have a setter*");
        }

        [Test]
        public void ReadOnlyPropertiesWorkIfTheReferenceMatches()
        {
            var document = new OclDocument
            {
                new OclBlock("ReadOnlyPassengers")
                {
                    new OclAttribute("Name", "Bob")
                }
            };

            var expected = new Car();
            expected.ReadOnlyPassengers.Add(new Person
                { Name = "Bob" });

            var context = new OclConversionContext(new OclSerializerOptions() ?? new OclSerializerOptions());
            var result = context.FromElement(typeof(Car), document, null);
            if (result == null)
                throw new OclException("Document conversion resulted in null, which is not valid");
            ((Car)result)
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
            public List<Person> ReadOnlyPassengers { get; } = new();
            public CarType Type { get; set; }
            public Dictionary<string, string>? StringDictionary { get; set; }
            public Dictionary<string, object?>? ObjectDictionary { get; set; }
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