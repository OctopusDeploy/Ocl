using System;
using System.Collections.Generic;
using System.Linq;
using Assent;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Hcl;
using Octopus.Hcl.Converters;

namespace Tests
{
    public class ComplexDocumentSerializationFixture
    {
        DeploymentProcess GetTestData()
            => new DeploymentProcess()
            {
                Steps = new List<DeploymentStep>()
                {
                    new DeploymentStep("Simple Script Step")
                    {
                        Roles = new List<string>() { "web-server", },
                        Actions = new List<DeploymentAction>()
                        {
                            new DeploymentAction()
                            {
                                Name = "Simple Script Action",
                                Type = "Inline Script Action",
                                Properties = new Dictionary<string, string>()
                                {
                                    { "Syntax", "PowerShell" },
                                    { "Body", "Write-Host 'Hi'" },
                                }
                            }
                        }
                    },
                    new DeploymentStep("Rolling Step")
                    {
                        Roles = new List<string>() { "role1", "role2" },
                        Actions = new List<DeploymentAction>()
                        {
                            new DeploymentAction()
                            {
                                Name = "Deploy Website",
                                Type = "Deploy to IIS",
                                Properties = new Dictionary<string, string>()
                                {
                                    { "AppPool.Framework", "v4.0" },
                                    { "AppPool.Identity", "ApplicationPoolIdentity" },
                                    { "CustomField", "Another value" }
                                }
                            },
                            new DeploymentAction()
                            {
                                Name = "Deploy Website",
                                Type = "Deploy to IIS",
                                Properties = new Dictionary<string, string>()
                                {
                                    { "AppPool.Framework", "v4.0" },
                                    { "AppPool.Identity", "ApplicationPoolIdentity" },
                                    { "CustomField", "Another value" }
                                }
                            }
                        }
                    },
                }
            };

        [Test]
        public void ToHDocument()
        {
            var options = new HclSerializerOptions()
            {
                Converters = new List<IHclConverter>()
                {
                    new ActionHclConverter()
                }
            };

            var obj = new DeploymentStep("MyStep")
            {
                Actions = new List<DeploymentAction>()
                {
                    new DeploymentAction { Type = "Decisive", Name = "Run" },
                    new DeploymentAction { Type = "Take No", Name = "Sit" }
                }
            };

            var result = HclConvert.ToHDocument(obj, options);

            result.Should()
                .BeEquivalentTo(
                    new HDocument()
                    {
                        new HAttribute("Roles", null),
                        new HBlock("decisive", new[] { "run" }),
                        new HBlock("take_no", new[] { "sit" })
                    },
                    config => config.IncludingAllRuntimeProperties()
                );
        }

        [Test]
        public void Serialization()
        {
            var options = new HclSerializerOptions()
            {
                Converters = new List<IHclConverter>()
                {
                    new StepHclConverter(),
                    new ActionHclConverter()
                }
            };

            var result = HclConvert.Serialize(GetTestData(), options);
            this.Assent(result);
        }

        class StepHclConverter : HclConverter
        {
            public override bool CanConvert(Type type)
                => typeof(DeploymentStep).IsAssignableFrom(type);

            protected override IHElement ConvertInternal(HclConversionContext context, string name, object obj)
            {
                var actionHclConverter = new ActionHclConverter();

                var step = (DeploymentStep)obj;

                var actions = step.Actions.SelectMany(a => actionHclConverter.Convert(context, name, a)).ToArray();

                var element = actions.Length == 1
                    ? (HBlock) actionHclConverter.Convert(context, name, step.Actions[0]).Single()
                    : new HBlock("rolling", new[] { step.Name }, actions);

                var properties = from p in typeof(DeploymentStep).GetProperties()
                    where p.CanRead
                    where p.Name != nameof(DeploymentStep.Name)
                    where p.Name != nameof(DeploymentStep.Actions)
                    select p;

                var children = GetElements(obj, properties, context);

                element.InsertRange(0, children);

                return element;
            }
        }

        class ActionHclConverter : DefaultBlockHclConverter
        {
            public override bool CanConvert(Type type)
                => typeof(DeploymentAction).IsAssignableFrom(type);

            protected override string GetName(string name, object obj)
                => ((DeploymentAction)obj).Type.Replace(" ", "_").ToLower();

            protected override IEnumerable<string> GetLabels(object obj)
                => new[] { ((DeploymentAction)obj).Name };

            protected override IEnumerable<IHElement> GetElements(object obj, HclConversionContext context)
            {
                var action = (DeploymentAction)obj;
                var properties = from p in typeof(DeploymentAction).GetProperties()
                    where p.CanRead
                    where p.Name != nameof(DeploymentAction.Type)
                    where p.Name != nameof(DeploymentAction.Name)
                    where p.Name != nameof(DeploymentAction.Properties)
                    select p;

                var elements = GetElements(obj, properties, context).ToList();

                var actionProperties = action.Properties
                    .SelectMany(kvp => context.ToElements(kvp.Key.ToLower(), kvp.Value));

                var firstBlockIndex = elements.FindIndex(e => e is HBlock);
                if (firstBlockIndex == -1)
                    elements.AddRange(actionProperties);
                else
                    elements.InsertRange(firstBlockIndex, elements);

                return elements;
            }
        }

        class DeploymentProcess
        {
            public List<DeploymentStep> Steps { get; set; } = new List<DeploymentStep>();
        }

        class DeploymentStep
        {
            public DeploymentStep(string name)
            {
                Name = name;
            }

            public string Name { get; }
            public List<string>? Roles { get; set; }
            public List<DeploymentAction> Actions { get; set; } = new List<DeploymentAction>();
        }

        class DeploymentAction
        {
            public string Type { get; set; } = "";
            public string Name { get; set; } = "";
            public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        }
    }
}