using System.Diagnostics.CodeAnalysis;
using Octopus.TinyTypes;

namespace Tests.RealLifeScenario.Entities
{
    /// <summary>
    /// Copied subset of class from server.
    /// </summary>
    public class FeedIdOrName : CaseInsensitiveStringTinyType
    {
        internal FeedIdOrName(string value) : base(value)
        {
        }
    }
    
    public static class FeedIdOrNameExtensionMethods
    {
        [return: NotNullIfNotNull("value")]
        public static FeedIdOrName? ToFeedIdOrName(this string? value) => value is null ? null : new FeedIdOrName(value);
    }
}