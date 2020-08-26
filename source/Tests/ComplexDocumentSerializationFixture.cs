using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Hcl;
using Octopus.Hcl.Converters;

namespace Tests
{
    public class ComplexDocumentSerializationFixture
    {
        [Test]
        public void ToHDocument()
        {
            var options = new HclSerializerOptions()
            {
                Converters = new List<IHclConverter>()
                {
                    new ActionConverter()
                }
            };

            var obj = new ThoughtProcess()
            {
                Actions = new List<Action>()
                {
                    new Action { Type = "Decisive", Name = "Run" },
                    new Action { Type = "Take No", Name = "Sit" }
                }
            };
            HclConvert.ToHDocument(obj, options)
                .Should()
                .BeEquivalentTo(
                    new HDocument()
                    {
                        new HBlock("decisive", new[] { "run" }),
                        new HBlock("take_no", new[] { "sit" })
                    }
                );
        }

        class ActionConverter : BlockHclConverter
        {
            public override bool CanConvert(Type type)
                => typeof(Action).IsAssignableFrom(type);

            protected override string GetName(string name, object obj)
                => ((Action)obj).Type.Replace(" ", "_").ToLower();

            protected override IEnumerable<string> GetLabels(object obj)
                => new[] { ((Action)obj).Name.ToLower() };

            protected override IEnumerable<IHElement> GetElements(object obj, HclConversionContext context)
                => GetElements(
                    obj,
                    from p in obj.GetType().GetProperties()
                    where p.CanRead
                    where p.Name != nameof(Action.Type)
                    where p.Name != nameof(Action.Name)
                    select p,
                    context
                );
        }

        class ThoughtProcess
        {
            public List<Action> Actions { get; set; } = new List<Action>();
        }

        class Action
        {
            public string Type { get; set; } = "";
            public string Name { get; set; } = "";
        }
    }
}