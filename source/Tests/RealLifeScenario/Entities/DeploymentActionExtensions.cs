using System;
using Octopus.Server.Extensibility.HostServices.Model;

namespace Tests.RealLifeScenario.Entities
{
    public static class DeploymentActionExtensions
    {
        public static DeploymentAction WithProperty(this DeploymentAction action, string key, string value)
        {
            action.Properties[key] = new PropertyValue(value);
            return action;
        }

        public static DeploymentAction WithSensitiveProperty(this DeploymentAction action, string key, string value)
        {
            action.Properties[key] = new PropertyValue(value, true);
            return action;
        }
    }
}