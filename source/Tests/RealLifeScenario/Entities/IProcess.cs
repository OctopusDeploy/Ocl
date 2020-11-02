using System;
using System.Collections.Generic;

namespace Tests.RealLifeScenario.Entities
{
    public interface IProcess
    {
        List<DeploymentStep> Steps { get; }
        IEnumerable<DeploymentAction> AllEnabledActions { get; }
        IEnumerable<DeploymentAction> AllActions { get; }
    }
}