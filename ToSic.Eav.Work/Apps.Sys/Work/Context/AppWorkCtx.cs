namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// Context object for performing App modifications.
/// This should help us change all the Read/Parts etc. to be fully functional and not depend on a Parent object. 
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppWorkCtx : IAppWorkCtx
{
    /// <inheritdoc />
    public int ZoneId { get; }

    /// <inheritdoc />
    public int AppId { get; }

    public IAppReader AppReader { get; }

    public AppWorkCtx(IAppReader appReader)
    {
        AppId = appReader.AppId;
        ZoneId = appReader.ZoneId;
        AppReader = appReader;
    }

    public AppWorkCtx(IAppWorkCtx original, IAppReader appReader = default)
    {
        if (original == null)
            throw new ArgumentException(@"Original must exist", nameof(original));
        AppId = appReader?.AppId ?? original.AppId;
        ZoneId = appReader?.ZoneId ?? original.ZoneId;
        AppReader = appReader ?? original.AppReader;
    }

}
