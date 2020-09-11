using System;
using System.Collections.Generic;
using System.Linq;
using Assent;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

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
        public void ToOclDocument()
        {
            var options = new OclSerializerOptions()
            {
                Converters = new List<IOclConverter>()
                {
                    new ActionOclConverter()
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

            var result = OclConvert.ToOclDocument(obj, options);

            result.Should()
                .Be(
                    new OclDocument()
                    {
                        new OclAttribute("Name", "MyStep"),
                        new OclAttribute("Roles", null),
                        new OclBlock("decisive", new[] { "run" }),
                        new OclBlock("take_no", new[] { "sit" })
                    }
                );
        }

        [Test]
        public void Serialization()
        {
            var options = new OclSerializerOptions()
            {
                Converters = new List<IOclConverter>()
                {
                    new StepOclConverter(),
                    new ActionOclConverter()
                }
            };

            var result = OclConvert.Serialize(GetTestData(), options);
            this.Assent(result);
        }

        class StepOclConverter : OclConverter
        {
            public override bool CanConvert(Type type)
                => typeof(DeploymentStep).IsAssignableFrom(type);

            protected override IOclElement ConvertInternal(OclConversionContext context, string name, object obj)
            {
                var actionOclConverter = new ActionOclConverter();

                var step = (DeploymentStep)obj;

                var actions = step.Actions.SelectMany(a => actionOclConverter.ToElements(context, name, a)).ToArray();

                var element = actions.Length == 1
                    ? (OclBlock) actionOclConverter.ToElements(context, name, step.Actions[0]).Single()
                    : new OclBlock("rolling", new[] { step.Name }, actions);

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

        class ActionOclConverter : DefaultBlockOclConverter
        {
            public override bool CanConvert(Type type)
                => typeof(DeploymentAction).IsAssignableFrom(type);

            protected override string GetName(string name, object obj)
                => ((DeploymentAction)obj).Type.Replace(" ", "_").ToLower();

            protected override IEnumerable<string> GetLabels(object obj)
                => new[] { ((DeploymentAction)obj).Name };

            protected override IEnumerable<IOclElement> GetElements(object obj, OclConversionContext context)
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

                var firstBlockIndex = elements.FindIndex(e => e is OclBlock);
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