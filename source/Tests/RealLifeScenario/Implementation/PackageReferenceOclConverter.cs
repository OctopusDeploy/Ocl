using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Octopus.Ocl;
using Octopus.Ocl.Converters;
using Octopus.Server.Extensibility.HostServices.Model;

namespace Tests.RealLifeScenario.Implementation
{
    public class PackageReferenceOclConverter : DefaultBlockOclConverter
    {
        readonly IReadOnlyList<PropertyInfo> labelProperties = new[]
        {
            typeof(PackageReference).GetProperty(nameof(PackageReference.Name))!
        };

        public override bool CanConvert(Type type)
            => type == typeof(PackageReference);

        protected override void SetLabels(Type type, OclBlock block, object target)
        {
            if (block.Labels.Count == 0)
                return;
            if (block.Labels.Count > 1)
                throw new OclException("Package reference blocks can only have zero or one label");

            ((PackageReference) target).Name = block.Labels[0];
        }

        protected override IEnumerable<string> GetLabels(object obj)
        {
            var name = ((PackageReference) obj).Name;
            return string.IsNullOrWhiteSpace(name) ? Array.Empty<string>() : new[] { name };
        }

        protected override IEnumerable<PropertyInfo> GetProperties(Type type)
            => base.GetProperties(type).Where(ShouldSerialize);

        internal static bool ShouldSerialize(PropertyInfo property)
        {
            switch (property.Name)
            {
                case nameof(PackageReference.Id):
                case nameof(PackageReference.Name):
                case nameof(PackageReference.IsPrimaryPackage):
                    return false;
            }

            return true;
        }
    }
}