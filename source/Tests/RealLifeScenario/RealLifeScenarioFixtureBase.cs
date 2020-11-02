using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Assent;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Data.Model;
using Octopus.Ocl;
using Octopus.Server.Extensibility.HostServices.Model;
using Octopus.Server.Extensibility.HostServices.Model.Feeds;
using Octopus.Server.Extensibility.HostServices.Model.Projects;
using Octopus.Server.Extensibility.HostServices.Model.Tenants;
using Octopus.Server.Extensibility.Resources;
using Tests.RealLifeScenario.ConverterStrategy.Implementation;
using Tests.RealLifeScenario.Entities;
using Tests.RealLifeScenario.Implementation;

namespace Tests.RealLifeScenario
{
    public class RealLifeScenarioFixture
    {
        const string SpaceId = "Spaces-1";
        const string ProjectId = "Projects-1";

        protected string Serialize(VcsRunbookPersistenceModel model)
            => new OclSerializerFactory().Create().Serialize(model);

        protected VcsRunbookPersistenceModel Deserialize(string ocl)
            => new OclSerializerFactory().Create().Deserialize<VcsRunbookPersistenceModel>(ocl);

        [Test]
        public void MostFieldsWithNonDefaults()
            => ExecuteTest(
                new VcsRunbook("My Runbook")
                {
                    Description = "This is a description",
                    EnvironmentScope = RunbookEnvironmentScope.Specified,
                    Environments = { "Production", "Development" },
                    ConnectivityPolicy = new ProjectConnectivityPolicy()
                    {
                        TargetRoles = new ReferenceCollection("Target Role"),
                        ExcludeUnhealthyTargets = true,
                        SkipMachineBehavior = SkipMachineBehavior.SkipUnavailableMachines,
                        AllowDeploymentsToNoTargets = true
                    },
                    MultiTenancyMode = TenantedDeploymentMode.TenantedOrUntenanted,
                    DefaultGuidedFailureMode = GuidedFailureMode.On
                },
                new RunbookProcess(ProjectId, SpaceId)
                {
                    Id = "Should not appear",
                    Version = 4,
                    OwnerId = "Should not appear",
                    IsFrozen = true,
                    RunbookId = "Should not appear",
                    Steps =
                    {
                        new DeploymentStep("Script Step")
                        {
                            Id = "Should Not Appear",

                            Condition = DeploymentStepCondition.Always,
                            StartTrigger = DeploymentStepStartTrigger.StartWithPrevious,
                            PackageRequirement = DeploymentStepPackageRequirement.BeforePackageAcquisition,
                            Properties =
                            {
                                { "StepProperty", new PropertyValue("Value1", false) },
                                { "Octopus.Action.TargetRoles", new PropertyValue("Portal") }
                            },
                            Actions =
                            {
                                new DeploymentAction("Script Step", ActionNames.Script)
                                {
                                    Id = "Should Not Appear",
                                    Channels = { new ChannelIdOrName("Release"), new ChannelIdOrName("Beta") },
                                    Condition = DeploymentActionCondition.Variable,
                                    Container = new DeploymentActionContainer()
                                    {
                                        Image = "ContainerImage",
                                        FeedId = "Container Feed"
                                    },
                                    Environments = { new DeploymentEnvironmentIdOrName("Pre Production") },
                                    ExcludedEnvironments = { new DeploymentEnvironmentIdOrName("Production") },
                                    TenantTags = { new TagCanonicalIdOrName("David/Who") },
                                    WorkerPoolIdOrName = new WorkerPoolIdOrName("My Worker Pool"),
                                    WorkerPoolVariable = "WorkerPoolVar",
                                    IsDisabled = true,
                                    IsRequired = true,
                                    Packages =
                                    {
                                        new PackageReference("", "OctoFx.Web", new FeedIdOrName("External Feed"), PackageAcquisitionLocation.ExecutionTarget),
                                        new PackageReference("Helper Package", "OctoFx.Helper", new FeedIdOrName("External Feed"), PackageAcquisitionLocation.Server)
                                    },
                                    Properties =
                                    {
                                        { "Action.Prop", new PropertyValue("The Value") },
                                        // { "Action.Sensitive", new PropertyValue("Sensitive Value", true) }
                                    }
                                },
                            }
                        }
                    }
                }
            );

