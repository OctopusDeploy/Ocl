using System;
using System.Collections.Generic;
using Octopus.Ocl;
using Tests.RealLifeScenario.ConverterStrategy.Implementation;

namespace Tests.RealLifeScenario.Implementation
{
    public class OclSerializerFactory
    {
        public IOclSerializer Create()
            => new OclSerializer(
                new OclSerializerOptions
                {
                    Converters = new List<IOclConverter>
                    {
                        new ReferenceCollectionOclConverter(),
                        new TinyTypeOclConverter(),
                        new TinyTypeReferenceCollectionOclConverter(),
                        new DeploymentStepOclConverter(),
                        new DeploymentActionOclConverter(),
                        new PackageReferenceOclConverter(),
                        new PropertiesDictionaryOclConverter(),
                        new VcsRunbookPersistenceModelOclConverter()
                    }
                });
    }
}