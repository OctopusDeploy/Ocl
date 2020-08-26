using System;
using System.Collections.Generic;

namespace Octopus.Hcl.Converters
{
    public class HclConverter : IHclConverter
    {
        private readonly IHclNamingStrategy namingStrategy;
        private readonly IHclLabelStrategy hclLabelStrategy;
        private readonly IHclChildElementStrategy hclChildElementStrategy;

        public virtual bool CanConvert(Type type)
            => true;

        public HclConverter(
            IHclNamingStrategy namingStrategy,
            IHclLabelStrategy hclLabelStrategy,
            IHclChildElementStrategy hclChildElementStrategy
        )
        {
            this.namingStrategy = namingStrategy;
            this.hclLabelStrategy = hclLabelStrategy;
            this.hclChildElementStrategy = hclChildElementStrategy;
        }

        public IEnumerable<IHElement> Convert(HclConversionContext context, string name, object obj)
            => new[]
            {
                HAttribute.IsSupportedValueType(obj.GetType())
                    ? new HAttribute(namingStrategy.GetName(name, obj), obj)
                    : new HBlock(
                        namingStrategy.GetName(name, obj),
                        hclLabelStrategy.GetLabels(obj),
                        hclChildElementStrategy.GetElements(obj, context)
                    )
            };
    }
}