using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Tests.RealLifeScenario.Entities
{
    public class DeploymentStep
    {
        public DeploymentStep(string name)
        {
            Name = name;
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public DeploymentStepCondition Condition { get; set; }
        public DeploymentStepStartTrigger StartTrigger { get; set; }
        public DeploymentStepPackageRequirement PackageRequirement { get; set; }

        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public List<DeploymentAction> Actions { get; } = new();

        [JsonIgnore]
        public IEnumerable<PropertiesDictionary> InheritedPropertiesForActions
        {
            get { return Actions.Select(a => new PropertiesDictionary(a.Properties, Properties)); }
        }

        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public PropertiesDictionary Properties { get; } = new();
    }
}