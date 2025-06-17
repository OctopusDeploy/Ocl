using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using Newtonsoft.Json;

namespace Tests.RealLifeScenario.Entities
{
    public class RunbookProcess : IProcess
    {
        public RunbookProcess(string projectId, string spaceId)
        {
            ProjectId = projectId;
            Id = Guid.NewGuid().ToString();
            OwnerId = projectId;
            SpaceId = spaceId;
            Steps = new List<DeploymentStep>();
        }

        public string ProjectId { get; set; }

        [JsonIgnore]
        public string RunbookId
        {
            get => OwnerId;
            set => OwnerId = value;
        }

        public string Id { get; internal set; }
        public string OwnerId { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsFrozen { get; set; }

        public int Version { get; set; }
        public string SpaceId { get; set; }
        public List<DeploymentStep> Steps { get; protected set; }

        [JsonIgnore]
        public IEnumerable<DeploymentAction> AllEnabledActions
        {
            get { return AllActions.Where(a => !a.IsDisabled); }
        }

        [JsonIgnore]
        public IEnumerable<DeploymentAction> AllActions
        {
            get { return Steps.SelectMany(s => s.Actions); }
        }

        [JsonIgnore]
        public IEnumerable<(string id, Type documentType)> RelatedDocuments
        {
            get
            {
                var actions = (IsFrozen ? AllEnabledActions : AllActions).ToArray();

                var environments = actions.SelectMany(a => a.Environments).Select(x => x.Value).Select(id => (id, typeof(DeploymentEnvironmentId)));
                var excludedEnvironments = actions.SelectMany(a => a.ExcludedEnvironments).Select(x => x.Value).Select(id => (id, typeof(DeploymentEnvironmentId)));
                var channels = actions.SelectMany(a => a.Channels).Select(x => x.Value).Select(id => (id, typeof(Channel)));
                var tenantTags = actions.SelectMany(a => a.TenantTags).Select(x => x.Value).Select(id => (id, typeof(TagSetId)));

                return environments
                    .Concat(excludedEnvironments)
                    .Concat(channels)
                    .Concat(tenantTags);
            }
        }
    }
}