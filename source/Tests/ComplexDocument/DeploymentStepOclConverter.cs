using System;
using System.Linq;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.ComplexDocument
{
    class DeploymentStepOclConverter : OclConverter
    {
        public override bool CanConvert(Type type)
            => typeof(DeploymentStep).IsAssignableFrom(type);

        protected override IOclElement ConvertInternal(OclConversionContext context, string name, object obj)
        {
            var actionOclConverter = new DeploymentActionOclConverter();

            var step = (DeploymentStep)obj;

            var actions = step.Actions.SelectMany(a => actionOclConverter.ToElements(context, name, a)).ToArray();

            var element = actions.Length == 1
                ? (OclBlock)actionOclConverter.ToElements(context, name, step.Actions[0]).Single()
                : new OclBlock("rolling", new[] { step.Name }, actions);

            var properties = from p in typeof(DeploymentStep).GetProperties()
                where p.CanRead
                where p.Name != nameof(DeploymentStep.Name)
                where p.Name != nameof(DeploymentStep.Actions)
                select p;

            var children = GetElements(obj, properties, context);

            element.InsertRange(0, children);

            return element;
        }
    }
}