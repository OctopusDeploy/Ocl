using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.ComplexDocument
{
    class DeploymentStepOclConverter : OclConverter
    {
        const string RollingBlockName = "rolling";

        public override bool CanConvert(Type type)
            => typeof(DeploymentStep).IsAssignableFrom(type);

        protected override IOclElement ConvertInternal(OclConversionContext context, string name, object obj)
        {
            var actionOclConverter = new DeploymentActionOclConverter();

            var step = (DeploymentStep)obj;

            var actions = step.Actions.SelectMany(a => actionOclConverter.ToElements(context, name, a)).ToArray();

            var element = actions.Length == 1
                ? (OclBlock)actionOclConverter.ToElements(context, name, step.Actions[0]).Single()
                : new OclBlock(RollingBlockName, new[] { step.Name }, actions);

            var properties = GetSettableProperties();

            var children = GetElements(obj, properties, context);

            element.InsertRange(0, children);

            return element;
        }

        public override object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue)
            => StepFromElement(context, element);

        public DeploymentStep StepFromElement(OclConversionContext context, IOclElement element)
        {
            var actionConverter = new DeploymentActionOclConverter();

            var block = (OclBlock)element;

            var step = new DeploymentStep(block.Labels[0]);
            var notFound = SetProperties(context, block, step, GetSettableProperties().ToArray()).ToHashSet();

            if (block.Name == RollingBlockName)
            {
                step.Actions = notFound.Select(e => actionConverter.ActionFromElement(context, e)).ToList();
            }
            else
            {
                var actionTempBlock = new OclBlock(block.Name, block.Labels, notFound);
                step.Actions.Add(actionConverter.ActionFromElement(context, actionTempBlock));
            }

            return step;
        }

        IEnumerable<PropertyInfo> GetSettableProperties()
            => from p in GetProperties(typeof(DeploymentStep))
                where p.Name != nameof(DeploymentStep.Name)
                where p.Name != nameof(DeploymentStep.Actions)
                select p;
    }
}