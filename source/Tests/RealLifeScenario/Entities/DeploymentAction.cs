using System;
using Newtonsoft.Json;
using Octopus.Data.Model;
using Octopus.Server.Extensibility.HostServices.Model;

namespace Tests.RealLifeScenario.Entities
{
    public class DeploymentAction
    {
        public DeploymentAction(string name, string actionType)
        {
            Id = Guid.NewGuid().ToString();
            Container = new DeploymentActionContainer();
            Name = name;
            ActionType = actionType;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string ActionType { get; set; }

        [JsonProperty("WorkerPoolId")]
        public WorkerPoolIdOrName? WorkerPoolIdOrName { get; set; }

        public string? WorkerPoolVariable { get; set; }

        public DeploymentActionContainer Container { get; set; }

        [JsonIgnore]
        public bool HasWorkerPoolSet => !string.IsNullOrWhiteSpace(WorkerPoolIdOrName?.Value) || !string.IsNullOrWhiteSpace(WorkerPoolVariable);

        public bool IsDisabled { get; set; }

        [JsonIgnore]
        public bool Enabled => !IsDisabled;

        public bool IsRequired { get; set; }

        [JsonIgnore]
        public bool CanBeUsedForProjectVersioning => true;

        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public ReferenceCollection<DeploymentEnvironmentIdOrName> Environments { get; } = new();

        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public ReferenceCollection<DeploymentEnvironmentIdOrName> ExcludedEnvironments { get; } = new();

        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public ReferenceCollection<ChannelIdOrName> Channels { get; } = new();

        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public ReferenceCollection<TagCanonicalIdOrName> TenantTags { get; } = new();

        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public PackageReferenceCollection Packages { get; } = new();

        public DeploymentActionCondition Condition { get; set; }

        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public PropertiesDictionary Properties { get; } = new();
    }
}