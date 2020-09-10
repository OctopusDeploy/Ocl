using Octopus.Ocl;

namespace Tests
{
    public static class AssertionExtensions
    {
        public static OclBodyAssertions Should(this OclBody? subject)
            => new OclBodyAssertions(subject);
    }
}
