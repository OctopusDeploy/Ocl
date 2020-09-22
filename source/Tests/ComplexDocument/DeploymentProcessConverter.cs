using System;
using System.Linq;
using Octopus.Ocl;
using Octopus.Ocl.Converters;

namespace Tests.ComplexDocument
{
    public class DeploymentProcessConverter : DefaultBlockOclConverter
    {
        public override bool CanConvert(Type type)
            => type == typeof(DeploymentProcess);

        public override object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue)
        {
            var process = new DeploymentProcess();

            var stepConverter = new DeploymentStepOclConverter();
            process.Steps = ((OclBody)element)
                .Select(c => stepConverter.StepFromElement(context, c))
                .ToList();
            return process;
        }
    }
}