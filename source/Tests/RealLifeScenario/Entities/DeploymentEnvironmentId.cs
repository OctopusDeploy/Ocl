using System.Diagnostics.CodeAnalysis;
using Octopus.TinyTypes;

namespace Tests.RealLifeScenario.Entities
{
    /// <summary>
    /// Copied subset of class from server.
    /// </summary>
    public class DeploymentEnvironmentId : PrefixedNumericStringTinyType
    {
        internal DeploymentEnvironmentId(string value) : base(value, "Environments-")
        {
        }
    }
}