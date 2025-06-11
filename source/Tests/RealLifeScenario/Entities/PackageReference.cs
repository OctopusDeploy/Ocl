using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Octopus.Ocl.Converters;
using Octopus.Server.MessageContracts;
using Octopus.Server.MessageContracts.Features.Feeds;

namespace Tests.RealLifeScenario.Entities
{
    /// <summary>
    ///     Represents a reference from a deployment-process (specifically an action or action-template) to a package.
    ///     May be named or un-named.
    /// </summary>
    /// <history>
    ///     Prior to Octopus 2018.8, deployment actions could have at most a single package-reference. This was
    ///     captured as properties on the action (Octopus.Action.Package.PackageId, Octopus.Action.Package.FeedId, etc).
    ///     In 2018.8, we introduced support for actions with multiple packages (initially script steps and kubernetes step).
    ///     Storing collections of nested objects in the property-bag gets very messy, so package-references were moved into
    ///     their own class
    ///     and collection on the deployment actions.
    /// </history>
    public class PackageReference : IId
    {
        /// <summary>
        ///     Constructs a named package-reference.
        /// </summary>
        /// <param name="name">The package-reference name.</param>
        /// <param name="packageId">The package ID or a variable-expression</param>
        /// <param name="feedIdOrName">The feed ID or a variable-expression</param>
        /// <param name="acquisitionLocation">The location the package should be acquired</param>
        public PackageReference(string? name, string packageId, FeedIdOrName feedIdOrName, PackageAcquisitionLocation acquisitionLocation)
            : this(name, packageId, feedIdOrName, acquisitionLocation.ToString())
        {
        }

        /// <summary>
        ///     Constructs a named package-reference.
        /// </summary>
        /// <param name="name">The package-reference name.</param>
        /// <param name="packageId">The package ID or a variable-expression</param>
        /// <param name="feedIdOrName">The feed ID or a variable-expression</param>
        /// <param name="acquisitionLocation">The location the package should be acquired.
        /// May be one <see cref="PackageAcquisitionLocation" /> or a variable-expression.</param>
        public PackageReference(string? name, string packageId, FeedIdOrName feedIdOrName, string acquisitionLocation)
            : this(
                null,
                name,
                packageId,
                feedIdOrName,
                acquisitionLocation)
        {
        }

        /// <summary>
        ///     For JSON deserialization only
        /// </summary>
        [JsonConstructor]
        public PackageReference(string? id,
            string? name,
            string packageId,
            FeedIdOrName feedIdOrName,
            string acquisitionLocation)
            : this()
        {
            if (!string.IsNullOrEmpty(id)) Id = id;

            PackageId = packageId;
            FeedIdOrName = feedIdOrName;
            AcquisitionLocation = acquisitionLocation;
            Name = name ?? string.Empty;
        }

        /// <summary>
        ///     Constructs a primary package (an un-named package reference)
        /// </summary>
        public PackageReference(string packageId, FeedIdOrName feedIdOrName, PackageAcquisitionLocation acquisitionLocation)
            : this(null, packageId, feedIdOrName, acquisitionLocation)
        {
        }

        /// <summary>
        ///     Constructs a primary package (an un-named package reference)
        /// </summary>
        public PackageReference(string packageId, FeedIdOrName feedIdOrName, string acquisitionLocation)
            : this(null, packageId, feedIdOrName, acquisitionLocation)
        {
        }

        /// <summary>
        ///     Constructs a primary package (an un-named package reference)
        /// </summary>
        public PackageReference(string packageId, FeedIdOrName feedIdOrName)
            : this(packageId, feedIdOrName, PackageAcquisitionLocation.Server)
        {
        }

        /// <summary>
        ///     In the scenario where we are migrating from 2.6 instances, we set the ID of the package-reference
        ///     to the same ID as the deployment-action.
        ///     This was the least-bad option that let the migrator do its thing when migrating project
        ///     release-creation-strategy and versioning-strategy.
        /// </summary>
        public PackageReference(string id) : this()
        {
            if (!string.IsNullOrEmpty(id)) Id = id;
        }

        public PackageReference()
        {
            Id = Guid.NewGuid().ToString();
            Properties = new Dictionary<string, string>();
            FeedIdOrName = "feeds-builtin".ToFeedIdOrName();
        }

        public string Id { get; }

        /// <summary>
        ///     An name for the package-reference.
        ///     This may be empty.
        ///     This is used to discriminate the package-references. Package ID isn't suitable because an action may potentially
        ///     have multiple references to the same package ID (e.g. if you wanted to use different versions of the same package).
        ///     Also, the package ID may be a variable-expression.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Package ID or a variable-expression
        /// </summary>
        public string PackageId { get; set; } = string.Empty;

        /// <summary>
        ///     Feed ID, name or a variable-expression
        /// </summary>
        [JsonProperty("FeedId")] // This is named FeedId for backward-compatibility as we don't yet want to change the underlying database JSON/schema.
        [OclName("feed")]
        public FeedIdOrName FeedIdOrName { get; set; }

        /// <summary>
        ///     The package-acquisition location.
        ///     One of <see cref="PackageAcquisitionLocation" /> or a variable-expression
        /// </summary>
        public string AcquisitionLocation { get; set; } = string.Empty;

        /// <summary>
        ///    Specific version to use for this package. If not specified, package can be 
        ///    selected at release creation or runbook run time.
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        ///     This reference identifier is populated when a step package step contains a package reference
        ///     It allows us to correlate the reference within the step package inputs to this Server package reference
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? StepPackageInputsReferenceId { get; set; }

        /// <summary>
        /// This should never be null for new objects but there are a small number of customers with
        /// older databases that may have it as null.
        /// </summary>
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public IDictionary<string, string>? Properties { get; private set; }

        [JsonIgnore]
        public bool IsPrimaryPackage => Name == "";

        /// <summary>
        /// Convenience helper for setting a property using a fluent-builder style interface
        /// </summary>
        public PackageReference WithProperty(string propertyKey, string propertyValue)
        {
            Properties ??= new Dictionary<string, string>();

            Properties[propertyKey] = propertyValue;
            return this;
        }

        public PackageReference DeepClone() =>
            new(
                Id,
                Name,
                PackageId,
                FeedIdOrName,
                AcquisitionLocation)
            {
                Properties = Properties is null ? null : new Dictionary<string, string>(Properties),
                StepPackageInputsReferenceId = StepPackageInputsReferenceId,
                Version = Version
            };

        /// <summary>
        ///     Performs a case-insensitive comparison between the name of this package-reference and the
        ///     supplied name.  Nulls and empty strings are considered equal.
        /// </summary>
        public bool NameMatches(string name) =>
            Name == ""
                ? string.IsNullOrEmpty(name)
                : Name.Equals(name, StringComparison.OrdinalIgnoreCase);
    }
}
