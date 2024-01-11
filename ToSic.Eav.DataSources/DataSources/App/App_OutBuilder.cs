using ToSic.Eav.Apps.State;
using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.DataSource.Streams;
using static ToSic.Eav.DataSource.Internal.DataSourceConstants;

namespace ToSic.Eav.DataSources;

partial class App: IDataSourceReset
{
    #region Dynamic Out

    private readonly StreamDictionary _out;
    private bool _requiresRebuildOfOut = true;

    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    void IDataSourceReset.Reset() => _requiresRebuildOfOut = true;
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    protected void Reset() => _requiresRebuildOfOut = true;

    /// <inheritdoc/>
    public override IReadOnlyDictionary<string, IDataStream> Out
    {
        get
        {
            // Use pre-built if already ready and nothing changed RequiresRebuild
            if (!_requiresRebuildOfOut) return _out.AsReadOnly();

            // Parse config before we continue, as AppSwitch could be set now
            Configuration.Parse();

            // if the rebuilt is required because the app or zone are not default, then attach it first
            if (AppSwitch != 0 || ZoneSwitch != 0)
                AttachOtherDataSource();

            // now create all streams
            CreateAppOutWithAllStreams();
            _requiresRebuildOfOut = false;
            return _out.AsReadOnly();
        }
    }
    #endregion

    /// <summary>
    /// Create a stream for each data-type
    /// </summary>
    private StreamDictionary CreateAppOutWithAllStreams()
    {
        var l = Log.Fn<StreamDictionary>();
        IDataStream upstream;
        try
        {
            // auto-attach to cache of current system?
            if (!In.ContainsKey(StreamDefaultName))
                AttachOtherDataSource();
            upstream = In[StreamDefaultName];
        }
        catch (KeyNotFoundException)
        {
            throw new Exception(
                $"Trouble with the App DataSource - must have a Default In-Stream with name {StreamDefaultName}. It has {In.Count} In-Streams.");
        }

        var upstreamDataSource = upstream.Source;
        _out.Clear();
        _out.Add(StreamDefaultName, upstreamDataSource.Out[StreamDefaultName]);

        // now provide all data streams for all data types; only need the cache for the content-types list, don't use it as the source...
        // because the "real" source already applies filters like published
        var appState = AppState;
        var listOfTypes = appState.ContentTypes;
        var showDraftsForCacheKey = _services.UserPermissions.UserPermissions().UserMayEdit;
        var typeList = "";
        foreach (var contentType in listOfTypes)
        {
            var typeName = contentType.Name;
            if (typeName == StreamDefaultName || typeName.StartsWith("@") || _out.ContainsKey(typeName))
                continue;
            typeList += typeName + ",";

            var deferredStream = new DataStreamWithCustomCaching(
                Services.CacheService,
                () => new CacheInfoAppAndMore("AppTypeStream" + AppRootCacheKey.AppCacheKey(this), ((IAppStateInternal)appState).StateCache,
                    $"Name={typeName}&Drafts={showDraftsForCacheKey}&{nameof(WithAncestors)}={WithAncestors}"),
                this,
                typeName,
                () => BuildTypeStream(upstreamDataSource, typeName).List.ToImmutableList(),
                true,
                contentType.Scope);
            _out.Add(typeName, deferredStream);
        }

        return l.Return(_out, $"Added with drafts:{showDraftsForCacheKey} streams: {typeList}");
    }

    /// <summary>
    /// Build an EntityTypeFilter for this content-type to provide as a stream
    /// </summary>
    private EntityTypeFilter BuildTypeStream(IDataSource upstreamDataSource, string typeName)
    {
        var l = Log.Fn<EntityTypeFilter>($"..., ..., {typeName}");
        var ds = _services.DataSourceFactory.Create<EntityTypeFilter>(attach: upstreamDataSource,
            options: new DataSourceOptions(appIdentity: this, lookUp: Configuration.LookUpEngine));
        ds.TypeName = typeName;
        ds.AddDebugInfo(Guid, null); // tell the inner source that it has the same ID as this one, as we're pretending it's the same source
        return l.Return(ds);
    }
}