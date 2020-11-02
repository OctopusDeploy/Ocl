using System;
using Octopus.TinyTypes;

namespace Tests.RealLifeScenario.Entities
{
    public class ChannelIdOrName : CaseInsensitiveStringTinyType
    {
        public ChannelIdOrName(string value)
            : base(value)
        {
        }
    }
}