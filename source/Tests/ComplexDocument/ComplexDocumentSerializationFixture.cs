using System;
using System.Collections.Generic;
using System.Text;
using Assent;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Ocl;
using Octopus.Ocl.Parsing;

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
                                Properties = new Dictionary<string, string?>
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
                                Properties = new Dictionary<string, string?>
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
                                Properties = new Dictionary<string, string?>
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

        static OclSerializerOptions GetOptions()
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

            OclDocument ret;
            if (obj == null)
                ret = new OclDocument();
            else
            {
                var context = new OclConversionContext(options ?? new OclSerializerOptions());
                var converter = context.GetConverterFor(obj.GetType());
                ret = converter.ToDocument(context, obj);
            }

            var result = ret;

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

            object? obj = GetTestData();
            OclDocument ret;
            if (obj == null)
                ret = new OclDocument();
            else
            {
                var context = new OclConversionContext(options ?? new OclSerializerOptions());
                var converter = context.GetConverterFor(obj.GetType());
                ret = converter.ToDocument(context, obj);
            }

            var document = obj as OclDocument ?? ret;
            OclSerializerOptions? options1 = null;
            options1 ??= new OclSerializerOptions();
            var sb = new StringBuilder();
            // ReSharper disable once ConvertToUsingDeclaration
            using (var writer = new OclWriter(sb, options1))
            {
                writer.Write(document);
            }

            var result = sb.ToString();
            this.Assent(result);
        }

        [Test]
        public void Roundtrip()
        {
            var oclSerializerOptions = GetOptions();
            var input = GetTestData();

            OclDocument ret;
            if (input == null)
                ret = new OclDocument();
            else
            {
                var context = new OclConversionContext(oclSerializerOptions ?? new OclSerializerOptions());
                var converter = context.GetConverterFor(input.GetType());
                ret = converter.ToDocument(context, input);
            }

            var document = (object?)input as OclDocument ?? ret;
            OclSerializerOptions? options1 = null;
            options1 ??= new OclSerializerOptions();
            var sb = new StringBuilder();
            // ReSharper disable once ConvertToUsingDeclaration
            using (var writer = new OclWriter(sb, options1))
            {
                writer.Write(document);
            }

            var ocl = sb.ToString();
            var document1 = OclParser.Execute(ocl);
            var context1 = new OclConversionContext(oclSerializerOptions ?? new OclSerializerOptions());
            var result1 = context1.FromElement(typeof(DeploymentProcess), document1, null);
            if (result1 == null)
                throw new OclException("Document conversion resulted in null, which is not valid");
            var result = typeof(DeploymentProcess) == typeof(OclDocument)
                ? (DeploymentProcess)(object)document1
                : (DeploymentProcess)result1;

            result.Should().BeEquivalentTo(input);
        }
    }
}