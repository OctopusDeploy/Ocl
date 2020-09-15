using System;
using System.Collections.Generic;

namespace Tests.ComplexDocument
{
    class DeploymentProcess
    {
        public List<DeploymentStep> Steps { get; set; } = new List<DeploymentStep>();
    }

    class DeploymentStep
    {
        public DeploymentStep(string name)
            => Name = name;

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