using System;
using System.Collections.Generic;
using System.Linq;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.ComplexDocument
{
    class DeploymentActionOclConverter : DefaultBlockOclConverter
    {
        public override bool CanConvert(Type type)
            => typeof(DeploymentAction).IsAssignableFrom(type);

        protected override string GetName(OclConversionContext context, string name, object obj)
            => ((DeploymentAction)obj).Type.Replace(" ", "_").ToLower();

        protected override IEnumerable<string> GetLabels(object obj)
            => new[] { ((DeploymentAction)obj).Name };

        protected override IEnumerable<IOclElement> GetElements(object obj, OclConversionContext context)
        {
            var action = (DeploymentAction)obj;
            var properties = from p in typeof(DeploymentAction).GetProperties()
                where p.CanRead
                where p.Name != nameof(DeploymentAction.Type)
                where p.Name != nameof(DeploymentAction.Name)
                where p.Name != nameof(DeploymentAction.Properties)
                select p;

            var elements = GetElements(obj, properties, context).ToList();

            var actionProperties = action.Properties
                .SelectMany(kvp => context.ToElements(kvp.Key.ToLower(), kvp.Value));

            var firstBlockIndex = elements.FindIndex(e => e is OclBlock);
            if (firstBlockIndex == -1)
                elements.AddRange(actionProperties);
            else
                elements.InsertRange(firstBlockIndex, elements);

            return elements;
        }
    }
}