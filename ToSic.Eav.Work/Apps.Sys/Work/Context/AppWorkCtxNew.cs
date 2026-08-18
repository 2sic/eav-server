using ToSic.Eav.DataSource;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppWorkCtxNew(IAppReader appReader, AppWorkContextService ctxSvc, bool? showDrafts = default) : IAppWorkCtxForDiWip
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
    public DbStorage DbStorage => field ??= ctxSvc.DbGenerator.New(new(AppReader));

}