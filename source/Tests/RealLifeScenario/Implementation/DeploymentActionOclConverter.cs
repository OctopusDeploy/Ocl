using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Octopus.Ocl;
using Octopus.Ocl.Converters;
using Tests.RealLifeScenario.Entities;

namespace Tests.RealLifeScenario.ConverterStrategy.Implementation
{
        public class DeploymentActionOclConverter : DefaultBlockOclConverter
    {
        public override bool CanConvert(Type type)
            => type == typeof(DeploymentAction);

        protected override IOclElement ConvertInternal(OclConversionContext context, string name, object obj)
        {
            var action = (DeploymentAction) obj;
            if (string.IsNullOrWhiteSpace(action.Name))
                throw new Exception("The name of the action must be set");

            return base.ConvertInternal(context, name, obj);
        }

        protected override object CreateInstance(Type type, IOclElement oclElement)
        {
            var block = (OclBlock)oclElement;

            if (block.Labels.Count != 1)
                throw new OclException("The block for a deployment action must contain one label, the name of the step");

            var actionTypeElement = (OclAttribute?) block.FirstOrDefault(b => b.Name == "action_type");
            if (actionTypeElement == null)
                throw new OclException("The block for a deployment action must contain the type field");

            var actionType = actionTypeElement.Value as string;
            if(actionType == null)
                throw new OclException("The action type must be a string and not null");

            return new DeploymentAction(block.Labels[0], actionType);
        }

        protected override void SetLabels(Type type, OclBlock block, object target)
        {
        }

        protected override IEnumerable<PropertyInfo> GetLabelProperties(Type type)
            => new[]
            {
                typeof(DeploymentAction).GetProperty(nameof(DeploymentAction.Name))!
            };

        protected override IEnumerable<PropertyInfo> GetProperties(Type type)
            => base.GetProperties(type).Where(ShouldSerialize);


        internal static bool ShouldSerialize(PropertyInfo property)
        {
            switch (property.Name)
            {
                case nameof(DeploymentAction.Id):
                    return false;
            }

            return property.GetCustomAttribute<JsonIgnoreAttribute>() == null;
        }

        protected override IEnumerable<IOclElement> GetElements(object obj, OclConversionContext context)
        {
            var elements = base.GetElements(obj, context).ToList();
            elements.RemoveAll(e => e.Name == "container" && ((OclBlock) e).None());
            return elements;
        }

        public override object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue)
        {
            var obj = base.FromElement(context, type, element, currentValue);

            if (obj is DeploymentAction action)
            {
                action.Id = action.Name.Replace(" ", "-");
            }

            return obj;
        }
    }

}