        [Test]
        public void Rolling()
            => ExecuteTest(
                new VcsRunbook("My Runbook"),
                new RunbookProcess(ProjectId, SpaceId)
                {
                    Steps =
                    {
                        new DeploymentStep("My Rolling Step")
                        {
                            Actions =
                            {
                                new DeploymentAction("First", ActionNames.Script),
                                new DeploymentAction("Second", ActionNames.Script),
                            }
                        }
                    }
                }
            );

        [Test]
        [Ignore("Line endings are not handled correctly yet")]
        public void ScriptAction()
            => ExecuteTest(
                new DeploymentAction("Backup the Database", ActionNames.Script)
                    .WithProperty("Octopus.Action.RunOnServer", "true")
                    .WithProperty("Octopus.Action.Script.Syntax", "PowerShell")
                    .WithProperty("Octopus.Action.Script.ScriptSource", "Inline")
                    .WithProperty(
                        "Octopus.Action.Script.ScriptBody",
                        "$backupResult = $OctopusParameters[\"Octopus.Action[Backup Octofront Prod SQL Database].Output.backupResult\"]\r\n\r\n\r\n$TableSAName = 'infrascoreprodaccount'\r\n$TableName = 'results'\r\n$partitionKey = \"OctofrontSQL\"\r\n\r\n$data = @{\r\n  RowKey = ([guid]::NewGuid().tostring())\r\n  PartitionKey = $partitionKey\r\n  state = $backupResult \r\n  description = \"Octofront SQL Backup weekly\"\r\n  dtDate = [datetime]::UtcNow\r\n}\r\nNew-AzureTableEntity -StorageAccountName $TableSAName `\r\n                     -StorageAccountAccessKey $TableStorageKey `\r\n                     -TableName $TableName `\r\n                     -Entities $data        \r\n\r\n$querystring = \"(PartitionKey eq '$partitionKey')\"\r\n$tableWrite = Get-AzureTableEntity -TableName $TableName `\r\n                                 -StorageAccountName $TableSAName `\r\n                                 -StorageAccountAccessKey $TableStorageKey  `\r\n                                 -QueryString $querystring `\r\n                                 -ConvertDateTimeFields $true `\r\n                                 -GetAll $false \r\n\r\n$ordered = $tableWrite | Sort-Object -Descending -Property Timestamp \r\n$entryResult = $ordered[0] | Sort-Object -Descending -Property Timestamp | Select-Object description, state\r\n\r\nwrite-output $entryResult | ft\r\n"
                    ));

