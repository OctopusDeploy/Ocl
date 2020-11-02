using Tests.RealLifeScenario.ConverterStrategy.Implementation;
using Tests.RealLifeScenario.Entities;

namespace Tests.RealLifeScenario.ConverterStrategy
{
    public class RealLifeScenarioConverterStrategyFixture : RealLifeScenarioFixtureBase
    {
        protected override string Serialize(VcsRunbookPersistenceModel model)
            => new OclSerializerFactory()
                .Create()
                .Serialize(model);

        protected override VcsRunbookPersistenceModel Deserialize(string ocl)
            => new OclSerializerFactory()
                .Create()
                .Deserialize<VcsRunbookPersistenceModel>(ocl);
    }
}