using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppWorkCtxNew(IAppReader appReader, AppWorkContextService ctxSvc) : IAppWorkCtxForDiWip
{
    /// <inheritdoc />
    public int ZoneId { get; } = appReader.ZoneId;

    /// <inheritdoc />
    public int AppId { get; } = appReader.AppId;

    public IAppReader AppReader { get; } = appReader;

    [field: AllowNull, MaybeNull]
    public DbStorage DbStorage => field ??= ctxSvc.DbGenerator.New(new(AppReader));

}