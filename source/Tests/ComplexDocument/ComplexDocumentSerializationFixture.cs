using System;
using System.Collections.Generic;
using Assent;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;

namespace Tests.ComplexDocument
{
    public class ComplexDocumentSerializationFixture
    {
        DeploymentProcess GetTestData()
            => new DeploymentProcess
            {
                Steps = new List<DeploymentStep>
                {
                    new DeploymentStep("Simple Script")
                    {
                        Roles = new List<string>
                            { "web-server" },
                        Actions = new List<DeploymentAction>
                        {
                            new DeploymentAction
                            {
                                Name = "Simple Script",
                                Type = "Inline Script Action",
                                Properties = new Dictionary<string, string?>()
                                {
                                    { "Syntax", "PowerShell" },
                                    { "Body", "Write-Host 'Hi'" }
                                }
                            }
                        }
                    },
                    new DeploymentStep("Rolling Step")
                    {
                        Roles = new List<string>
                            { "role1", "role2" },
                        Actions = new List<DeploymentAction>
                        {
                            new DeploymentAction
                            {
                                Name = "Deploy Website",
                                Type = "Deploy to IIS",
                                Properties = new Dictionary<string, string?>()
                                {
                                    { "AppPool.Framework", "v4.0" },
                                    { "AppPool.Identity", "ApplicationPoolIdentity" },
                                    { "CustomField", "Another value" }
                                }
                            },
                            new DeploymentAction
                            {
                                Name = "Deploy Website",
                                Type = "Deploy to IIS",
                                Properties = new Dictionary<string, string?>()
                                {
                                    { "AppPool.Framework", "v4.0" },
                                    { "AppPool.Identity", "ApplicationPoolIdentity" },
                                    { "CustomField", "Another value" }
                                }
                            }
                        }
                    }
                }
            };

        private static OclSerializerOptions GetOptions()
            => new OclSerializerOptions()
            {
                Converters = new List<IOclConverter>()
                {
                    new DeploymentProcessConverter(),
                    new DeploymentStepOclConverter(),
                    new DeploymentActionOclConverter()
                }
            };

        [Test]
        public void ToOclDocument()
        {
            var options = new OclSerializerOptions
            {
                Converters = new List<IOclConverter>
                {
                    new DeploymentActionOclConverter()
                }
            };

            var obj = new DeploymentStep("MyStep")
            {
                Actions = new List<DeploymentAction>
                {
                    new DeploymentAction { Type = "Decisive", Name = "Run" },
                    new DeploymentAction { Type = "Take No", Name = "Sit" }
                }
            };

            var result = OclConvert.ToOclDocument(obj, options);

            result.Should()
                .Be(
                    new OclDocument
                    {
                        new OclAttribute("name", "MyStep"),
                        new OclBlock("decisive", new[] { "run" }),
                        new OclBlock("take_no", new[] { "sit" })
                    }
                );
        }

        [Test]
        public void Serialization()
        {
            var options = new OclSerializerOptions
            {
                Converters = new List<IOclConverter>
                {
                    new DeploymentStepOclConverter(),
                    new DeploymentActionOclConverter()
                }
            };

            var result = OclConvert.Serialize(GetTestData(), options);
            this.Assent(result);
        }


        [Test]
        public void Roundtrip()
        {
            var oclSerializerOptions = GetOptions();
            var input = GetTestData();

            var ocl = OclConvert.Serialize(input, oclSerializerOptions);
            var result = OclConvert.Deserialize<DeploymentProcess>(ocl, oclSerializerOptions);

            result.Should().BeEquivalentTo(input);
        }
    }
}