using ToSic.Eav.DataSource;
using ToSic.Eav.Services;

namespace ToSic.Eav.Apps.Internal.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppWorkCtxPlus : AppWorkCtx, IAppWorkCtxPlus
{
    public AppWorkCtxPlus(IDataSourcesService dsf, IAppReader appReader, bool? showDrafts, IDataSource data = default) : base(appReader)
    {
        DataSourcesFactory = dsf;
        ShowDrafts = showDrafts;
        _data = data;
    }

    public AppWorkCtxPlus(IAppWorkCtx original, IDataSourcesService dsf = default, IAppReader appReader = default, bool? showDrafts = default, IDataSource data = default) : base(original, appReader)
    {
        var origOfClass = original as AppWorkCtxPlus;
        DataSourcesFactory = dsf ?? origOfClass?.DataSourcesFactory;
        ShowDrafts = showDrafts ?? (original as IAppWorkCtxPlus)?.ShowDrafts ?? origOfClass?.ShowDrafts;
        _data = data ?? origOfClass?._data;
    }

    public IAppWorkCtxPlus SpawnNewWithPresetData(IDataSource data)
        => new AppWorkCtxPlus(this, data: data);

    private IDataSourcesService DataSourcesFactory { get; }



    public IDataSource Data => _data
        ??= DataSourcesFactory.CreateDefault(new DataSourceOptions
        {
            AppIdentityOrReader = AppReader.PureIdentity(),
            ShowDrafts = ShowDrafts,
        });
    private IDataSource _data;


    public bool? ShowDrafts { get; }

}