using System;
using Octopus.Data.Model;
using Octopus.Server.Extensibility.HostServices.Model.Projects;
using Octopus.Server.Extensibility.HostServices.Model.Tenants;

namespace Tests.RealLifeScenario.Entities
{
    public class VcsRunbook
    {
        public VcsRunbook(string name)
            => Name = name;

        public string Name { get; }

        public string Description { get; set; } = "";

        public TenantedDeploymentMode MultiTenancyMode { get; set; }

        public ProjectConnectivityPolicy ConnectivityPolicy { get; set; } = new ProjectConnectivityPolicy()
        {
            AllowDeploymentsToNoTargets = true
        };

        public RunbookEnvironmentScope EnvironmentScope { get; set; }

        public ReferenceCollection Environments { get; } = new ReferenceCollection();

        public GuidedFailureMode DefaultGuidedFailureMode { get; set; }
    }
}