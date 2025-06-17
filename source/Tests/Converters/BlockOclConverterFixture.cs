using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.Converters
{
    public class BlockOclConverterFixture
    {
        [Test]
        public void NameCaseIsKept()
        {
            var context = new OclConversionContext(new OclSerializerOptions());
            var data = new object();
            var result = (OclBlock)new DefaultBlockOclConverter().ToElements(context, typeof(WithIndexer).GetProperty(nameof(WithIndexer.MyProp))!, data).Single();
            result.Name.Should().Be("my_prop");
        }

        [Test]
        public void AttributesComeBeforeBlocks()
        {
            var context = new OclConversionContext(new OclSerializerOptions());
            var data = new
            {
                MyBlock = new { BlockProp = "OtherValue" },
                MyProp = "MyValue"
            };
            var result = (OclBlock)new DefaultBlockOclConverter().ToElements(context, typeof(WithIndexer).GetProperty(nameof(WithIndexer.MyProp)), data).Single();
            result.First()
                .Should()
                .BeEquivalentTo(new OclAttribute("my_prop", "MyValue"));
        }

        [Test]
        public void IndexersAreIgnored()
        {
            var context = new OclConversionContext(new OclSerializerOptions());
            var data =
                new Dummy
                {
                    Foo = new WithIndexer()
                };
            var result = (OclBlock)new DefaultBlockOclConverter().ToElements(context, typeof(Dummy).GetProperty(nameof(Dummy.Foo)), data.Foo).Single();
            result.Should()
                .Be(
                    new OclBlock("foo")
                    {
                        new OclAttribute("my_prop", "MyValue")
                    }
                );
        }

        [Test]
        public void CanGetPropertiesWithNonPublicSettersInAbstractBaseClass()
        {
            // Arrange
            var context = new OclConversionContext(new OclSerializerOptions());
            var data = new ConcreteImplementation("TestPrivateValue", "TestProtectedValue");
            // Act
            var result = (OclBlock)new DefaultBlockOclConverter()
                .ToElements(context,
                    typeof(ConcreteImplementation)
                        .GetProperty(nameof(ConcreteImplementation)),
                    data)
                .Single();

            // Assert
            result.Should()
                .Be(
                    new OclBlock("concrete_implementation")
                    {
                        new OclAttribute("base_private_property", "TestPrivateValue"),
                        new OclAttribute("base_protected_property", "TestProtectedValue")
                    }
                );
        }

        [Test]
        public void CanSetPropertiesWithNonPublicSetters()
        {
            // Arrange
            var context = new OclConversionContext(new OclSerializerOptions());
            var converter = new DefaultBlockOclConverter();

            // Create an OCL block with a property that corresponds to a private setter in the abstract base class
            var oclBlock = new OclBlock("class_with_non_public_setter")
            {
                new OclAttribute("private_foo", "TestPrivateValue"),
                new OclAttribute("protected_foo", "TestProtectedValue")
            };

            // Act
            var result = converter.FromElement(context, typeof(ClassWithNonPublicSetter), oclBlock, null) as ClassWithNonPublicSetter;

            // Assert
            result.Should().NotBeNull();
            result!.PrivateFoo.Should().Be("TestPrivateValue");
            result!.ProtectedFoo.Should().Be("TestProtectedValue");
            result!.NoSetter.Should().Be("NoSetterValue");
        }

        [Test]
        public void ThrowsExceptionWhenNoSetters()
        {
            // Arrange
            var context = new OclConversionContext(new OclSerializerOptions());
            var converter = new DefaultBlockOclConverter();

            // Create an OCL block with a property that corresponds to a private setter in the abstract base class
            var oclBlock = new OclBlock("class_with_non_public_setter")
            {
                new OclAttribute("no_setter", "ShouldThrowException"),
            };

            // Act
            var ex = Assert.Throws<OclException>(
                () => converter.FromElement(context, typeof(ClassWithNonPublicSetter), oclBlock, null)
            );
            ex.Should().NotBeNull();
            ex!.Message.Should().Contain("The property 'NoSetter' on 'ClassWithNonPublicSetter' does not have a setter");
        }
        
        [Test]
        public void ThrowsExceptionWhenNoSettersOnAbstractClass()
        {
            // Arrange
            var context = new OclConversionContext(new OclSerializerOptions());
            var converter = new DefaultBlockOclConverter();

            // Create an OCL block with a property that corresponds to a private setter in the abstract base class
            var oclBlock = new OclBlock("concrete_implementation")
            {
                new OclAttribute("no_setter", "ShouldThrowException")
            };

            // Act
            var ex = Assert.Throws<OclException>(
                () => converter.FromElement(context, typeof(ConcreteImplementation), oclBlock, null)
            );
            ex.Should().NotBeNull();
            ex!.Message.Should().Contain("The property 'NoSetter' on 'ConcreteImplementation' does not have a setter");
        }

        [Test]
        public void CanSetPropertiesWithNonPublicSettersInAbstractBaseClass()
        {
            // Arrange
            var context = new OclConversionContext(new OclSerializerOptions());
            var converter = new DefaultBlockOclConverter();

            // Create an OCL block with a property that corresponds to a private setter in the abstract base class
            var oclBlock = new OclBlock("concrete_implementation")
            {
                new OclAttribute("base_private_property", "TestPrivateValue"),
                new OclAttribute("base_protected_property", "TestProtectedValue")
            };

            // Act
            var result = converter.FromElement(context, typeof(ConcreteImplementation), oclBlock, null) as ConcreteImplementation;

            // Assert
            result.Should().NotBeNull();
            result!.BasePrivateProperty.Should().Be("TestPrivateValue");
            result!.BaseProtectedProperty.Should().Be("TestProtectedValue");
        }

        class WithIndexer
        {
            public string MyProp => "MyValue";
            public string this[int index] => throw new NotImplementedException();
        }

        class Dummy
        {
            public object? Foo { get; set; }
        }

        class ClassWithNonPublicSetter
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string? PrivateFoo { get; private set; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string? ProtectedFoo { get; protected set; }

            public string? NoSetter { get; } = "NoSetterValue";
        }

        // Test classes for abstract base class scenario
        public abstract class AbstractBase
        {
            // ReSharper disable once ConvertToPrimaryConstructor
            protected AbstractBase(string? basePrivateProperty, string? baseProtectedProperty)
            {
                // Initialize properties in the constructor
                BasePrivateProperty = basePrivateProperty ?? string.Empty;
                BaseProtectedProperty = baseProtectedProperty ?? string.Empty;
            }

            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
            public string BasePrivateProperty { get; private set; }

            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
            public string BaseProtectedProperty { get; private set; }
            
            public string? NoSetter { get; } = null;
        }

        public class ConcreteImplementation : AbstractBase
        {
            // Parameterless constructor for OCL converter
            public ConcreteImplementation()
                : base(string.Empty, string.Empty)
            {
            }

            // This class inherits BaseProperties but the setter is private or protected in the base class
            // The OclConverter should still be able to set it via reflection
            // ReSharper disable once ConvertToPrimaryConstructor
            public ConcreteImplementation(string? basePrivateProperty, string? baseProtectedProperty)
                : base(basePrivateProperty, baseProtectedProperty)
            {
            }
        }
    }
}