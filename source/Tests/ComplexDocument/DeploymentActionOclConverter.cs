using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.ComplexDocument
{
    class DeploymentActionOclConverter : DefaultBlockOclConverter
    {
        public override bool CanConvert(Type type)
            => typeof(DeploymentAction).IsAssignableFrom(type);

        protected override string GetName(OclConversionContext context, string name, object obj)
            => ((DeploymentAction)obj).Type.Replace(" ", "_");

        protected override IEnumerable<string> GetLabels(object obj)
            => new[] { ((DeploymentAction)obj).Name };

        protected override IEnumerable<IOclElement> GetElements(object obj, OclConversionContext context)
        {
            var action = (DeploymentAction)obj;
            var properties = GetSettableProperties();

            var elements = GetElements(obj, properties, context).ToList();

            var actionProperties = action.Properties
                .SelectMany(kvp => context.ToElements(kvp.Key, kvp.Value));

            var firstBlockIndex = elements.FindIndex(e => e is OclBlock);
            if (firstBlockIndex == -1)
                elements.AddRange(actionProperties);
            else
                elements.InsertRange(firstBlockIndex, elements);

            return elements;
        }

        public DeploymentAction ActionFromElement(OclConversionContext context, IOclElement element)
        {
            var block = (OclBlock)element;

            var action = new DeploymentAction
            {
                Name = block.Labels[0],
                Type = block.Name.Replace("_", " ")
            };

            var notFound = SetProperties(context, block, action, GetSettableProperties());
            action.Properties = notFound.Cast<OclAttribute>()
                .ToDictionary(
                    a => a.Name.Replace("_", "."),
                    a => (string?)a.Value
                );
            return action;
        }

        protected override void SetLabels(Type type, OclBlock block, object target)
            => ((DeploymentAction)target).Name = block.Labels[0];

        IEnumerable<PropertyInfo> GetSettableProperties()
        {
            var properties = from p in GetProperties(typeof(DeploymentAction))
                where p.Name != nameof(DeploymentAction.Type)
                where p.Name != nameof(DeploymentAction.Name)
                where p.Name != nameof(DeploymentAction.Properties)
                select p;
            return properties;
        }
    }
}