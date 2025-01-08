using ToSic.Eav.Apps;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSources.Internal;
using ToSic.Eav.DataSources.Sys;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Helpers;
using static ToSic.Eav.DataSource.DataSourceConstants;

namespace ToSic.Eav.DataSources;

/// <summary>
/// Get Target Entities (metadata targets) of the Entities coming into this DataSource
/// </summary>
/// <remarks>
/// * Added in v12.10
/// * Changed in v15.05 to use the [immutable convention](xref:NetCode.Conventions.Immutable)
/// </remarks>
[VisualQuery(
    NiceName = "Metadata Targets",
    UiHint = "Get the item's targets (if they are metadata)",
    Icon = DataSourceIcons.Metadata,
    Type = DataSourceType.Lookup,
    NameId = "afaf73d9-775c-4932-aebd-23e898b1643e",
    In = [InStreamDefaultRequired],
    DynamicOut = false,
    ConfigurationType = "7dcd26eb-a70c-4a4f-bb3b-5bd5da304232",
    HelpLink = "https://go.2sxc.org/DsMetadataTargets")]
[InternalApi_DoNotUse_MayChangeWithoutNotice("WIP")]
public class MetadataTargets(DataSourceBase.MyServices services, IAppReaderFactory appReaders, IDataFactory dataFactory)
    : MetadataDataSourceBase(services, $"{DataSourceConstantsInternal.LogPrefix}.MetaTg", connect: [appReaders, dataFactory])
{
    /// <summary>
    /// Optional TypeName restrictions to only get **Targets** of this Content Type.
    /// </summary>
    [Configuration]
    public override string ContentTypeName => Configuration.GetThis();

    /// <summary>
    /// If it should filter duplicates. Default is true.
    /// </summary>
    [Configuration(Fallback = true)]
    public bool FilterDuplicates => Configuration.GetThis(true);

    protected override IEnumerable<IEntity> SpecificGet(IImmutableList<IEntity> originals, string typeName)
    {
        var getTargetFunc = GetTargetsFunctionGenerator();

        var relationships = originals.SelectMany(getTargetFunc);

        if (FilterDuplicates) relationships = relationships.Distinct();

        if (typeName.HasValue())
            relationships = relationships.OfType(typeName);

        return relationships;
    }

    private IDataFactory ContentTypeFactory => ctFactory.Get(() =>
    {
        var opts = ContentTypeUtil.Options with { AppId = AppId, WithMetadata = true };
        var x = dataFactory.New(options: opts);
        return x;
    });
    private GetOnce<IDataFactory> ctFactory = new();

    /// <summary>
    /// Construct function for the get of the related items
    /// </summary>
    /// <returns></returns>
    [PrivateApi]
    private Func<IEntity, IEnumerable<IEntity>> GetTargetsFunctionGenerator()
    {
        var appState = appReaders.Get(this);
        return o =>
        {
            var mdFor = o.MetadataFor;

            // If not Metadata, exit early
            if (!mdFor.IsMetadata)
                return [];
            
            // If for entities, retrieve them
            // We seem to have a historic setup where we sometimes use IDs and sometimes GUIDs?
            if (mdFor.TargetType == (int)TargetTypes.Entity)
            {
                if (mdFor.KeyGuid != null)
                    return [appState.List.One(mdFor.KeyGuid.Value)];
                if (mdFor.KeyNumber != null)
                    return [appState.List.One(mdFor.KeyNumber.Value)];
            }

            if (mdFor.TargetType == (int)TargetTypes.ContentType)
            {
                var key = mdFor.KeyString ?? mdFor.KeyGuid?.ToString();
                if (key == null) return [];
                var ct = appState.GetContentType(key);
                if (ct == null) return [];
                var ctEntity = ContentTypeFactory.Create(ContentTypeUtil.ToRaw(ct));
                return [ctEntity];
            }

            return [];
        };
    }
}