        [Test]
        public void IisAction()
            => ExecuteTest(
                new DeploymentAction("Deploy Website", ActionNames.Iis)
                    .WithProperty("Octopus.Action.IISWebSite.DeploymentType", "webSite")
                    .WithProperty("Octopus.Action.IISWebSite.CreateOrUpdateWebSite", "True")
                    .WithProperty("Octopus.Action.IISWebSite.Bindings",
                        "[{\"protocol\",\"https\",\"port\",\"443\",\"host\",\"#{Domain}\",\"thumbprint\",null,\"certificateVariable\",\"Certificate\",\"requireSni\",\"True\",\"enabled\",\"True\"){\"protocol\",\"https\",\"ipAddress\",\"\",\"port\",\"443\",\"host\",\"#{DirectDomainPrefix}.#{Domain}\",\"thumbprint\",null,\"certificateVariable\",\"Certificate\",\"requireSni\",\"True\",\"enabled\",true){\"protocol\",\"http\",\"ipAddress\",\"*\",\"port\",\"80\",\"host\",\"#{Domain}\",\"thumbprint\",null,\"certificateVariable\",null,\"requireSni\",false,\"enabled\",true){\"protocol\",\"http\",\"ipAddress\",\"*\",\"port\",\"80\",\"host\",\"www.#{Domain}\",\"thumbprint\",null,\"certificateVariable\",null,\"requireSni\",false,\"enabled\",true){\"protocol\",\"https\",\"ipAddress\",\"*\",\"port\",\"443\",\"host\",\"www.#{domain}\",\"thumbprint\",null,\"certificateVariable\",\"Certificate\",\"requireSni\",\"True\",\"enabled\",true}]")
                    .WithProperty("Octopus.Action.IISWebSite.ApplicationPoolIdentityType", "ApplicationPoolIdentity")
                    .WithProperty("Octopus.Action.IISWebSite.EnableAnonymousAuthentication", "True")
                    .WithProperty("Octopus.Action.IISWebSite.EnableBasicAuthentication", "False")
                    .WithProperty("Octopus.Action.IISWebSite.EnableWindowsAuthentication", "False")
                    .WithProperty("Octopus.Action.IISWebSite.WebApplication.ApplicationPoolFrameworkVersion", "v4.0")
                    .WithProperty("Octopus.Action.IISWebSite.WebApplication.ApplicationPoolIdentityType", "ApplicationPoolIdentity")
                    .WithProperty("Octopus.Action.EnabledFeatures", "Octopus.Features.IISWebSite,Octopus.Features.JsonConfigurationVariables")
                    .WithProperty("Octopus.Action.Package.FeedId", "My External Feed")
                    .WithProperty("Octopus.Action.Package.DownloadOnTentacle", "True")
                    .WithProperty("Octopus.Action.IISWebSite.WebRootType", "packageRoot")
                    .WithProperty("Octopus.Action.IISWebSite.StartApplicationPool", "True")
                    .WithProperty("Octopus.Action.IISWebSite.StartWebSite", "True")
                    .WithProperty("Octopus.Action.Package.PackageId", "OctoFX.Web")
                    .WithProperty("Octopus.Action.IISWebSite.WebSiteName", "#{Domain}")
                    .WithProperty("Octopus.Action.IISWebSite.ApplicationPoolName", "#{Domain}")
                    .WithProperty("Octopus.Action.Package.JsonConfigurationVariablesEnabled", "True")
                    .WithProperty("Octopus.Action.Package.JsonConfigurationVariablesTargets", "appsettings.json")
            );

        void ExecuteTest(
            DeploymentAction action,
            [CallerMemberName]
            string? testName = null
        )
            => ExecuteTest(
                new VcsRunbook("My Runbook"),
                new RunbookProcess(ProjectId, SpaceId)
                {
                    Steps =
                    {
                        new DeploymentStep(action.Name)
                        {
                            Actions = { action }
                        }
                    }
                },
                testName);

        void ExecuteTest(
            VcsRunbook runbook,
            RunbookProcess process,
            [CallerMemberName]
            string? testName = null
        )
        {
            var model = new VcsRunbookPersistenceModel(runbook) { Process = process };
            var ocl = Serialize(model);
            this.Assent(ocl, testName: testName);

            var result = Deserialize(ocl);
            result.Should()
                .BeEquivalentTo(
                    model,
                    config => config.IncludingAllDeclaredProperties()
                        // v5 of Fluent assertions should provide us the property info directly
                        .Excluding(i => ExcludePropertyFromAssertion(i.SelectedMemberInfo.DeclaringType.GetProperty(i.SelectedMemberInfo.Name)!)
                        )
                );
        }

        protected bool ExcludePropertyFromAssertion(PropertyInfo info)
        {
            if (info.DeclaringType == typeof(RunbookProcess))
                return !VcsRunbookPersistenceModelOclConverter.RunbookProcessOclConverter.ShouldSerialize(info);
            if (info.DeclaringType == typeof(DeploymentStep))
                return !DeploymentStepOclConverter.ShouldSerialize(info);
            if (info.DeclaringType == typeof(DeploymentAction))
                return !DeploymentActionOclConverter.ShouldSerialize(info);
            if (info.DeclaringType == typeof(PackageReference))
                return !PackageReferenceOclConverter.ShouldSerialize(info);
            return false;
        }
    }
}