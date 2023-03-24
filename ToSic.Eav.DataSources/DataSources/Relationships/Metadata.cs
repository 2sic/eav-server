using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Get Metadata (metadata Entities) of the Entities coming into this DataSource
    /// </summary>
    /// <remarks>
    /// * Added in v12.10
    /// * Changed in v15.05 to use the [immutable convention](xref:NetCode.Conventions.Immutable)
    /// </remarks>
    [VisualQuery(
        NiceName = "Metadata",
        UiHint = "Get the item's metadata",
        Icon = Icons.OfferLocal,
        Type = DataSourceType.Lookup,
        NameId = "3ab4b010-2daa-4a7f-b882-635d2d9fa0a0",
        In = new[] { QueryConstants.InStreamDefaultRequired },
        DynamicOut = false,
        ConfigurationType = "d7858b36-1ef1-4c3d-b15c-c567b0d7bdd4",
        HelpLink = "https://r.2sxc.org/DsMetadata")]
    [InternalApi_DoNotUse_MayChangeWithoutNotice("WIP")]

    public class Metadata : MetadataDataSourceBase
    {
        /// <summary>
        /// Optional Type Name restriction to only get **Metadata** of this Content Type.
        /// </summary>
        [Configuration]
        public override string ContentTypeName => Configuration.GetThis();

        public Metadata(MyServices services) : base(services, $"{DataSourceConstants.LogPrefix}.MetaDt")
        {
        }

        protected override IEnumerable<IEntity> SpecificGet(IImmutableList<IEntity> originals, string typeName)
        {
            var getMdFunc = GetMetadataFunctionGenerator(typeName);

            return originals.SelectMany(getMdFunc);
        }

        /// <summary>
        /// Construct function for the get of the related items
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        [PrivateApi]
        private Func<IEntity, IEnumerable<IEntity>> GetMetadataFunctionGenerator(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return o => o.Metadata;

            return o => o.Metadata.OfType(typeName);
        }

    }
}
