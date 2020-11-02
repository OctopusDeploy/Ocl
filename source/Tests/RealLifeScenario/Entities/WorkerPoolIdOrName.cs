using System;
using Octopus.TinyTypes;

namespace Tests.RealLifeScenario.Entities
{
    public class WorkerPoolIdOrName : CaseInsensitiveStringTinyType
    {
        public WorkerPoolIdOrName(string value)
            : base(value)
        {
        }
    }
}