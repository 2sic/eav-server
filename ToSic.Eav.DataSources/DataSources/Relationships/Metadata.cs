using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Get Metadata (metadata Entities) of the Entities coming into this DataSource
    /// </summary>
    /// <remarks>
    /// Added in v12.10
    /// </remarks>
    [VisualQuery(
        NiceName = "Metadata",
        UiHint = "Get the item's metadata",
        Icon = "local_offer",
        Type = DataSourceType.Lookup,
        GlobalName = "3ab4b010-2daa-4a7f-b882-635d2d9fa0a0",
        In = new[] { Constants.DefaultStreamNameRequired },
        DynamicOut = false,
        ExpectsDataOfType = "d7858b36-1ef1-4c3d-b15c-c567b0d7bdd4",
        HelpLink = "https://r.2sxc.org/DsMetadata")]
    [InternalApi_DoNotUse_MayChangeWithoutNotice("WIP")]

    public class Metadata : MetadataDataSourceBase
    {
        /// <inheritdoc/>
        [PrivateApi]
        public override string LogId => $"{DataSourceConstants.LogPrefix}.Child";

        /// <summary>
        /// TODO
        /// </summary>
        public override string ContentTypeName
        {
            get => Configuration[nameof(ContentTypeName)];
            set => Configuration[nameof(ContentTypeName)] = value;
        }

        protected override IEnumerable<IEntity> SpecificGet(IImmutableList<IEntity> originals, string typeName)
        {
            var find = InnerGet(typeName);

            return originals.SelectMany(o => find(o));
        }

        /// <summary>
        /// Construct function for the get of the related items
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        [PrivateApi]
        private Func<IEntity, IEnumerable<IEntity>> InnerGet(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return o => o.Metadata;

            return o => o.Metadata.OfType(typeName);
        }
    }
}
