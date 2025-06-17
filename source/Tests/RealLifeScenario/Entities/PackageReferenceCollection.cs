using System;
using System.Collections;
using System.Collections.Generic;

namespace Tests.RealLifeScenario.Entities
{
    /// <summary>
    /// This is a copy from Octopus.Server.MessageContracts.PackageReferenceCollectionResource, with a subset of properties.
    /// </summary>
    public class PackageReferenceCollection : ICollection<PackageReference>
    {
        readonly Dictionary<string, PackageReference> idMap = new(StringComparer.OrdinalIgnoreCase);

        readonly Dictionary<string, PackageReference> nameMap = new(StringComparer.OrdinalIgnoreCase);

        public PackageReferenceCollection()
        {
        }

        public PackageReferenceCollection(IEnumerable<PackageReference> packages)
        {
            foreach (var package in packages) Add(package);
        }

        public PackageReference? PrimaryPackage => nameMap.ContainsKey("") ? nameMap[""] : null;

        public bool HasPrimaryPackage => PrimaryPackage != null;

        public int Count => nameMap.Count;

        public bool IsReadOnly => false;

        public void Add(PackageReference item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (nameMap.ContainsKey(item.Name))
            {
                throw new ArgumentException($"A package reference with the name '{item.Name}' already exists");
            }

            if (idMap.ContainsKey(item.Id))
            {
                throw new ArgumentException($"A package reference with the ID '{item.Id}' already exists");
            }

            nameMap.Add(item.Name, item);
            idMap.Add(item.Id, item);
        }

        public bool Contains(PackageReference item) => idMap.ContainsKey(item.Id);

        public void CopyTo(PackageReference[] array, int arrayIndex)
        {
            nameMap.Values.CopyTo(array, arrayIndex);
        }

        public bool Remove(PackageReference item)
        {
            nameMap.Remove(item.Name);
            return idMap.Remove(item.Id);
        }

        public void Clear()
        {
            idMap.Clear();
            nameMap.Clear();
        }

        public IEnumerator<PackageReference> GetEnumerator() => nameMap.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}