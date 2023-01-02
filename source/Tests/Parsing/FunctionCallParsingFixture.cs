using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Parsing;

namespace Tests.Parsing
{
    public class FunctionCallParsingFixture
    {
        [Test]
        public void WithSingleArgument()
            => OclParser.Execute(@"Child = dateCalc(""today"")")
                .Should()
                .HaveChildrenExactly(
                    new OclAttribute("Child",
                        new OclFunctionCall("dateCalc", new[] { "today" }))
                );

        [Test]
        public void WithNoArgument()
            => OclParser.Execute(@"Child = dateCalc()")
                .Should()
                .HaveChildrenExactly(
                    new OclAttribute("Child",
                        new OclFunctionCall("dateCalc", new object?[] { }))
                );

        [Test]
        public void WithMultipleArgument()
            => OclParser.Execute(@"Child = dateCalc(12, ""cat"", null)")
                .Should()
                .HaveChildrenExactly(
                    new OclAttribute("Child",
                        new OclFunctionCall("dateCalc", new object?[] { 12, "cat", null }))
                );

        [Test]
        public void WithinBlock()
            => OclParser.Execute(@"
                Parent {
                    Child = dateCalc()
                }")
                .Should()
                .HaveChildrenExactly(new OclBlock("Parent")
                {
                    new OclAttribute("Child",
                        new OclFunctionCall("dateCalc", new object?[] { }))
                });
    }
}