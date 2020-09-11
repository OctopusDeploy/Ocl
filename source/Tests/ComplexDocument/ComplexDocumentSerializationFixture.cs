using System;
using System.Collections.Generic;
using Assent;
using NUnit.Framework;
using Octopus.Ocl;

namespace Tests.ComplexDocument
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
                                Properties = new Dictionary<string, string?>()
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
                                Properties = new Dictionary<string, string?>()
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
                                Properties = new Dictionary<string, string?>()
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
                    new DeploymentActionOclConverter()
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
                    new DeploymentStepOclConverter(),
                    new DeploymentActionOclConverter()
                }
            };

            var result = OclConvert.Serialize(GetTestData(), options);
            this.Assent(result);
        }

    }
}