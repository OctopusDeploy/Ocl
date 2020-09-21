using FluentAssertions;
using NUnit.Framework;
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
    }
}