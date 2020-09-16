using System;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Parsing;

namespace Tests.Parsing
{
    public class BlockParsingFixture
    {
        [Test]
        public void EmptySingleLine()
            => OclParser.Execute(@"MyBlock {}")
                .Should()
                .HaveChildrenExactly(new OclBlock("MyBlock"));

        [Test]
        public void Empty()
            => OclParser.Execute(@"
                    MyBlock {

                    }
                ")
                .Should()
                .HaveChildrenExactly(new OclBlock("MyBlock"));

        [Test]
        public void MultipleInRoot()
            => OclParser.Execute(@"
                MyBlock {}
                MyBlock {}
                ")
                .Should()
                .HaveChildrenExactly(
                    new OclBlock("MyBlock"),
                    new OclBlock("MyBlock")
                );

        [Test]
        public void SingleLabel()
            => OclParser.Execute(@"MyBlock ""Foo"" {}")
                .Should()
                .HaveChildrenExactly(new OclBlock("MyBlock", new[] { "Foo" }));

        [Test]
        public void MultipleLabels()
            => OclParser.Execute(@"MyBlock ""Foo"" ""Bar"" ""Baz"" {}")
                .Should()
                .HaveChildrenExactly(new OclBlock("MyBlock", new[] { "Foo" }));

        [Test]
        public void WithSingleAttributeChild()
            => OclParser.Execute(@"
                    MyBlock {
                        Child = 1
                    }
                ")
                .Should()
                .HaveChildrenExactly(new OclBlock("MyBlock")
                {
                    new OclAttribute("Child", 1)
                });

        [Test]
        public void WithMultipleAttributeChild()
            => OclParser.Execute(@"
                    MyBlock {
                        Child = 1
                        Child2 = 2

                    }
                ")
                .Should()
                .HaveChildrenExactly(new OclBlock("MyBlock")
                {
                    new OclAttribute("Child", 1),
                    new OclAttribute("Child2", 2)
                });

        [Test]
        public void NestedBlocks()
            => OclParser.Execute(@"
                    MyBlock {
                        ChildBlock {
                            GrandChildBlock {}
                        }
                    }
                ")
                .Should()
                .HaveChildrenExactly(new OclBlock("MyBlock")
                {
                    new OclBlock("ChildBlock")
                    {
                        new OclBlock("GrandChildBlock")
                    }
                });

        [Test]
        public void MixedChildren()
            => OclParser.Execute(@"
                    MyBlock {
                        ChildBlock1 {


                        }
                        Child1 = 1
                        Child2 = 2


    ChildBlock2 ""Label"" {
        GrandChildBlock {}


        GrandChild = ""A""

    }
                    }
                ")
                .Should()
                .HaveChildrenExactly(new OclBlock("MyBlock")
                {
                    new OclBlock("ChildBlock1"),
                    new OclAttribute("Child1", 1),
                    new OclAttribute("Child2", 2),

                    new OclBlock("ChildBlock2", new[] { "Label" })
                    {
                        new OclBlock("GrandChildBlock"),
                        new OclAttribute("GrandChild", "A")
                    }
                });
    }
}