using System.Collections.Generic;

namespace Tests.RealLifeScenario.Entities
{
    public class ActionNames
    {
        public const string Iis = "Octopus.IIS";
        public const string Script = "Octopus.Script";

        public static IReadOnlyList<string> All = new[]
        {
            Iis,
            Script
        };
    }
}