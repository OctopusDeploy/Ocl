using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Octopus.Ocl;
using Octopus.Ocl.Converters;
using Tests.RealLifeScenario.Entities;

namespace Tests.RealLifeScenario.ConverterStrategy.Implementation
{
    public class DeploymentStepOclConverter : DefaultBlockOclConverter
    {
        protected override IOclElement ConvertInternal(OclConversionContext context, string name, object obj)
        {
            var step = (DeploymentStep)obj;
            if (string.IsNullOrWhiteSpace(step.Name))
                throw new Exception("The name of the action must be set");

            return base.ConvertInternal(context, name, obj);
        }

        public override bool CanConvert(Type type)
            => type == typeof(DeploymentStep);

        protected override object CreateInstance(Type type, IOclElement oclElement)
        {
            var block = (OclBlock)oclElement;

            if (block.Labels.Count != 1)
                throw new OclException("The block for a deployment step must contain one label, the name of the step");

            return new DeploymentStep(block.Labels[0]);
        }

        protected override void SetLabels(Type type, OclBlock block, object target)
        {
        }

        protected override IEnumerable<PropertyInfo> GetLabelProperties(Type type)
            => new[]
            {
                typeof(DeploymentStep).GetProperty(nameof(DeploymentStep.Name))!
            };

        protected override IEnumerable<PropertyInfo> GetProperties(Type type)
            => base.GetProperties(type).Where(ShouldSerialize);

        internal static bool ShouldSerialize(PropertyInfo property)
        {
            switch (property.Name)
            {
                case nameof(DeploymentStep.Id):
                    return false;

                case nameof(DeploymentStep.Actions):
                case nameof(DeploymentStep.Properties):
                    return true;
            }

            return property.CanWrite;
        }
    }
}