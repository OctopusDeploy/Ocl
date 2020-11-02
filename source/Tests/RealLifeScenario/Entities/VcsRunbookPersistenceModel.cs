using System;

namespace Tests.RealLifeScenario.Entities
{
    public class VcsRunbookPersistenceModel
    {
        public VcsRunbookPersistenceModel(VcsRunbook runbook)
            => Runbook = runbook;

        public VcsRunbook Runbook { get; }

        public RunbookProcess? Process { get; set; }
    }
}