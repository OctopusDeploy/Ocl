using System;
using System.Linq;
using Octopus.TinyTypes;

namespace Tests.RealLifeScenario.Entities
{
    /// <summary>
    /// Copied from server.
    /// </summary>
    public class TagSetId : PrefixedNumericStringTinyType
    {
        internal TagSetId(string value) : base(value, "TagSets-")
        {
            if (!TagSetIdOrName.LooksLikeASingleTokenIdOrName(value))
            {
                throw new ArgumentException("Value must look like a canonical TagSet ID");
            }
        }
    }

    /// <summary>
    /// Copied just the static method from server.
    /// </summary>
    public static class TagSetIdOrName
    {
        internal static bool LooksLikeASingleTokenIdOrName(string s)
        {
            var tokens = s.Split("/".ToCharArray());
            if (tokens.Length != 1) return false;
            if (tokens.Any(string.IsNullOrWhiteSpace)) return false;
            return true;
        }
    }
}