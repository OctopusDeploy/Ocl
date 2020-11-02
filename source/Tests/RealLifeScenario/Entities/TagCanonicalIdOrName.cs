using System;
using Octopus.TinyTypes;

namespace Tests.RealLifeScenario.Entities
{
    public class TagCanonicalIdOrName : CaseInsensitiveStringTinyType
    {
        public TagCanonicalIdOrName(string value)
            : base(value)
        {
        }
    }
}