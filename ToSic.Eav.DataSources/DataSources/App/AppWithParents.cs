using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Services;

namespace ToSic.Eav.DataSources;

internal class AppWithParents: DataSourceBase
{
    private readonly IDataSourceGenerator<StreamMerge> _mergeGenerator;

    public override int AppId
    {
        get => field == 0 ? base.AppId : field;
        protected set => field = value;
    }

    public override int ZoneId
    {
        get => field == 0 ? base.ZoneId : field;
        protected set => field = value;
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


    private readonly IAppReaderFactory _appReaders;
    private readonly IDataSourcesService _dataSourceFactory;

    public AppWithParents(MyServices services, IDataSourcesService dataSourceFactory, IAppReaderFactory appReaders, IDataSourceGenerator<StreamMerge> mergeGenerator) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.ApWPar")
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
        var appReader = _appReaders.Get(this);
            
        var initialSource = _dataSourceFactory.CreateDefault(new DataSourceOptions
        {
            AppIdentityOrReader = appReader,
        });
        var initialLink = initialSource.Link;

        // 2dm 2023-01-22 #maybeSupportIncludeParentApps
        var parentAppState = appReader.GetParentCache();
        var countRecursions = 0;
        while (parentAppState != null && countRecursions++ < 5)
        {
            var next = _dataSourceFactory.CreateDefault(new DataSourceOptions
            {
                AppIdentityOrReader = parentAppState,
            });
            initialLink = initialLink.Add(next.Link.Rename(inName: $"App{parentAppState.NameId}"));
            parentAppState = parentAppState.ParentApp?.AppState;
        }

        var merge = _mergeGenerator.New(attach: initialLink);

        return merge.Out.First().Value.List.ToImmutableList();
    });
}