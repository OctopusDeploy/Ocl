using System;
using Octopus.TinyTypes;

namespace Tests.RealLifeScenario.Entities
{
    public class DeploymentEnvironmentIdOrName : CaseInsensitiveStringTinyType
    {
        public DeploymentEnvironmentIdOrName(string value)
            : base(value)
        {
        }
    }
}