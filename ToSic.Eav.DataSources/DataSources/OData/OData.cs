using ToSic.Eav.DataSource.Sys;
using ToSic.Eav.Services;
using ToSic.Sys.OData;
using static ToSic.Eav.DataSource.DataSourceConstants;

namespace ToSic.Eav.DataSources;

/// <summary>
/// DataSource which applies OData system query options ($filter, $orderby, $top, $skip, $select) to the upstream stream.
/// It parses a raw OData query string and builds the corresponding ValueFilter/ValueSort pipeline.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[VisualQuery(
    NiceName = "OData",
    UiHint = "Filter and sort using OData options like $filter, $orderby, $top, $skip",
    Icon = DataSourceIcons.FilterList,
    Type = DataSourceType.Filter,
    NameId = "ToSic.Eav.DataSources.OData, ToSic.Eav.DataSources",
    In = [InStreamDefaultRequired],
    DynamicOut = false,
    Audience = Audience.Advanced,
    ConfigurationType = "|Config ToSic.Eav.DataSources.EntityIdFilter", // TODO: change
    HelpLink = "https://go.2sxc.org/DsOData")]
public sealed class OData : DataSourceBase
{
    #region Configuration

    /// <summary>
    /// Raw OData query string to apply, for example: "$filter=Title eq 'Hello'&$orderby=Created desc&$top=10".
    /// Can start with '?' and may contain any supported system options.
    /// </summary>
    [Configuration(Field = "EntityIds")] // TODO: change
    public string? ODataQueryString
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }

    #endregion

    #region Ctor / DI
    
    public OData(Dependencies parentServices, LazySvc<IDataSourcesService> dataSourceFactory)
        : base(parentServices, $"{DataSourceConstantsInternal.LogPrefix}.OData", connect: [parentServices, dataSourceFactory])
    {
        _engine = new ODataQueryEngine(dataSourceFactory.Value);
        ProvideOut(GetODataResult);
    }

    private readonly ODataQueryEngine _engine;

    #endregion

    private IImmutableList<IEntity> GetODataResult()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        Configuration.Parse();

        // Ensure we have an upstream
        var inList = TryGetIn();
        if (inList is null)
            return l.ReturnAsError(Error.TryGetInFailed());
        if (!inList.Any())
            return l.Return(inList, "empty");

        var raw = ODataQueryString;
        if (string.IsNullOrWhiteSpace(raw))
            return l.Return(inList, "no odata");

        // Parse OData options (safe against malformed % etc.)
        var options = SystemQueryOptionsParser.Parse(raw!);
        if (!options.RawAllSystem.Any())
            return l.Return(inList, "no system options");

        // Build query AST and pipeline, then apply skip/top during execution
        var query = UriQueryParser.Parse(options);

        // Use the upstream data source (not this) as the root for filtering/sorting
        var upstream = In[StreamDefaultName].Source;

        var result = _engine.Execute(upstream, query);
        return l.ReturnAsOk(result.Items.ToImmutableOpt());
    }
}
