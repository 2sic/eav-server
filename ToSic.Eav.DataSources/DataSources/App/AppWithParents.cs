using ToSic.Eav.Apps;
using ToSic.Eav.Services;

namespace ToSic.Eav.DataSources;

internal class AppWithParents: DataSourceBase
{
    private readonly IDataSourceGenerator<StreamMerge> _mergeGenerator;

    public override int AppId
    {
        get => _appId == 0 ? base.AppId : _appId;
        protected set => _appId = value;
    }

    public override int ZoneId
    {
        get => _zoneId == 0 ? base.ZoneId : _zoneId;
        protected set => _zoneId = value;
    }

    ///// <summary>
    ///// Indicates whether to show drafts or only Published Entities. 
    ///// </summary>
    //[Configuration(Fallback = QueryConstants.ShowDraftsDefault)]
    //public bool ShowDrafts
    //{
    //    get => Configuration.GetThis(QueryConstants.ShowDraftsDefault);
    //    set => Configuration.SetThis(value);
    //}


    private readonly IAppReaders _appReaders;
    private readonly IDataSourcesService _dataSourceFactory;
    private int _appId;
    private int _zoneId;

    public AppWithParents(MyServices services, IDataSourcesService dataSourceFactory, IAppReaders appReaders, IDataSourceGenerator<StreamMerge> mergeGenerator) : base(services, $"{DataSourceConstants.LogPrefix}.ApWPar")
    {
        ConnectLogs([
            _dataSourceFactory = dataSourceFactory,
            _appReaders = appReaders,
            _mergeGenerator = mergeGenerator
        ]);
        ProvideOut(GetList);
    }

    private IImmutableList<IEntity> GetList() => Log.Func(() =>
    {
        var appReader = _appReaders.GetReader(this);
            
        var initialSource = _dataSourceFactory.CreateDefault(new DataSourceOptions(appIdentity: appReader));
        var initialLink = initialSource.Link;

        // 2dm 2023-01-22 #maybeSupportIncludeParentApps
        var parentAppState = appReader.ParentAppState;
        var countRecursions = 0;
        while (parentAppState != null && countRecursions++ < 5)
        {
            var next = _dataSourceFactory.CreateDefault(new DataSourceOptions(appIdentity: parentAppState));
            initialLink = initialLink.Add(next.Link.Rename(inName: $"App{parentAppState.NameId}"));
            parentAppState = parentAppState.ParentApp?.AppState;
        }

        var merge = _mergeGenerator.New(attach: initialLink);

        return merge.Out.First().Value.List.ToImmutableList();
    });
}