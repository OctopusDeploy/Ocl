using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.RealLifeScenario.Entities
{
    /// <summary>
    /// A case-insensitive collection of unique strings used for holding document ID's or names.
    /// Copied subset from server. (the structure is the same for serialization, just skipped on helper methods)
    /// </summary>
    public class ReferenceCollection : HashSet<string>
    {
        public ReferenceCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public ReferenceCollection(string singleValue)
            : this()
        {
            ReplaceAll(new[] { singleValue });
        }

        public void ReplaceAll(IEnumerable<string> newItems)
        {
            Clear();

            if (newItems == null) return;

            foreach (var item in newItems)
            {
                Add(item);
            }
        }
    }

    public class ReferenceCollection<T> : HashSet<T>
    {
        public override string ToString()
        {
            return string.Join(", ", this.Where(x => x != null).Select(x => x!.ToString()));
        }
    }
}