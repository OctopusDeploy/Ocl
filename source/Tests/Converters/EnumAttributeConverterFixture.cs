using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.Converters
{
    internal class EnumAttributeConverterFixture
    {
        [Test]
        public void WhenConvertingEnum_ValueIsEnumDefault_ReturnsNull()
        {
            var context = new OclConversionContext(new OclSerializerOptions());

            var result = (OclAttribute?)new EnumAttributeOclConverter().ToElements(context, typeof(Dummy).GetProperty(nameof(Dummy.TestProperty))!, Test.Value1).SingleOrDefault();

            result.Should().BeNull();
        }

        [Test]
        public void WhenConvertingEnum_ValueIsNotEnumDefault_ReturnsValueOfProperty()
        {
            var context = new OclConversionContext(new OclSerializerOptions());

            var result = (OclAttribute?)new EnumAttributeOclConverter().ToElements(context, typeof(Dummy).GetProperty(nameof(Dummy.TestProperty))!, Test.Value2).SingleOrDefault();

            result.Should().NotBeNull();
            result!.Value.Should().Be("Value2");
        }

        [Test]
        public void WhenConvertingEnumWithDefaultAttribute_ValueIsEnumDefault_ReturnsValueOfProperty()
        {
            var context = new OclConversionContext(new OclSerializerOptions());

            var result = (OclAttribute?)new EnumAttributeOclConverter().ToElements(context, typeof(DummyWithDefault).GetProperty(nameof(DummyWithDefault.TestProperty))!, Test.Value1).SingleOrDefault();

            result.Should().NotBeNull();
            result!.Value.Should().Be("Value1");
        }

        [Test]
        public void WhenConvertingEnumWithDefaultAttribute_ValueIsNotADefault_ReturnsValueOfProperty()
        {
            var context = new OclConversionContext(new OclSerializerOptions());

            var result = (OclAttribute?)new EnumAttributeOclConverter().ToElements(context, typeof(DummyWithDefault).GetProperty(nameof(DummyWithDefault.TestProperty))!, Test.Value2).SingleOrDefault();

            result.Should().NotBeNull();
            result!.Value.Should().Be("Value2");
        }

        [Test]
        public void WhenConvertingEnumWithDefaultAttribute_ValueIstAttributedDefault_ReturnsNull()
        {
            var context = new OclConversionContext(new OclSerializerOptions());

            var result = (OclAttribute?)new EnumAttributeOclConverter().ToElements(context, typeof(DummyWithDefault).GetProperty(nameof(DummyWithDefault.TestProperty))!, Test.Value3).SingleOrDefault();

            result.Should().BeNull();
        }

        enum Test
        {
            Value1,
            Value2,
            Value3,
        }

        class Dummy
        {
            public Test TestProperty { get; } = Test.Value1;
        }

        class DummyWithDefault
        {
            [OclDefaultEnumValue(Test.Value3)]
            public Test TestProperty { get; } = Test.Value1;
        }
    }
}