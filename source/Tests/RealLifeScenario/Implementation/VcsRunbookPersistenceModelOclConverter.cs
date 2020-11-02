using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Octopus.Ocl;
using Octopus.Ocl.Converters;
using Tests.RealLifeScenario.Entities;

namespace Tests.RealLifeScenario.Implementation
{
    public class VcsRunbookPersistenceModelOclConverter : IOclConverter
    {
        readonly RunbookProcessOclConverter processConverter;
        readonly VcsRunbookOclConverter runbookConverter;

        public VcsRunbookPersistenceModelOclConverter()
        {
            runbookConverter = new VcsRunbookOclConverter();
            processConverter = new RunbookProcessOclConverter();
        }

        public bool CanConvert(Type type)
            => type == typeof(VcsRunbookPersistenceModel);

        public IEnumerable<IOclElement> ToElements(OclConversionContext context, string name, object value)
            => throw new NotSupportedException();

        public OclDocument ToDocument(OclConversionContext context, object obj)
        {
            var model = (VcsRunbookPersistenceModel)obj;

            var doc = runbookConverter.ToDocument(context, model.Runbook);
            if (model.Process != null)
            {
                var process = processConverter.ToDocument(context, model.Process);

                foreach (var child in process)
                    doc.Add(child);
            }

            return doc;
        }

        public object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue)
        {
            if (!(element is OclDocument doc))
                throw new OclException("Can only convert from OclDocument");

            var (process, runbookElements) = processConverter.FromDocument(context, doc);

            var runbook = runbookConverter.FromRemainingElements(context, runbookElements);

            return new VcsRunbookPersistenceModel(runbook)
            {
                Process = process
            };
        }

        class VcsRunbookOclConverter : DefaultBlockOclConverter
        {
            public override bool CanConvert(Type type)
                => throw new NotImplementedException("This converter is used directly");

            public VcsRunbook FromRemainingElements(OclConversionContext context, IReadOnlyList<IOclElement> runbookElements)
            {
                var name = runbookElements.OfType<OclAttribute>().First(r => r.Name == "name").Value?.ToString();
                var runbook = new VcsRunbook(name ?? "");
                var properties = GetProperties(typeof(VcsRunbook)).ToArray();
                SetProperties(context, runbookElements, runbook, properties);
                return runbook;
            }

            protected override IEnumerable<IOclElement> GetElements(object obj, OclConversionContext context)
                => base.GetElements(obj, context)
                    .OrderByThenAlpha(
                        nameof(VcsRunbook.Name),
                        nameof(VcsRunbook.Description)
                    );
        }

        internal class RunbookProcessOclConverter : DefaultBlockOclConverter
        {
            public override bool CanConvert(Type type)
                => type == typeof(RunbookProcess);

            protected override IEnumerable<PropertyInfo> GetProperties(Type type)
                => base.GetProperties(type).Where(ShouldSerialize);

            public (RunbookProcess process, IReadOnlyList<IOclElement> notFound) FromDocument(
                OclConversionContext context,
                OclDocument doc
            )
            {
                var runbook = new RunbookProcess("PLACEHOLDER", "PLACEHOLDER");

                var properties = GetProperties(typeof(RunbookProcess)).ToArray();
                var notFound = SetProperties(context, doc, runbook, properties);

                return (runbook, notFound);
            }

            internal static bool ShouldSerialize(PropertyInfo property)
            {
                switch (property.Name)
                {
                    case nameof(RunbookProcess.Id):
                    case nameof(RunbookProcess.SpaceId):
                    case nameof(RunbookProcess.OwnerId):
                    case nameof(RunbookProcess.IsFrozen):
                    case nameof(RunbookProcess.Version):
                        return false;
                }

                return property.GetCustomAttribute<JsonIgnoreAttribute>() == null;
            }
        }
    }
}