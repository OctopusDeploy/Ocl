using System;
using FluentAssertions.Primitives;
using Octopus.Ocl;

namespace Tests
{
    public class OclBodyAssertions
    {
        private readonly OclBody? subject;

        public OclBodyAssertions(OclBody? subject)
        {
            this.subject = subject;
        }

        public void Be(OclBody expected)
        {
            new ObjectAssertions(subject).BeEquivalentTo(
                expected,
                config => config.IncludingAllRuntimeProperties()
            );
        }

        public void BeNull()
            => new ObjectAssertions(subject).BeNull();

        public void HaveChildrenExactly(params IOclElement[] expectedChildren)
            => Be(new OclDocument(expectedChildren));
    }
}
