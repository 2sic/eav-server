using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Get Target Entities (metadata targets) of the Entities coming into this DataSource
    /// </summary>
    /// <remarks>
    /// Added in v12.10
    /// </remarks>
    [VisualQuery(
        NiceName = "Metadata Targets",
        UiHint = "Get the item's targets (if they are metadata)",
        Icon = Icons.Metadata,
        Type = DataSourceType.Lookup,
        NameId = "afaf73d9-775c-4932-aebd-23e898b1643e",
        In = new[] { QueryConstants.InStreamDefaultRequired },
        DynamicOut = false,
        ConfigurationType = "7dcd26eb-a70c-4a4f-bb3b-5bd5da304232",
        HelpLink = "https://r.2sxc.org/DsMetadataTargets")]
    [InternalApi_DoNotUse_MayChangeWithoutNotice("WIP")]

    public class MetadataTargets: MetadataDataSourceBase
    {
        /// <summary>
        /// Optional TypeName restrictions to only get **Targets** of this Content Type.
        /// </summary>
        [Configuration]
        public override string ContentTypeName
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Defaults to true
        /// </remarks>
        [Configuration(Fallback = true)]
        public bool FilterDuplicates
        {
            get => Configuration.GetThis(true);
            set => Configuration.SetThis(value);
        }
        public MetadataTargets(IAppStates appStates, MyServices services) : base(services, $"{DataSourceConstants.LogPrefix}.MetaTg")
        {
            _appStates = appStates;
        }
        private readonly IAppStates _appStates;

        protected override IEnumerable<IEntity> SpecificGet(IImmutableList<IEntity> originals, string typeName)
        {
            var getTargetFunc = GetTargetsFunctionGenerator();

            var relationships = originals.SelectMany(getTargetFunc);

            if (FilterDuplicates) relationships = relationships.Distinct();

            if (typeName.HasValue())
                relationships = relationships.OfType(typeName);

            return relationships;
        }

        /// <summary>
        /// Construct function for the get of the related items
        /// </summary>
        /// <returns></returns>
        [PrivateApi]
        private Func<IEntity, IEnumerable<IEntity>> GetTargetsFunctionGenerator()
        {
            var appState = _appStates.Get(this);
            return o =>
            {
                var mdFor = o.MetadataFor;

                // The next block could maybe be re-used elsewhere...
                if (!mdFor.IsMetadata || mdFor.TargetType != (int)TargetTypes.Entity) return Enumerable.Empty<IEntity>();
                
                if (mdFor.KeyGuid != null) return new[] { appState.List.One(mdFor.KeyGuid.Value) };
                if (mdFor.KeyNumber != null) return new[] { appState.List.One(mdFor.KeyNumber.Value) };

                return Enumerable.Empty<IEntity>();
            };
        }

        //[PrivateApi]
        //protected override IEnumerable<IEntity> Postprocess(IEnumerable<IEntity> results) 
        //    => FilterDuplicates ? results.Distinct() : results;
    }
}
