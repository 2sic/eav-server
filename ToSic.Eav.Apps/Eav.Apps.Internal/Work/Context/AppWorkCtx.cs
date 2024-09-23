using ToSic.Eav.Apps.State;

namespace ToSic.Eav.Apps.Internal.Work;

/// <summary>
/// Context object for performing App modifications.
/// This should help us change all the Read/Parts etc. to be fully functional and not depend on a Parent object. 
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppWorkCtx : IAppWorkCtx
{
    /// <inheritdoc />
    public int ZoneId { get; }

    /// <inheritdoc />
    public int AppId { get; }

    public AppWorkCtx(IAppReader appState)
    {
        AppId = appState.AppId;
        ZoneId = appState.ZoneId;
        AppReader = appState;
    }

    public AppWorkCtx(IAppWorkCtx original, IAppReader appState = default)
    {
        if (original == null) throw new ArgumentException(@"Original must exist", nameof(original));
        AppId = appState?.AppId ?? original.AppId;
        ZoneId = appState?.ZoneId ?? original.ZoneId;
        AppReader = appState ?? original.AppReader;
    }

    public IAppReader AppReader { get; }

}
