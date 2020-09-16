using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.Converters
{
    public class DefaultCollectionOclConverterFixture
    {
        const string Value = "Daffy";

        readonly OclConversionContext context = new OclConversionContext(new OclSerializerOptions());

        [Test]
        public void FromElement_IEnumerableTargetWithCurrentAsNullReturnsAList()
            => ExecuteFromElement<IEnumerable<string>>(null)
                .Should()
                .BeOfType<List<string>>()
                .And
                .BeEquivalentTo(new[] { Value });

        [Test]
        public void FromElement_IListTargetWithCurrentAsNullReturnsAList()
            => ExecuteFromElement<IEnumerable<string>>(null)
                .Should()
                .BeOfType<List<string>>()
                .And
                .BeEquivalentTo(new[] { Value });

        [Test]
        public void FromElement_HashSetTargetWithCurrentAsNullReturnsAHashSet()
            => ExecuteFromElement<HashSet<string>>(null)
                .Should()
                .BeOfType<HashSet<string>>()
                .And
                .BeEquivalentTo(new[] { Value });

        [Test]
        public void FromElement_ReusesCurrentCollection()
        {
            var existing = new HashSet<string>() { "ExistingItem" };
            ExecuteFromElement<HashSet<string>>(existing)
                .Should()
                .BeSameAs(existing)
                .And
                .BeEquivalentTo(new[] { "ExistingItem", Value });
        }

        object? ExecuteFromElement<TTarget>(object? currentValue)
            => new DefaultCollectionOclConverter()
                .FromElement(context, typeof(TTarget), new OclAttribute("Test", Value), () => currentValue);
    }
}