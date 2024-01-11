using ToSic.Eav.Apps;
using ToSic.Eav.DataSources.Internal;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using static ToSic.Eav.DataSource.Internal.DataSourceConstants;

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
    In = new[] { InStreamDefaultRequired },
    DynamicOut = false,
    ConfigurationType = "7dcd26eb-a70c-4a4f-bb3b-5bd5da304232",
    HelpLink = "https://go.2sxc.org/DsMetadataTargets")]
[InternalApi_DoNotUse_MayChangeWithoutNotice("WIP")]
public class MetadataTargets: MetadataDataSourceBase
{
    /// <summary>
    /// Optional TypeName restrictions to only get **Targets** of this Content Type.
    /// </summary>
    [Configuration]
    public override string ContentTypeName => Configuration.GetThis();

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Defaults to true
    /// </remarks>
    [Configuration(Fallback = true)]
    public bool FilterDuplicates => Configuration.GetThis(true);

    public MetadataTargets(IAppStates appStates, MyServices services) : base(services, $"{LogPrefix}.MetaTg")
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
        var appState = _appStates.GetReader(this);
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
}