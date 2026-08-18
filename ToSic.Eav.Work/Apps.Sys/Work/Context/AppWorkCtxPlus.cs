using ToSic.Eav.DataSource;
using ToSic.Eav.Services;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppWorkCtxPlus : AppWorkCtx, IAppWorkCtxPlus
{
    internal AppWorkCtxPlus(IDataSourcesService dsf, IAppReader appReader, bool? showDrafts, IDataSource? data) : base(appReader)
    {
        DataSourcesFactory = dsf;
        ShowDrafts = showDrafts;
        Data = data;
    }

    private AppWorkCtxPlus(AppWorkCtxPlus original, IDataSource data) : base(original)
    {
        DataSourcesFactory = original.DataSourcesFactory;
        ShowDrafts = original.ShowDrafts;
        Data = data;
    }

    public IAppWorkCtxPlus SpawnNewWithPresetData(IDataSource data)
        => new AppWorkCtxPlus(this, data);

    /// <summary>
    /// Temp solution to provide the data if it was not itself initialized.
    /// </summary>
    private IDataSourcesService DataSourcesFactory { get; }

    public IDataSource Data => field
        ??= DataSourcesFactory.CreateDefault(new DataSourceOptions
        {
            AppIdentityOrReader = AppReader.PureIdentity(),
            ShowDrafts = ShowDrafts,
        });


    public bool? ShowDrafts { get; }

}