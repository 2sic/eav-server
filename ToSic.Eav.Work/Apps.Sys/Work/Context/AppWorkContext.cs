using ToSic.Eav.DataSource;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppWorkContext(IAppReader appReader, AppWorkContextService ctxSvc, bool? showDrafts = default) : IAppWorkContext
{
    /// <inheritdoc />
    public int ZoneId { get; } = appReader.ZoneId;

    /// <inheritdoc />
    public int AppId { get; } = appReader.AppId;

    public IAppReader AppReader { get; } = appReader;

    public bool? ShowDrafts { get; } = showDrafts;

    public IDataSource Data => field
        ??= ctxSvc.DataSourcesSvc.Value.CreateDefault(new DataSourceOptions
        {
            AppIdentityOrReader = AppReader.PureIdentity(),
            ShowDrafts = ShowDrafts,
        });

    [field: AllowNull, MaybeNull]
    public DbStorage DbStorage => field ??= NewDbStorage();
    
    /// <inheritdoc />
    public DbStorage NewDbStorage() => ctxSvc.DbGenerator.New(new(AppReader));

    public IAppWorkContext FreshContext(IAppReader? freshReader = null) => ctxSvc.ContextNew(freshReader ?? AppReader);

}