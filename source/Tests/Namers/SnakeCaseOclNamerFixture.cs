using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Namers;

namespace Tests.Namers
{
    public class SnakeCaseOclNamerFixture
    {
        [TestCase("SnakeCase", "snake_case")]
        [TestCase("MultipleCAPitals", "multiple_capitals")]
        [TestCase("", "")]
        [TestCase("ALLUPPER", "allupper")]
        [TestCase("alllower", "alllower")]
        [TestCase("With A Space", "with_a_space")]
        public void FormatName(string input, string expected)
            => new SnakeCaseOclNamer().FormatName(input).Should().Be(expected);

        [Test]
        public void GetPropertyNotFound()
            => new SnakeCaseOclNamer()
                .GetProperty("DoesNotExist", GetTestProperties())
                .Should()
                .BeNull();

        [TestCase("MyProperty")]
        [TestCase("myproperty")]
        [TestCase("myproPERTy")]
        [TestCase("My_Property")]
        [TestCase("my_property")]
        public void GetPropertySingleMatch(string name)
            => new SnakeCaseOclNamer()
                .GetProperty(name, GetTestProperties())
                .Should()
                .NotBeNull()
                .And.Subject.Name.Should()
                .Be("MyProperty");

        [Test]
        public void GetPropertyMultipleMatch()
        {
            Action action = () => new SnakeCaseOclNamer().GetProperty("DuplICAte", GetTestProperties());
            action.Should().Throw<OclException>().WithMessage("Multiple properties match the name 'DuplICAte': duplicate, Duplicate");
        }

        IReadOnlyCollection<PropertyInfo> GetTestProperties()
            => new
                {
                    MyProperty = "",
                    duplicate = "",
                    Duplicate = ""
                }.GetType()
                .GetProperties();
    }
}